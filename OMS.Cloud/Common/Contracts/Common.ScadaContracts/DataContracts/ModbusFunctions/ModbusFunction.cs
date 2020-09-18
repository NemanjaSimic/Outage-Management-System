using OMS.Common.SCADA;
using System;
using System.Runtime.Serialization;

namespace OMS.Common.ScadaContracts.DataContracts.ModbusFunctions
{
    [DataContract]
    [KnownType(typeof(ReadFunction))]
    [KnownType(typeof(WriteSingleFunction))]
    [KnownType(typeof(WriteMultipleFunction))]
    public abstract class ModbusFunction : IModbusFunction
    {
        [DataMember]
        public ModbusFunctionCode FunctionCode { get; set; }

        protected ModbusFunction(ModbusFunctionCode functionCode)
        {
            FunctionCode = functionCode;
        }
    }
}