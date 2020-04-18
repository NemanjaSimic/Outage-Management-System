using EasyModbus;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using OMS.Cloud.SCADA.ModbusFunctions.Parameters;
using OMS.Cloud.SCADA.Data.Repository;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using OMS.Common.SCADA;
using OMS.Common.SCADA.FunctionParameters;
using OMS.Common.Cloud.WcfServiceFabricClients.SCADA;

namespace OMS.Cloud.SCADA.ModbusFunctions.Read
{
    public class ReadCoilsFunction : ModbusFunction, IReadDiscreteModbusFunction
    {
        public IModbusReadCommandParameters ModbusReadCommandParameters { get; private set; }

        public ReadCoilsFunction(ModbusCommandParameters commandParameters)
            : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
            ModbusReadCommandParameters = commandParameters as IModbusReadCommandParameters;
            Data = new Dictionary<long, DiscreteModbusData>();
        }

        #region IModBusFunction
        public Dictionary<long, DiscreteModbusData> Data { get; protected set; }

        public async override void Execute(ModbusClient modbusClient)
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

            bool[] data = new bool[0];

            try
            {
                if (modbusClient.Connected)
                {
                    data = modbusClient.ReadCoils(startAddress - 1, quantity);
                }
                else
                {
                    Logger.LogError("modbusClient is disconected ");
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Error on ReadCoils()", e);
                throw e;
            }

            Data = new Dictionary<long, DiscreteModbusData>(data.Length);

            ScadaModelReadAccessClient modelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            var currentSCADAModel = await modelReadAccessClient.GetGidToPointItemMap();
            var currentAddressToGidMap = await modelReadAccessClient.GetAddressToGidMap();
            var commandValuesCache = await modelReadAccessClient.GetCommandDescriptionCache();

            for (ushort i = 0; i < data.Length; i++)
            {
                ushort address = (ushort)(startAddress + i);
                ushort value = (ushort)(data[i] ? 1 : 0);

                //for commands enqueued during model update
                if (!currentAddressToGidMap[(ushort)PointType.DIGITAL_OUTPUT].ContainsKey(address))
                {
                    Logger.LogWarn($"ReadCoilsFunction execute => trying to read value on address {address}, Point type: {PointType.DIGITAL_OUTPUT}, which is not in the current SCADA Model.");
                    continue;
                }

                long gid = currentAddressToGidMap[(ushort)PointType.DIGITAL_OUTPUT][address];

                //for commands enqueued during model update
                if (!currentSCADAModel.ContainsKey(gid))
                {
                    Logger.LogWarn($"ReadCoilsFunction execute => trying to read value for measurement with gid: 0x{gid:X16}, which is not in the current SCADA Model.");
                    continue;
                }

                if (!(currentSCADAModel[gid] is DiscreteSCADAModelPointItem pointItem))
                {
                    string message = $"PointItem [Gid: 0x{gid:X16}] is not type DiscreteSCADAModelPointItem.";
                    Logger.LogError(message);
                    throw new Exception(message);
                }
                
                if (pointItem.CurrentValue != value)
                {
                    pointItem.CurrentValue = value;//TODO: Update na provideru
                    Logger.LogInfo($"Alarm for Point [Gid: 0x{pointItem.Gid:X16}, Address: {pointItem.Address}] set to {pointItem.Alarm}.");
                }


                CommandOriginType commandOrigin = CommandOriginType.OTHER_COMMAND;

                if (commandValuesCache.ContainsKey(gid) && commandValuesCache[gid].Value == value)
                {
                    commandOrigin = commandValuesCache[gid].CommandOrigin;
                    commandValuesCache.Remove(gid); //TODO: Update na provider-u
                    Logger.LogDebug($"[ReadCoilsFunction] Command origin of command address: {pointItem.Address} is set to {commandOrigin}.");
                }

                DiscreteModbusData digitalData = new DiscreteModbusData(value, pointItem.Alarm, gid, commandOrigin);
                Data.Add(gid, digitalData);
                //Logger.LogDebug($"ReadCoilsFunction execute => Current value: {value} from address: {address}, gid: 0x{gid:X16}.");
            }

            //Logger.LogDebug($"ReadCoilsFunction executed SUCCESSFULLY. StartAddress: {startAddress}, Quantity: {quantity}");
        }

        #endregion IModBusFunction

        #region Obsolete

        /// <inheritdoc/>
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

            if (response[7] == (byte)ModbusFunctionCode.READ_COILS)
            {
                int numberOfBytes = response[8];

                for (ushort i = 0; i < numberOfBytes; i++)   //petlja za bajtove
                {
                    for (ushort j = 0; j < 8; j++)          //petlja za svaki bit u bajtu
                    {
                        ushort value = (response[9 + i] & (byte)Math.Pow(2, j)) != 0 ? (byte)1 : (byte)0;

                        returnResponse.Add
                        (new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, (ushort)(mdb_read_comm_pars.StartAddress + i * 8 + j)), value);
                    }
                }
            }

            return returnResponse;
        }

        #endregion Obsolete
    }
}