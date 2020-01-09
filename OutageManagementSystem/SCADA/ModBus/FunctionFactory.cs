using EasyModbus;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADACommon;

namespace Outage.SCADA.ModBus
{
    public class FunctionFactory
    {
        public static ModbusFunction CreateModbusFunction(ModbusCommandParameters commandParameters)
        {
            switch ((ModbusFunctionCode)commandParameters.FunctionCode)
            {
                case ModbusFunctionCode.READ_COILS:
                    return new ReadCoilsFunction(commandParameters);

                case ModbusFunctionCode.READ_DISCRETE_INPUTS:
                    return new ReadDiscreteInputsFunction(commandParameters);

                case ModbusFunctionCode.READ_INPUT_REGISTERS:
                    return new ReadInputRegistersFunction(commandParameters);

                case ModbusFunctionCode.READ_HOLDING_REGISTERS:
                    return new ReadHoldingRegistersFunction(commandParameters);

                case ModbusFunctionCode.WRITE_SINGLE_COIL:
                    return new WriteSingleCoilFunction(commandParameters);

                case ModbusFunctionCode.WRITE_SINGLE_REGISTER:
                    return new WriteSingleRegisterFunction(commandParameters);

                default:
                    return null;
            }
        }
    }
}