using EasyModbus;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.SCADACommon;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Outage.SCADA.ModBus.ModbusFuntions
{
    public class ReadHoldingRegistersFunction : ModbusFunction, IReadAnalogModBusFunction
    {
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) 
            : base(commandParameters)
        {
            //TOOD: check?
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        #region IModBusFunction
        public int[] Data { get; protected set; }

        public override void Execute(ModbusClient modbusClient)
        {
            ModbusReadCommandParameters mdb_read_comm_pars = this.CommandParameters as ModbusReadCommandParameters;
            Data = modbusClient.ReadHoldingRegisters(mdb_read_comm_pars.StartAddress, mdb_read_comm_pars.Quantity);
            logger.LogDebug($"ReadHoldingRegistersFunction executed SUCCESSFULLY. StartAddress: {mdb_read_comm_pars.StartAddress}, Quantity: {mdb_read_comm_pars.Quantity}");
        }
        #endregion


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

            //TODO: debug log

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
        #endregion
    }
}