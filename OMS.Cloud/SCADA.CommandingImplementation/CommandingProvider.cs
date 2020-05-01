﻿using OMS.Common.SCADA;
using Outage.Common;
using Outage.Common.Exceptions.SCADA;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using OMS.Common.ScadaContracts;
using SCADA.ModbusFunctions;
using OMS.Common.ScadaContracts.DataContracts;
using SCADA.ModbusFunctions.Parameters;
using OMS.Common.Cloud.WcfServiceFabricClients.SCADA;

namespace SCADA.CommandingImplementation
{
    public class CommandingProvider : IScadaCommandingContract
    {
        private readonly FunctionFactory functionFactory;
        private WriteCommandEnqueuerClient commandEnqueuerClient;

        private ILogger logger;
        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }


        public CommandingProvider()
        {
            this.functionFactory = new FunctionFactory();
            this.commandEnqueuerClient = WriteCommandEnqueuerClient.CreateClient();
        }

        public async Task SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType)
        {
            Dictionary<long, IScadaModelPointItem> currentScadaModel = new Dictionary<long, IScadaModelPointItem>(); //TODO: get from scada access client

            if (currentScadaModel == null)
            {
                string message = $"SendAnalogCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            if (!currentScadaModel.ContainsKey(gid))
            {
                string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            IScadaModelPointItem pointItem = currentScadaModel[gid];

            if (pointItem is IAnalogPointItem analogPointItem && pointItem.RegisterType == PointType.ANALOG_OUTPUT)
            {
                try
                {
                    int modbusValue = analogPointItem.EguToRawValueConversion(commandingValue);
                    await SendCommand(pointItem, modbusValue, commandOriginType);
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
        }

        public async Task SendDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType)
        {

            Dictionary<long, IScadaModelPointItem> currentScadaModel = new Dictionary<long, IScadaModelPointItem>(); //TODO: get from scada access client
            if (currentScadaModel == null)
            {
                string message = $"SendDiscreteCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            if (!currentScadaModel.ContainsKey(gid))
            {
                string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            IScadaModelPointItem pointItem = currentScadaModel[gid];

            if (pointItem is IDiscretePointItem && pointItem.RegisterType == PointType.DIGITAL_OUTPUT)
            {
                try
                {
                    await SendCommand(pointItem, commandingValue, commandOriginType);
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


        }

        private async Task SendCommand(IScadaModelPointItem pointItem, object commandingValue, CommandOriginType commandOriginType, bool isRetry = false)
        {
            ushort length = 6;
            ModbusWriteCommandParameters modbusWriteCommandParams;
            StringBuilder sb = new StringBuilder();

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

                IWriteModbusFunction modbusFunction = functionFactory.CreateWriteModbusFunction(modbusWriteCommandParams, commandOriginType);
                await this.commandEnqueuerClient.EnqueueWriteCommand(modbusFunction);

                sb.AppendLine("Command SUCCESSFULLY enqueued.");
                Logger.LogInfo(sb.ToString());
            }
            catch (Exception e)
            {
                if (!isRetry)
                {
                    this.commandEnqueuerClient = WriteCommandEnqueuerClient.CreateClient();
                    await SendCommand(pointItem, commandingValue, commandOriginType, true);
                }
                else
                {
                    string message = $"Exception in SendCommand() method.";
                    Logger.LogError(message, e);
                    throw new InternalSCADAServiceException(message, e);
                }
            }
        }
    }
}