using Outage.Common;
using OMS.Cloud.SCADA.ModbusFunctions.Parameters;
using OMS.Cloud.SCADA.ModbusFunctions.Read;
using OMS.Cloud.SCADA.ModbusFunctions.Write;
using OMS.Cloud.SCADA.Common;
using OMS.Cloud.SCADA.Data.Repository;
using System;

namespace OMS.Cloud.SCADA.ModbusFunctions
{
    public static class FunctionFactory
    {
        #region Static Members

        private static SCADAModel scadaModel = null;

        public static SCADAModel SCADAModel
        {
            set
            {
                if (scadaModel == null)
                {
                    scadaModel = value;
                }
            }
        }

        #endregion


        public static IReadModbusFunction CreateReadModbusFunction(ModbusReadCommandParameters commandParameters)
        {
            IReadModbusFunction modbusFunction;

            if(FunctionFactory.scadaModel == null)
            {
                string message = $"CreateReadModbusFunction => SCADA model is null.";
                LoggerWrapper.Instance.LogError(message);
                //TODO: InternalSCADAServiceException
                throw new Exception(message);
            }

            switch ((ModbusFunctionCode)commandParameters.FunctionCode)
            {
                case ModbusFunctionCode.READ_COILS:
                    modbusFunction = new ReadCoilsFunction(commandParameters, FunctionFactory.scadaModel);
                    break;

                case ModbusFunctionCode.READ_DISCRETE_INPUTS:
                    modbusFunction = new ReadDiscreteInputsFunction(commandParameters, FunctionFactory.scadaModel);
                    break;

                case ModbusFunctionCode.READ_INPUT_REGISTERS:
                    modbusFunction = new ReadInputRegistersFunction(commandParameters, FunctionFactory.scadaModel);
                    break;

                case ModbusFunctionCode.READ_HOLDING_REGISTERS:
                    modbusFunction = new ReadHoldingRegistersFunction(commandParameters, FunctionFactory.scadaModel);
                    break;

                default:
                    modbusFunction = null;
                    string message = $"CreateReadModbusFunction => Wrong function code {(ModbusFunctionCode)commandParameters.FunctionCode}.";
                    LoggerWrapper.Instance.LogError(message);
                    break;
            }

            return modbusFunction;
        }

        public static IWriteModbusFunction CreateWriteModbusFunction(ModbusWriteCommandParameters commandParameters, CommandOriginType commandOrigin)
        {
            IWriteModbusFunction modbusFunction;

            if (FunctionFactory.scadaModel == null)
            {
                string message = $"CreateWriteModbusFunction => SCADA model is null.";
                LoggerWrapper.Instance.LogError(message);
                //TODO: InternalSCADAServiceException
                throw new Exception(message);
            }

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