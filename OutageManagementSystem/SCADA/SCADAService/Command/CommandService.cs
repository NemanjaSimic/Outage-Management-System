using System;
using System.Text;
using Outage.Common;
using Outage.Common.Exceptions.SCADA;
using Outage.Common.ServiceContracts.SCADA;
using Outage.SCADA.ModBus;
using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Repository;

namespace Outage.SCADA.SCADAService.Command
{
    public class CommandService : ISCADACommand
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        #region Static Members

        protected static FunctionExecutor functionExecutor = null;

        public static FunctionExecutor FunctionExecutor
        {
            set
            {
                if (functionExecutor == null)
                {
                    functionExecutor = value;
                }
            }
        }

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




        public bool SendAnalogCommand(long gid, float commandingValue)
        {
            bool success;

            if(CommandService.scadaModel == null)
            {
                string message = $"SendAnalogCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            var currentScadaModel = CommandService.scadaModel.CurrentScadaModel;

            if (!currentScadaModel.ContainsKey(gid))
            {
                string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            ISCADAModelPointItem pointItem = currentScadaModel[gid];

            if (pointItem is IAnalogSCADAModelPointItem analogPointItem && pointItem.RegisterType == PointType.ANALOG_OUTPUT)
            {
                try
                {
                    int modbusValue = analogPointItem.EguToRawValueConversion(commandingValue);
                    success = SendCommand(pointItem, modbusValue);
                }
                catch (Exception e)
                {
                    string message = $"Exception in SendAnalogCommand() method.";
                    Logger.LogError(message, e);
                    throw new InternalSCADAServiceException(message, e);
                }
            }
            else
            {
                string message = $"Either RegistarType of entity with gid: 0x{gid:X16} is not ANALOG_OUTPUT or entity does not implement IAnalogSCADAModelPointItem interface.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }



            return success;
        }

        public bool SendDiscreteCommand(long gid, ushort commandingValue)
        {
            bool success;

            if (CommandService.scadaModel == null)
            {
                string message = $"SendDiscreteCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            var currentScadaModel = CommandService.scadaModel.CurrentScadaModel;

            if (!currentScadaModel.ContainsKey(gid))
            {
                string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            ISCADAModelPointItem pointItem = currentScadaModel[gid];

            if (pointItem is IDiscreteSCADAModelPointItem && pointItem.RegisterType == PointType.DIGITAL_OUTPUT)
            {
                try
                {
                    success = SendCommand(pointItem, commandingValue);
                }
                catch (Exception e)
                {
                    string message = $"Exception in SendDiscreteCommand() method.";
                    Logger.LogError(message, e);
                    throw new InternalSCADAServiceException(message, e);
                }
            }
            else
            {
                string message = $"RegistarType of entity with gid: 0x{gid:X16} is not DIGITAL_OUTPUT or entity does not implement IDiscreteSCADAModelPointItem interface.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            return success;
        }

        private bool SendCommand(ISCADAModelPointItem pointItem, object commandingValue)
        {
            bool success;
            ushort length = 6;
            ModbusWriteCommandParameters modbusWriteCommandParams;
            StringBuilder sb = new StringBuilder();

            if(CommandService.functionExecutor == null)
            {
                string message = $"SendCommand => Function Executor is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            try
            {
                if (pointItem.RegisterType == PointType.ANALOG_OUTPUT && commandingValue is int analogCommandingValue)
                {
                    modbusWriteCommandParams = new ModbusWriteCommandParameters(length,
                                                                                (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER,
                                                                                pointItem.Address,
                                                                                analogCommandingValue);

                    sb.AppendLine($"WRITE_SINGLE_REGISTER command parameters created. Commanding value: {analogCommandingValue}");
                }
                else if (pointItem.RegisterType == PointType.DIGITAL_OUTPUT && commandingValue is ushort discreteCommandingValue)
                {
                    modbusWriteCommandParams = new ModbusWriteCommandParameters(length,
                                                                                (byte)ModbusFunctionCode.WRITE_SINGLE_COIL,
                                                                                pointItem.Address,
                                                                                discreteCommandingValue);

                    sb.AppendLine($"WRITE_SINGLE_COIL command parameters created. Commanding value: {discreteCommandingValue}");
                }
                else
                {
                    modbusWriteCommandParams = null;
                    string message = $"Commanding arguments are not valid. Registry type: {pointItem.RegisterType}, value type: {commandingValue.GetType()}";
                    Logger.LogError(message);
                    throw new ArgumentException(message);
                }

                ModbusFunction modbusFunction = FunctionFactory.CreateModbusFunction(modbusWriteCommandParams);
                success = CommandService.functionExecutor.EnqueueCommand(modbusFunction);

                if (success)
                {
                    sb.AppendLine("Command SUCCESSFULLY enqueued.");
                }
                else
                {
                    sb.AppendLine("Command enqueuing FAILED.");
                }

                Logger.LogInfo(sb.ToString());
            }
            catch (Exception e)
            {
                string message = $"Exception in SendCommand() method.";
                Logger.LogError(message, e);
                throw new InternalSCADAServiceException(message, e);
            }

            return success;
        }
    }
}