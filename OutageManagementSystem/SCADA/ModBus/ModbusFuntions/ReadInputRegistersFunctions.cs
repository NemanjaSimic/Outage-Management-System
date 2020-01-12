using EasyModbus;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Repository;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Outage.SCADA.ModBus.ModbusFuntions
{
    public class ReadInputRegistersFunction : ModbusFunction, IReadAnalogModBusFunction
    {
        public ReadInputRegistersFunction(ModbusCommandParameters commandParameters)
            : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        #region IModBusFunction

        public Dictionary<long, int> Data { get; protected set; }

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

            int[] data = modbusClient.ReadInputRegisters(startAddress, quantity);
            Data = new Dictionary<long, int>(data.Length);

            SCADAModel scadaModel = SCADAModel.Instance;

            for (ushort i = 0; i < quantity; i++)
            {
                ushort address = (ushort)(startAddress + i);
                int value = data[i];
                long gid = scadaModel.CurrentAddressToGidMap[PointType.ANALOG_INPUT][address];

                if (scadaModel.CurrentScadaModel.ContainsKey(gid))
                {
                    scadaModel.CurrentScadaModel[gid].CurrentValue = value;
                    Logger.LogDebug($"ReadInputRegistersFunction execute => Current value: {value} from address: {address}, gid: 0x{gid:X16}.");
                }

                Data.Add(gid, data[i]);
            }

            Logger.LogDebug($"ReadInputRegistersFunction executed SUCCESSFULLY. StartAddress: {startAddress}, Quantity: {quantity}");
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
            ModbusReadCommandParameters mdbrp = (ModbusReadCommandParameters)CommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> returnResponse = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == (byte)ModbusFunctionCode.READ_INPUT_REGISTERS)
            {
                int n = response[8] / 2;

                for (ushort i = 0; i < n; i++)
                {
                    byte[] array = new byte[2];

                    array[0] = response[9 + i * 2 + 1];
                    array[1] = response[9 + i * 2];

                    ushort value = BitConverter.ToUInt16(array, 0);
                    returnResponse.Add(new Tuple<PointType, ushort>(PointType.ANALOG_INPUT, (ushort)(mdbrp.StartAddress + i)), value);
                }
            }
            return returnResponse;
        }

        #endregion Obsolete
    }
}