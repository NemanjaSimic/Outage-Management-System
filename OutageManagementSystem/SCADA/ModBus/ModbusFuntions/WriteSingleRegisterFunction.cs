using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.SCADA_Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Outage.SCADA.ModBus.ModbusFuntions
{
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters mdb_write_comm_pars = this.CommandParameters as ModbusWriteCommandParameters;
            byte[] mdb_request = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdb_write_comm_pars.TransactionId)), 0, mdb_request, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdb_write_comm_pars.ProtocolId)), 0, mdb_request, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdb_write_comm_pars.Length)), 0, mdb_request, 4, 2);
            mdb_request[6] = mdb_write_comm_pars.UnitId;
            mdb_request[7] = mdb_write_comm_pars.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdb_write_comm_pars.OutputAddress)), 0, mdb_request, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mdb_write_comm_pars.Value)), 0, mdb_request, 10, 2);

            //TODO: debug log

            return mdb_request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters mdb_write_comm_pars = this.CommandParameters as ModbusWriteCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> returnResponse = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER)
            {
                byte[] array = new byte[2];

                array[0] = response[9];
                array[1] = response[8];

                ushort out_a = BitConverter.ToUInt16(array, 0);

                array[0] = response[11];
                array[1] = response[10];

                ushort value = BitConverter.ToUInt16(array, 0);

                returnResponse.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, out_a), value);
            }

            return returnResponse;
        }
    }
}