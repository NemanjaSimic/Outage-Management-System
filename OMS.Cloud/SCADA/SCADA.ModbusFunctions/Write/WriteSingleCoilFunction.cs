using EasyModbus;
using Outage.Common;
using OMS.Cloud.SCADA.ModbusFunctions.Parameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using OMS.Common.SCADA;
using OMS.Common.SCADA.FunctionParameters;

namespace OMS.Cloud.SCADA.ModbusFunctions.Write
{
    public class WriteSingleCoilFunction : ModbusFunction, IWriteModbusFunction
    {
        public CommandOriginType CommandOrigin { get; private set; }
        public IModbusWriteCommandParameters ModbusWriteCommandParameters { get; private set; }

        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters, CommandOriginType commandOrigin)
            : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
            CommandOrigin = commandOrigin;
            ModbusWriteCommandParameters = commandParameters as IModbusWriteCommandParameters;
        }


        #region IModBusFunction

        public override void Execute(ModbusClient modbusClient)
        {

            ModbusWriteCommandParameters mdb_write_comm_pars = this.CommandParameters as ModbusWriteCommandParameters;
            ushort outputAddress = mdb_write_comm_pars.OutputAddress;
            int value = mdb_write_comm_pars.Value;

            if (outputAddress >= ushort.MaxValue || outputAddress == ushort.MinValue)
            {
                string message = $"Address is out of bound. Output address: {outputAddress}.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            bool commandingValue;
            if (value == 0)
            {
                commandingValue = false;
            }
            else if (value == 1)
            {
                commandingValue = true;
            }
            else
            {
                throw new ArgumentException("Non-boolean value in write single coil command parameter.");
            }


            //TODO: Check does current scada model has the requested address, maybe let anyway
            modbusClient.WriteSingleCoil(outputAddress - 1, commandingValue);
            Logger.LogInfo($"WriteSingleCoilFunction executed SUCCESSFULLY. OutputAddress: {outputAddress}, Value: {commandingValue}");
        }

        #endregion IModBusFunction

        #region Obsolete

        /// <inheritdoc/>
        [Obsolete]
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

        /// <inheritdoc/>
        [Obsolete]
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

        #endregion Obsolete
    }
}