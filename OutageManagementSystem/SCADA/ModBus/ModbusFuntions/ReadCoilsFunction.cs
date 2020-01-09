using EasyModbus;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.SCADA_Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Outage.SCADA.ModBus.ModbusFuntions
{
    public class ReadCoilsFunction : ModbusFunction
    {
        public ReadCoilsFunction(ModbusCommandParameters commandParameters, ModbusClient modbusClient) 
            : base(commandParameters, modbusClient)
        {
            //TODO: check?
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        #region IModBusFunction
        public override void Execute()
        {
            ModbusReadCommandParameters mdb_read_comm_pars = this.CommandParameters as ModbusReadCommandParameters;
            bool[] data = ModbusClient.ReadCoils(mdb_read_comm_pars.StartAddress, mdb_read_comm_pars.Quantity);

            throw new NotImplementedException("NO RETURN VALUE");

            logger.LogDebug($"ReadCoilsFunction executed SUCCESSFULLY. StartAddress: {mdb_read_comm_pars.StartAddress}, Quantity: {mdb_read_comm_pars.Quantity}");
        }
        #endregion


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

            //TODO: debug log

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
        #endregion
    }
}