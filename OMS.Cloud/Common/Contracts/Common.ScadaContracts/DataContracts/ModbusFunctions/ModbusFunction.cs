using OMS.Common.SCADA;
using System;
using System.Runtime.Serialization;

namespace OMS.Common.ScadaContracts.DataContracts.ModbusFunctions
{
    [Serializable]
    [DataContract]
    public abstract class ModbusFunction : IModbusFunction
    {
        [DataMember]
        public ModbusFunctionCode FunctionCode { get; protected set; }

        protected ModbusFunction(ModbusFunctionCode functionCode)
        {
            FunctionCode = functionCode;
        }
    }
}