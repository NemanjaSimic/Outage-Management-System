using OMS.Common.SCADA.FunctionParameters;

namespace OMS.Cloud.SCADA.ModbusFunctions.Parameters
{
    public class ModbusReadCommandParameters : ModbusCommandParameters, IModbusReadCommandParameters
    {
        private ushort startAddress;
        private ushort quantity;

        public ModbusReadCommandParameters(ushort length, byte functionCode, ushort startAddress, ushort quantity)
                : base(length, functionCode)
        {
            StartAddress = startAddress;
            Quantity = quantity;
        }

        public ushort StartAddress
        {
            get
            {
                return startAddress;
            }

            private set
            {
                startAddress = value;
            }
        }

        public ushort Quantity
        {
            get
            {
                return quantity;
            }

            private set
            {
                quantity = value;
            }
        }
    }
}