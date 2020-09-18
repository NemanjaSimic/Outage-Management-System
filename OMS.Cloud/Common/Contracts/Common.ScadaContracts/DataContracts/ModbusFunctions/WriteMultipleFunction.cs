using OMS.Common.SCADA;
using System.Runtime.Serialization;
using System;
using OMS.Common.Cloud;

namespace OMS.Common.ScadaContracts.DataContracts.ModbusFunctions
{
    [DataContract]
    public class WriteMultipleFunction : ModbusFunction, IWriteMultipleFunction
    {
        [DataMember]
        public ushort StartAddress { get; set; }
        [DataMember]
        public int[] CommandValues { get; set; }
        [DataMember]
        public CommandOriginType CommandOrigin { get; set; }

        public WriteMultipleFunction(ModbusFunctionCode functionCode, ushort startAddress, int[] commandValues, CommandOriginType commandOrigin)
            : base(functionCode)
        {
            StartAddress = startAddress;
            CommandValues = commandValues;
            CommandOrigin = commandOrigin;
        }
    }
}