using OMS.Common.SCADA;
using System;
using System.Runtime.Serialization;

namespace OMS.Common.ScadaContracts.DataContracts.ModbusFunctions
{
    [DataContract]
    public class ReadFunction : ModbusFunction, IReadModbusFunction
    {
        [DataMember]
        public ushort StartAddress { get; set; }
        [DataMember]
        public ushort Quantity { get; set; }
        
        public ReadFunction(ModbusFunctionCode functionCode, ushort startAddress, ushort quantity)
            : base(functionCode)
        {
            StartAddress = startAddress;
            Quantity = quantity;
        }

    }
}