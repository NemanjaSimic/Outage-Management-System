namespace Outage.SCADA.ModBus.FunctionParameters
{
    public class ModbusWriteCommandParameters : ModbusCommandParameters
    {
        private ushort outputAddress;
        private ushort value;

        public ModbusWriteCommandParameters(ushort length, byte functionCode, ushort outputAddress, ushort value)
            : base(length, functionCode)
        {
            OutputAddress = outputAddress;
            Value = value;
        }

        public ushort OutputAddress
        {
            get
            {
                return outputAddress;
            }

            private set
            {
                outputAddress = value;
            }
        }

        public ushort Value
        {
            get
            {
                return value;
            }

            private set
            {
                this.value = value;
            }
        }
    }
}