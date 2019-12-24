using Outage.SCADA.SCADA_Common;

using Outage.SCADA.ModBus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.ModBus.ModbusFuntions
{
    public class WriteSingleCoilFunction : ModbusFunction
    {
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
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

            return mdb_request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters mdb_write_comm_pars = this.CommandParameters as ModbusWriteCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> returnResponse = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == (byte)ModbusFunctionCode.WRITE_SINGLE_COIL)
            {
                byte[] array = new byte[2];

                array[0] = response[9];
                array[1] = response[8];

                ushort out_a = BitConverter.ToUInt16(array, 0);

                array[0] = response[11];
                array[1] = response[10];

                ushort value = BitConverter.ToUInt16(array, 0);

                returnResponse.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, out_a), value);
            }

            return returnResponse;
        }
    }
}
