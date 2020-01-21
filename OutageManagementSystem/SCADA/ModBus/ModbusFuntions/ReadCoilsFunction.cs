using EasyModbus;
using Outage.Common.PubSub.SCADADataContract;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Repository;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Outage.SCADA.ModBus.ModbusFuntions
{
    public class ReadCoilsFunction : ModbusFunction, IReadDiscreteModbusFunction
    {
        public ReadCoilsFunction(ModbusCommandParameters commandParameters)
            : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        #region IModBusFunction
        public Dictionary<long, DiscreteModbusData> Data { get; protected set; }

        public override void Execute(ModbusClient modbusClient)
        {
            ModbusReadCommandParameters mdb_read_comm_pars = this.CommandParameters as ModbusReadCommandParameters;
            ushort startAddress = mdb_read_comm_pars.StartAddress;
            ushort quantity = mdb_read_comm_pars.Quantity;

            if (startAddress + quantity >= ushort.MaxValue || startAddress + quantity == ushort.MinValue || startAddress == ushort.MinValue)
            {
                string message = $"Address is out of bound. Start address: {startAddress}, Quantity: {quantity}";
                Logger.LogError(message);
                throw new Exception(message);
            }

            bool[] data = modbusClient.ReadCoils(startAddress - 1, quantity);
            Data = new Dictionary<long, DiscreteModbusData>(data.Length);

            SCADAModel scadaModel = SCADAModel.Instance;

            for (ushort i = 0; i < quantity; i++)
            {
                ushort address = (ushort)(startAddress + i);
                ushort value = (ushort)(data[i] ? 1 : 0);
                long gid = scadaModel.CurrentAddressToGidMap[PointType.DIGITAL_OUTPUT][address];

                if (scadaModel.CurrentScadaModel.ContainsKey(gid))
                {
                    DiscreteSCADAModelPointItem pointItem = scadaModel.CurrentScadaModel[gid] as DiscreteSCADAModelPointItem;

                    if(pointItem == null)
                    {
                        string message = $"PointItem [Gid: 0x{gid:X16}] is not type DiscreteSCADAModelPointItem.";
                        Logger.LogError(message);
                        throw new Exception(message);
                    }

                    pointItem.CurrentValue = value;

                    bool alarmChanged = pointItem.SetAlarms();
                    if (alarmChanged)
                    {
                        Logger.LogInfo($"Alarm for Point [Gid: 0x{pointItem.Gid:X16}, Address: {pointItem.Address}] set to {pointItem.Alarm}.");
                    }

                    DiscreteModbusData digitalData = new DiscreteModbusData(value, pointItem.Alarm);
                    Data.Add(gid, digitalData);
                    Logger.LogDebug($"ReadCoilsFunction execute => Current value: {value} from address: {address}, gid: 0x{gid:X16}.");
                }
            }

            Logger.LogDebug($"ReadCoilsFunction executed SUCCESSFULLY. StartAddress: {startAddress}, Quantity: {quantity}");
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