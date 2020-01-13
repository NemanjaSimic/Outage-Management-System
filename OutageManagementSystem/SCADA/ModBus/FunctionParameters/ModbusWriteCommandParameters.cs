namespace Outage.SCADA.ModBus.FunctionParameters
{
    public class ModbusWriteCommandParameters : ModbusCommandParameters
    {
        private ushort outputAddress;
        private int value;

        public ModbusWriteCommandParameters(ushort length, byte functionCode, ushort outputAddress, int value)
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

        public int Value
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