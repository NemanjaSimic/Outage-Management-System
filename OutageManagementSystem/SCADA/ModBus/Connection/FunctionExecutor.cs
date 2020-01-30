using EasyModbus;
using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.ServiceProxies.PubSub;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Configuration;
using Outage.SCADA.SCADAData.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Outage.Common.PubSub.SCADADataContract;
using EasyModbus.Exceptions;
using Outage.SCADA.ModBus.FunctionParameters;
using System.ServiceModel;

namespace Outage.SCADA.ModBus.Connection
{

    public class FunctionExecutor
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private Thread functionExecutorThread;
        private bool threadCancellationSignal = false;

        private AutoResetEvent commandEvent;
        private ConcurrentQueue<IModbusFunction> commandQueue;
        private ConcurrentQueue<IModbusFunction> modelUpdateQueue;

        public  ISCADAConfigData ConfigData { get; private set; }
        public SCADAModel SCADAModel { get; private set; }
        public ModbusClient ModbusClient { get; private set; }

        #region Proxies

        private PublisherProxy publisherProxy = null;

        private PublisherProxy GetPublisherProxy()
        {
            //TODO: diskusija statefull vs stateless

            int numberOfTries = 0;
            int sleepInterval = 500;

            while (numberOfTries < 10)
            {
                try
                {
                    if (publisherProxy != null)
                    {
                        publisherProxy.Abort();
                        publisherProxy = null;
                    }

                    publisherProxy = new PublisherProxy(EndpointNames.PublisherEndpoint);
                    publisherProxy.Open();

                    if (publisherProxy.State == CommunicationState.Opened)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Exception on PublisherProxy initialization. Message: {ex.Message}";
                    Logger.LogError(message, ex);
                    publisherProxy = null;
                }
                finally
                {
                    numberOfTries++;
                    Logger.LogDebug($"FunctionExecutor: PublisherProxy getter, try number: {numberOfTries}.");

                    if (numberOfTries >= 100)
                    {
                        sleepInterval = 1000;
                    }

                    Thread.Sleep(sleepInterval);
                }
            }

            return publisherProxy;
        }

        #endregion Proxies

        public FunctionExecutor(SCADAModel scadaModel)
        {
            this.commandQueue = new ConcurrentQueue<IModbusFunction>();
            this.modelUpdateQueue = new ConcurrentQueue<IModbusFunction>();
            this.commandEvent = new AutoResetEvent(true);

            SCADAModel = scadaModel;
            SCADAModel.SignalIncomingModelConfirmation += EnqueueModelUpdateCommands;

            ConfigData = SCADAConfigData.Instance;
            ModbusClient = new ModbusClient(ConfigData.IpAddress.ToString(), ConfigData.TcpPort);

        }

        #region Public Members

        public void StartExecutorThread()
        {
            try
            {
                if(ModbusClient != null && !ModbusClient.Connected)
                {
                    ConnectToModbusClient();
                }

                functionExecutorThread = new Thread(FunctionExecutorThread)
                {
                    Name = "FunctionExecutorThread"
                };

                functionExecutorThread.Start();
            }
            catch (Exception e)
            {
                string message = "Exception caught in StartExecutor() method.";
                Logger.LogError(message, e);
            }
        }

        public void StopExecutorThread()
        {
            try
            {
                threadCancellationSignal = true;

                if (ModbusClient != null && ModbusClient.Connected)
                {
                    ModbusClient.Disconnect();
                }
            }
            catch (Exception e)
            {
                string message = "Exception caught in StopExecutor() method.";
                Logger.LogError(message, e);
            }
            
        }

        public bool EnqueueCommand(IModbusFunction modbusFunction)
        {
            bool success;

            try
            {
                this.commandQueue.Enqueue(modbusFunction);
                this.commandEvent.Set();
                success = true;
            }
            catch (Exception e)
            {
                success = false;
                string message = "Exception caught in EnqueueCommand() method.";
                Logger.LogError(message, e);
            }

            return success;
        }

        public bool EnqueueModelUpdateCommands(List<long> measurementGids)
        {
            bool success;
            ushort length = 6;
            try
            {
                foreach(long measurementGID in measurementGids)
                {
                    ISCADAModelPointItem scadaPointItem = SCADAModel.CurrentScadaModel[measurementGID];
                    IModbusFunction modbusFunction;

                    if (scadaPointItem is IAnalogSCADAModelPointItem analogSCADAModelPointItem)
                    {
                        modbusFunction = FunctionFactory.CreateModbusFunction(new ModbusWriteCommandParameters(length,
                                                                                                               (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER,
                                                                                                               analogSCADAModelPointItem.Address,
                                                                                                               analogSCADAModelPointItem.CurrentRawValue));
                    }
                    else if (scadaPointItem is IDiscreteSCADAModelPointItem discreteSCADAModelPointItem)
                    {
                        modbusFunction = FunctionFactory.CreateModbusFunction(new ModbusWriteCommandParameters(length,
                                                                                                               (byte)ModbusFunctionCode.WRITE_SINGLE_COIL,
                                                                                                               discreteSCADAModelPointItem.Address,
                                                                                                               discreteSCADAModelPointItem.CurrentValue));
                    }
                    else
                    {
                        Logger.LogWarn("Unknown type of ISCADAModelPointItem.");
                        continue;
                    }

                    this.modelUpdateQueue.Enqueue(modbusFunction);
                }
                
                
                this.commandEvent.Set();
                success = true;
            }
            catch (Exception e)
            {
                success = false;
                string message = "Exception caught in EnqueueModelUpdateCommands() method.";
                Logger.LogError(message, e);
            }

            return success;
        }

        #endregion Public Members


        #region Private Members

        private void ConnectToModbusClient()
        {
            int numberOfTries = 0;

            string message = $"Connecting to modbus client...";
            Console.WriteLine(message);
            Logger.LogInfo(message);

            while (!ModbusClient.Connected)
            {
                try
                {
                    ModbusClient.Connect();
                }
                catch(ConnectionException ce)
                {
                    Logger.LogWarn("ConnectionException on ModbusClient.Connect().", ce);
                }

                if (!ModbusClient.Connected)
                {
                    numberOfTries++;
                    Logger.LogDebug($"Connecting try number: {numberOfTries}.");
                    Thread.Sleep(500);
                }
                else if (!ModbusClient.Connected && numberOfTries == 40)
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

        private void FunctionExecutorThread()
        {
            Logger.LogInfo("FunctionExecutorThread is started");

            threadCancellationSignal = false;

            while (!this.threadCancellationSignal)
            {
                try
                {
                    if (ModbusClient == null)
                    {
                        ModbusClient = new ModbusClient(ConfigData.IpAddress.ToString(), ConfigData.TcpPort);
                    }

                    Logger.LogDebug("Connected and waiting for command event.");

                    this.commandEvent.WaitOne();

                    Logger.LogDebug("Command event happened.");

                    if (!ModbusClient.Connected)
                    {
                        ConnectToModbusClient();
                    }

                    //HIGH PRIORITY COMMANDS - model update commands
                    while (modelUpdateQueue.TryDequeue(out IModbusFunction currentCommand))
                    {
                        ExecuteCommand(currentCommand);
                    }

                    //REGULAR COMMANDS - acquisition and user commands
                    while (commandQueue.TryDequeue(out IModbusFunction currentCommand))
                    {
                        ExecuteCommand(currentCommand);
                    }
                }
                catch (Exception ex)
                {
                    string message = "Exception caught in FunctionExecutorThread.";
                    Logger.LogError(message, ex);
                }
            }

            if(ModbusClient.Connected)
            {
                ModbusClient.Disconnect();
            }

            Logger.LogInfo("FunctionExecutorThread is stopped.");
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
            }

            if (command is IReadAnalogModusFunction readAnalogCommand)
            {
                Dictionary<long, AnalogModbusData> data = readAnalogCommand.Data;

                //if data is empty that means that there are no new values in the current acquisition cycle
                if (data != null && data.Count > 0)
                {
                    SCADAMessage scadaMessage = new MultipleAnalogValueSCADAMessage(data);
                    PublishScadaData(Topic.MEASUREMENT, scadaMessage);
                }
            }
            else if (command is IReadDiscreteModbusFunction readDiscreteCommand)
            {
                Dictionary<long, DiscreteModbusData> data = readDiscreteCommand.Data;

                //if data is empty that means that there are no new values in the current acquisition cycle
                if (data != null && data.Count > 0)
                {
                    SCADAMessage scadaMessage = new MultipleDiscreteValueSCADAMessage(data);
                    PublishScadaData(Topic.SWITCH_STATUS, scadaMessage);
                }
            }
        }

        private void PublishScadaData(Topic topic, SCADAMessage scadaMessage)
        {
            SCADAPublication scadaPublication = new SCADAPublication(topic, scadaMessage);

            using (PublisherProxy publisherProxy = GetPublisherProxy())
            {
                if (publisherProxy != null)
                {
                    publisherProxy.Publish(scadaPublication);
                    Logger.LogInfo($"SCADA service published data from topic: {scadaPublication.Topic}");
                }
                else
                {
                    string errMsg = "PublisherProxy is null.";
                    Logger.LogWarn(errMsg);
                    throw new NullReferenceException(errMsg);
                }
            }
        }

        #endregion Private Members
    }
}