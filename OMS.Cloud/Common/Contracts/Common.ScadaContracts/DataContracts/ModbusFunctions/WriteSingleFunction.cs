using OMS.Common.SCADA;
using System.Runtime.Serialization;
using System;
using OMS.Common.Cloud;

namespace OMS.Common.ScadaContracts.DataContracts.ModbusFunctions
{
    [DataContract]
    public class WriteSingleFunction : ModbusFunction, IWriteSingleFunction
    {
        [DataMember]
        public ushort OutputAddress { get; set; }
        [DataMember]
        public int CommandValue { get; set; }
        [DataMember]
        public CommandOriginType CommandOrigin { get; set; }

        public WriteSingleFunction(ModbusFunctionCode functionCode, ushort outputAddress, int commandValue, CommandOriginType commandOrigin)
            : base(functionCode)
        {
            OutputAddress = outputAddress;
            CommandValue = commandValue;
            CommandOrigin = commandOrigin;
        }
    }
}