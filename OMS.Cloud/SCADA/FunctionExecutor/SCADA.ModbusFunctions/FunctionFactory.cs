using Outage.Common;
using OMS.Common.SCADA;
using SCADA.ModbusFunctions.Read;
using SCADA.ModbusFunctions.Parameters;
using SCADA.ModbusFunctions.Write;

namespace SCADA.ModbusFunctions
{
    public class FunctionFactory
    {
        public IReadModbusFunction CreateReadModbusFunction(ModbusReadCommandParameters commandParameters)
        {
            IReadModbusFunction modbusFunction;

            switch ((ModbusFunctionCode)commandParameters.FunctionCode)
            {
                case ModbusFunctionCode.READ_COILS:
                    modbusFunction = new ReadCoilsFunction(commandParameters);
                    break;

                case ModbusFunctionCode.READ_DISCRETE_INPUTS:
                    modbusFunction = new ReadDiscreteInputsFunction(commandParameters);
                    break;

                case ModbusFunctionCode.READ_INPUT_REGISTERS:
                    modbusFunction = new ReadInputRegistersFunction(commandParameters);
                    break;

                case ModbusFunctionCode.READ_HOLDING_REGISTERS:
                    modbusFunction = new ReadHoldingRegistersFunction(commandParameters);
                    break;

                default:
                    modbusFunction = null;
                    string message = $"CreateReadModbusFunction => Wrong function code {(ModbusFunctionCode)commandParameters.FunctionCode}.";
                    LoggerWrapper.Instance.LogError(message);
                    break;
            }

            return modbusFunction;
        }

        public IWriteModbusFunction CreateWriteModbusFunction(ModbusWriteCommandParameters commandParameters, CommandOriginType commandOrigin)
        {
            IWriteModbusFunction modbusFunction;

            switch ((ModbusFunctionCode)commandParameters.FunctionCode)
            {
                case ModbusFunctionCode.WRITE_SINGLE_COIL:
                    modbusFunction = new WriteSingleCoilFunction(commandParameters, commandOrigin);
                    break;

                case ModbusFunctionCode.WRITE_SINGLE_REGISTER:
                    modbusFunction = new WriteSingleRegisterFunction(commandParameters, commandOrigin);
                    break;

                default:
                    modbusFunction = null;
                    string message = $"CreateWriteModbusFunction => Wrong function code {(ModbusFunctionCode)commandParameters.FunctionCode}.";
                    LoggerWrapper.Instance.LogError(message);
                    break;
            }

            return modbusFunction;
        }
    }
}