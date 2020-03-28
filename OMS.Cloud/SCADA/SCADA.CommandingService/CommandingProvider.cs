using OMS.Cloud.SCADA.Data.Repository;
using OMS.Cloud.SCADA.ModbusFunctions;
using OMS.Cloud.SCADA.ModbusFunctions.Parameters;
using OMS.Common.SCADA;
using Outage.Common;
using Outage.Common.Exceptions.SCADA;
using System;
using System.Text;

namespace OMS.Cloud.SCADA.CommandingService
{
    public class CommandingProvider
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        #region Static Members

        protected static IWriteCommandEnqueuer writeCommandEnqueuer = null;

        public static IWriteCommandEnqueuer WriteCommandEnqueuer
        {
            set
            {
                if (writeCommandEnqueuer == null)
                {
                    writeCommandEnqueuer = value;
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

        public bool SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType)
        {
            bool success;

            if (CommandingProvider.scadaModel == null)
            {
                string message = $"SendAnalogCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            var currentScadaModel = CommandingProvider.scadaModel.CurrentScadaModel;

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
                    success = SendCommand(pointItem, modbusValue, commandOriginType);
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

        public bool SendDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType)
        {
            bool success;

            if (CommandingProvider.scadaModel == null)
            {
                string message = $"SendDiscreteCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            var currentScadaModel = CommandingProvider.scadaModel.CurrentScadaModel;

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
                    success = SendCommand(pointItem, commandingValue, commandOriginType);
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

        private bool SendCommand(ISCADAModelPointItem pointItem, object commandingValue, CommandOriginType commandOriginType)
        {
            bool success;
            ushort length = 6;
            ModbusWriteCommandParameters modbusWriteCommandParams;
            StringBuilder sb = new StringBuilder();

            if (CommandingProvider.writeCommandEnqueuer == null)
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

                IWriteModbusFunction modbusFunction = FunctionFactory.CreateWriteModbusFunction(modbusWriteCommandParams, commandOriginType);
                success = CommandingProvider.writeCommandEnqueuer.EnqueueWriteCommand(modbusFunction);

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
