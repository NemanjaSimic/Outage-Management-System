using EasyModbus;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using OMS.Cloud.SCADA.ModbusFunctions.Parameters;
using OMS.Cloud.SCADA.Common;
using OMS.Cloud.SCADA.Common.FunctionParameters;
using OMS.Cloud.SCADA.Data.Repository;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace OMS.Cloud.SCADA.ModbusFunctions.Read
{
    public class ReadHoldingRegistersFunction : ModbusFunction, IReadAnalogModusFunction
    {
        public SCADAModel SCADAModel { get; private set; }
        public IModbusReadCommandParameters ModbusReadCommandParameters { get; private set; }

        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters, SCADAModel scadaModel)
            : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
            SCADAModel = scadaModel;
            ModbusReadCommandParameters = commandParameters as IModbusReadCommandParameters;
            Data = new Dictionary<long, AnalogModbusData>();
        }

        #region IModBusFunction

        public Dictionary<long, AnalogModbusData> Data { get; protected set; }

        public override void Execute(ModbusClient modbusClient)
        {
            ModbusReadCommandParameters mdb_read_comm_pars = this.CommandParameters as ModbusReadCommandParameters;
            ushort startAddress = mdb_read_comm_pars.StartAddress;
            ushort quantity = mdb_read_comm_pars.Quantity;

            if (quantity <= 0)
            {
                string message = $"Reading Quantity: {quantity} does not make sense.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            if (startAddress + quantity >= ushort.MaxValue || startAddress + quantity == ushort.MinValue || startAddress == ushort.MinValue)
            {
                string message = $"Address is out of bound. Start address: {startAddress}, Quantity: {quantity}";
                Logger.LogError(message);
                throw new Exception(message);
            }

            int[] data = new int[0];

            try
            {
                if (modbusClient.Connected)
                {
                    data = modbusClient.ReadHoldingRegisters(startAddress - 1, quantity);
                }
                else
                {
                    Logger.LogError("modbusClient is disconected ");
                }
            }
            catch(Exception e)
            {
                Logger.LogError("Error on ReadHoldingRegisters()", e);
                throw e;
            }

            Data = new Dictionary<long, AnalogModbusData>(data.Length);

            var currentAddressToGidMap = SCADAModel.CurrentAddressToGidMap;
            var currentSCADAModel = SCADAModel.CurrentScadaModel;
            var commandValuesCache = SCADAModel.CommandedValuesCache;

            for (ushort i = 0; i < data.Length; i++)
            {
                ushort address = (ushort)(startAddress + i);
                int rawValue = data[i];

                //for commands enqueued during model update
                if (!currentAddressToGidMap[PointType.ANALOG_OUTPUT].ContainsKey(address))
                {
                    Logger.LogWarn($"ReadHoldingRegistersFunction execute => trying to read value on address {address}, Point type: {PointType.ANALOG_OUTPUT}, which is not in the current SCADA Model.");
                    continue;
                }

                long gid = currentAddressToGidMap[PointType.ANALOG_OUTPUT][address];

                //for commands enqueued during model update
                if (!currentSCADAModel.ContainsKey(gid))
                {
                    Logger.LogWarn($"ReadHoldingRegistersFunction execute => trying to read value for measurement with gid: 0x{gid:X16}, which is not in the current SCADA Model.");
                    continue;
                }

                if (!(currentSCADAModel[gid] is AnalogSCADAModelPointItem pointItem))
                {
                    string message = $"PointItem [Gid: 0x{gid:X16}] is not type AnalogSCADAModelPointItem.";
                    Logger.LogError(message);
                    throw new Exception(message);
                }


                float eguValue = pointItem.RawToEguValueConversion(rawValue);
                if(pointItem.CurrentEguValue != eguValue)
                {
                    pointItem.CurrentEguValue = eguValue;
                    Logger.LogInfo($"Alarm for Point [Gid: 0x{pointItem.Gid:X16}, Address: {pointItem.Address}] set to {pointItem.Alarm}.");
                }

                CommandOriginType commandOrigin = CommandOriginType.OTHER_COMMAND;

                if (commandValuesCache.ContainsKey(gid) && commandValuesCache[gid].Value == pointItem.CurrentRawValue)
                {
                    commandOrigin = commandValuesCache[gid].CommandOrigin;
                    commandValuesCache.Remove(gid);
                    Logger.LogDebug($"[ReadHoldingRegistersFunction] Command origin of command address: {pointItem.Address} is set to {commandOrigin}.");
                }

                AnalogModbusData digitalData = new AnalogModbusData(pointItem.CurrentEguValue, pointItem.Alarm, gid, commandOrigin);
                Data.Add(gid, digitalData);
                //Logger.LogDebug($"ReadHoldingRegistersFunction execute => Current value: {pointItem.CurrentEguValue} from address: {address}, gid: 0x{gid:X16}.");
            }

            //Logger.LogDebug($"ReadHoldingRegistersFunction executed SUCCESSFULLY. StartAddress: {startAddress}, Quantity: {quantity}");
        }

        #endregion IModBusFunction

        #region Obsolete

        /// <inheritdoc />
        [Obsolete]
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters mdb_read_comm_pars = this.CommandParameters as ModbusReadCommandParameters;
            byte[] mdb_request = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdb_read_comm_pars.TransactionId)), 0, mdb_request, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdb_read_comm_pars.ProtocolId)), 0, mdb_request, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdb_read_comm_pars.Length)), 0, mdb_request, 4, 2);
            mdb_request[6] = mdb_read_comm_pars.UnitId;
            mdb_request[7] = mdb_read_comm_pars.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdb_read_comm_pars.StartAddress)), 0, mdb_request, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdb_read_comm_pars.Quantity)), 0, mdb_request, 10, 2);

            return mdb_request;
        }

        /// <inheritdoc />
        [Obsolete]
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters mdb_read_comm_pars = this.CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> returnResponse = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == (byte)ModbusFunctionCode.READ_HOLDING_REGISTERS)
            {
                int numberOfBytes = response[8] / 2;

                for (ushort i = 0; i < numberOfBytes; i++)
                {
                    byte[] array = new byte[2];

                    array[0] = response[9 + i * 2 + 1];
                    array[1] = response[9 + i * 2];

                    ushort value = BitConverter.ToUInt16(array, 0);

                    returnResponse.Add
                    (new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, (ushort)(mdb_read_comm_pars.StartAddress + i)), value);
                }
            }

            return returnResponse;
        }

        #endregion Obsolete
    }
}