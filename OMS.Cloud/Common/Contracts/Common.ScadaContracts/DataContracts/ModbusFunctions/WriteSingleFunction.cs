using Outage.Common;
using OMS.Common.SCADA;
using System.Runtime.Serialization;
using System;

namespace OMS.Common.ScadaContracts.DataContracts.ModbusFunctions
{
    [Serializable]
    [DataContract]
    public class WriteSingleFunction : ModbusFunction, IWriteSingleFunction
    {
        [DataMember]
        public ushort OutputAddress { get; protected set; }
        [DataMember]
        public int CommandValue { get; protected set; }
        [DataMember]
        public CommandOriginType CommandOrigin { get; protected set; }

        public WriteSingleFunction(ModbusFunctionCode functionCode, ushort outputAddress, int commandValue, CommandOriginType commandOrigin)
            : base(functionCode)
        {
            OutputAddress = outputAddress;
            CommandValue = commandValue;
            CommandOrigin = commandOrigin;
        }
    }
}