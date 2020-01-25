using Outage.Common;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Repository;
using System;

namespace Outage.SCADA.ModBus
{
    public class FunctionFactory
    {
        #region Static Members

        protected static SCADAModel scadaModel = null;

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


        public static ModbusFunction CreateModbusFunction(ModbusCommandParameters commandParameters)
        {
            ModbusFunction modbusFunction;

            if(FunctionFactory.scadaModel == null)
            {
                string message = $"CreateModbusFunction => SCADA model is null.";
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

                case ModbusFunctionCode.WRITE_SINGLE_COIL:
                    modbusFunction = new WriteSingleCoilFunction(commandParameters);
                    break;

                case ModbusFunctionCode.WRITE_SINGLE_REGISTER:
                    modbusFunction = new WriteSingleRegisterFunction(commandParameters);
                    break;

                default:
                    modbusFunction = null;
                    break;
            }

            return modbusFunction;
        }
    }
}