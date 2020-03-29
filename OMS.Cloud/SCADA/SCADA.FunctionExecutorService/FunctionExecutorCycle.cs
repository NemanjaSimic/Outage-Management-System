using EasyModbus;
using EasyModbus.Exceptions;
using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Cloud.SCADA.Data.Repository;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.SCADA;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Threading;

namespace OMS.Cloud.SCADA.FunctionExecutorService
{
    internal class FunctionExecutorCycle
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private CloudQueue readCommandQueue;
        private CloudQueue writeCommandQueue;
        private CloudQueue modelUpdateCommandQueue;

        public ISCADAConfigData ConfigData { get; private set; }
        public SCADAModel SCADAModel { get; private set; }
        public ModbusClient ModbusClient { get; private set; }

        private Dictionary<long, IModbusData> measurementsCache;
        public Dictionary<long, IModbusData> MeasurementsCache
        {
            get { return measurementsCache ?? (measurementsCache = new Dictionary<long, IModbusData>()); }
        }

        public FunctionExecutorCycle()
        {
            //SCADAModel = scadaModel;
            //SCADAModel.SignalIncomingModelConfirmation += EnqueueModelUpdateCommands;

            //ConfigData = SCADAConfigData.Instance;
            ModbusClient = new ModbusClient(ConfigData.IpAddress.ToString(), ConfigData.TcpPort);

            CloudQueueHelper.TryGetQueue("readcommandqueue", out this.readCommandQueue);
            CloudQueueHelper.TryGetQueue("writecommandqueue", out this.writeCommandQueue);
            CloudQueueHelper.TryGetQueue("mucommandqueue", out this.modelUpdateCommandQueue);
        }

        public void Start()
        {
            //todo
            //try
            //{
            //    if (ModbusClient == null)
            //    {
            //        ModbusClient = new ModbusClient(ConfigData.IpAddress.ToString(), ConfigData.TcpPort);
            //    }

            //    //Logger.LogDebug("Connected and waiting for command event.");

            //    this.commandEvent.WaitOne();

            //    //Logger.LogDebug("Command event happened.");

            //    if (!ModbusClient.Connected)
            //    {
            //        ConnectToModbusClient();
            //    }

            //    //HIGH PRIORITY COMMANDS - model update commands
            //    while (modelUpdateQueue.TryDequeue(out IWriteModbusFunction currentCommand))
            //    {
            //        ExecuteCommand(currentCommand);
            //    }

            //    this.modelUpdateQueueEmptyEvent.Set();

            //    //WRITE COMMANDS
            //    while (writeCommandQueue.TryDequeue(out IWriteModbusFunction currentCommand))
            //    {
            //        ExecuteCommand(currentCommand);
            //    }

            //    this.writeCommandQueueEmptyEvent.Set();

            //    //READ COMMANDS - acquisition
            //    while (readCommandQueue.TryDequeue(out IReadModbusFunction currentCommand))
            //    {
            //        ExecuteCommand(currentCommand);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string message = "Exception caught in FunctionExecutorThread.";
            //    Logger.LogError(message, ex);
            //}
        }

        private void ConnectToModbusClient()
        {
            int numberOfTries = 0;
            int sleepInterval = 500;

            string message = $"Connecting to modbus client...";
            Console.WriteLine(message);
            Logger.LogInfo(message);

            while (!ModbusClient.Connected)
            {
                try
                {
                    ModbusClient.Connect();
                }
                catch (ConnectionException ce)
                {
                    Logger.LogWarn("ConnectionException on ModbusClient.Connect().", ce);
                }

                if (!ModbusClient.Connected)
                {
                    numberOfTries++;
                    Logger.LogDebug($"Connecting try number: {numberOfTries}.");

                    if (numberOfTries >= 100)
                    {
                        sleepInterval = 1000;
                    }

                    Thread.Sleep(sleepInterval);
                }
                else if (!ModbusClient.Connected && numberOfTries == int.MaxValue)
                {
                    string timeoutMessage = "Failed to connect to Modbus client by exceeding the maximum number of connection retries.";
                    Logger.LogError(timeoutMessage);
                    throw new Exception(timeoutMessage);
                }
                else
                {
                    message = $"Successfully connected to modbus client.";
                    Console.WriteLine(message);
                    Logger.LogInfo(message);
                }
            }
        }

        private void ExecuteCommand(IModbusFunction command)
        {
            try
            {
                command.Execute(ModbusClient);
            }
            catch (Exception e)
            {
                string message = "Exception on currentCommand.Execute().";
                Logger.LogWarn(message, e);
                ModbusClient.Disconnect();
                return;
            }

            if (command is IReadAnalogModusFunction readAnalogCommand)
            {
                //todo: MakeAnalogEntryToMeasurementCache(readAnalogCommand.Data, true);
            }
            else if (command is IReadDiscreteModbusFunction readDiscreteCommand)
            {
                //todo: MakeDiscreteEntryToMeasurementCache(readDiscreteCommand.Data, true);
            }
            else if (command is IWriteModbusFunction writeModbusCommand)
            {
                CommandDescription commandValue = new CommandDescription()
                {
                    Address = writeModbusCommand.ModbusWriteCommandParameters.OutputAddress,
                    Value = writeModbusCommand.ModbusWriteCommandParameters.Value,
                    CommandOrigin = writeModbusCommand.CommandOrigin,
                };

                PointType pointType;
                switch (writeModbusCommand.ModbusWriteCommandParameters.FunctionCode)
                {
                    case (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER:
                        pointType = PointType.ANALOG_OUTPUT;
                        break;
                    case (byte)ModbusFunctionCode.WRITE_SINGLE_COIL:
                        pointType = PointType.DIGITAL_OUTPUT;
                        break;
                    default:
                        Logger.LogError($"Function code {writeModbusCommand.ModbusWriteCommandParameters.FunctionCode} is not comatible with write command.");
                        return;
                }

                if (SCADAModel.CurrentAddressToGidMap[pointType].ContainsKey(commandValue.Address))
                {
                    long gid = SCADAModel.CurrentAddressToGidMap[pointType][commandValue.Address];

                    SCADAModel.CommandedValuesCache[gid] = commandValue;
                }
            }
        }
    }
}
