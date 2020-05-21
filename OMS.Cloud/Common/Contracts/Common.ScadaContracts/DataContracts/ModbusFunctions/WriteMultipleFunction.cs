using Outage.Common;
using OMS.Common.SCADA;
using System.Runtime.Serialization;
using System;

namespace OMS.Common.ScadaContracts.DataContracts.ModbusFunctions
{
    [Serializable]
    [DataContract]
    public class WriteMultipleFunction : ModbusFunction, IWriteMultipleFunction
    {
        [DataMember]
        public ushort StartAddress { get; protected set; }
        [DataMember]
        public int[] CommandValues { get; protected set; }
        [DataMember]
        public CommandOriginType CommandOrigin { get; protected set; }

        public WriteMultipleFunction(ModbusFunctionCode functionCode, ushort startAddress, int[] commandValues, CommandOriginType commandOrigin)
            : base(functionCode)
        {
            StartAddress = startAddress;
            CommandValues = commandValues;
            CommandOrigin = commandOrigin;
        }
    }
}