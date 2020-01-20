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

namespace Outage.SCADA.ModBus.Connection
{
    public delegate void UpdatePointDelegate(PointType type, ushort pointAddres, ushort newValue);

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
        private ConcurrentQueue<IModbusFunction> highPriorityCommandQueue;

        //public bool isDelta = false;
        public ISCADAConfigData ConfigData { get; protected set; }
        public SCADAModel SCADAModel { get; protected set; }
        public ModbusClient ModbusClient { get; protected set; }

        #region Proxies

        private PublisherProxy publisherProxy = null;

        public PublisherProxy PublisherProxy
        {
            //TODO: diskusija statefull vs stateless
            get
            {
                int numberOfTries = 0;

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
                        break;
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
                        Thread.Sleep(500);
                    }
                }

                return publisherProxy;
            }
        }

        #endregion Proxies

        #region Instance

        private static FunctionExecutor instance;
        private static readonly object lockSync = new object();

        public static FunctionExecutor Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockSync)
                    {
                        if (instance == null)
                        {
                            instance = new FunctionExecutor();
                        }
                    }
                }

                return instance;
            }
        }

        #endregion Instance

        private FunctionExecutor()
        {
            ConfigData = SCADAConfigData.Instance;
            SCADAModel = SCADAModel.Instance;
            //SCADAModel.EnableDelta = EnableDelta;
            //SCADAModel.DisableDelta = DisableDelta;
            ModbusClient = new ModbusClient(ConfigData.IpAddress, ConfigData.TcpPort);

            commandQueue = new ConcurrentQueue<IModbusFunction>();
            commandEvent = new AutoResetEvent(true);
        }

        #region Public Members

        public void StartExecutor()
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

        public void StopExecutor()
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

        public bool EnqueueCommand(ModbusFunction modbusFunction)
        {
            bool success;

            try
            {
                //if (ModbusClient != null && ModbusClient.Connected)
                //{
                this.commandQueue.Enqueue(modbusFunction);
                this.commandEvent.Set();
                success = true;
                //}
                //else
                //{
                //    success = false;
                //    string message = "Modbus client is either not connected or null.";
                //    Logger.LogError(message);
                //}
        }
            catch (Exception e)
            {
                success = false;
                string message = "Exception caught in EnqueueCommand() method.";
                Logger.LogError(message, e);
            }

            return success;
        }

        public bool EnqueueHighPriorityCommand(ModbusFunction modbusFunction)
        {
            bool success;

            try
            {
                this.highPriorityCommandQueue.Enqueue(modbusFunction);
                this.commandEvent.Set();
                success = true;
            }
            catch (Exception e)
            {
                success = false;
                string message = "Exception caught in EnqueueHighPriorityCommand() method.";
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
                        ModbusClient = new ModbusClient(ConfigData.IpAddress, ConfigData.TcpPort);
                    }

                    if (!ModbusClient.Connected)
                    {
                        ConnectToModbusClient();
                    }
                    
                    //if (SCADAModel.modelImported) InitSimulatorValues();

                    Logger.LogDebug("Connected and waiting for command event.");

                    this.commandEvent.WaitOne();

                    Logger.LogDebug("Command event happened.");

                    //HIGH PROPRITY COMMANDS - model update commands
                    while (highPriorityCommandQueue.TryDequeue(out IModbusFunction currentCommand))
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
                //todo: retry
                string message = "Exception on currentCommand.Execute().";
                Logger.LogWarn(message, e);
            }

            if (command is IReadAnalogModusFunction readAnalogCommand)
            {
                PublishAnalogData(readAnalogCommand.Data);
            }
            else if (command is IReadDiscreteModbusFunction readDiscreteCommand)
            {
                PublishDigitalData(readDiscreteCommand.Data);
            }
        }

        private void PublishAnalogData(Dictionary<long, AnalogModbusData> data)
        {
            SCADAMessage scadaMessage = new MultipleAnalogValueSCADAMessage(data);
            PublishScadaData(Topic.MEASUREMENT, scadaMessage);
        }

        private void PublishDigitalData(Dictionary<long, DiscreteModbusData> data)
        {
            SCADAMessage scadaMessage = new MultipleDiscreteValueSCADAMessage(data);
            PublishScadaData(Topic.SWITCH_STATUS, scadaMessage);
        }

        private void PublishScadaData(Topic topic, SCADAMessage scadaMessage)
        {
            SCADAPublication scadaPublication = new SCADAPublication(topic, scadaMessage);

            using (PublisherProxy publisherProxy = PublisherProxy)
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

        //private void InitSimulatorValues()
        //{
        //    foreach (ISCADAModelPointItem pointItem in SCADAModel.Instance.CurrentScadaModel.Values)
        //    {
        //        if(pointItem is AnalogSCADAModelPointItem analogPointItem)
        //        {
        //            ModbusClient.WriteSingleRegister(analogPointItem.Address - 1, analogPointItem.CurrentRawValue);
        //        }
        //        else if(pointItem is DiscreteSCADAModelPointItem discretePointItem)
        //        {
        //            ModbusClient.WriteSingleCoil(discretePointItem.Address - 1, (discretePointItem.CurrentValue == 1) ? true : false);
        //        }
        //    }
        //}

        //private void EnableDelta()
        //{
        //    isDelta = true;
        //    ModbusClient.Disconnect();
        //}

        //private void DisableDelta()
        //{
        //    isDelta = false;
        //}

        private void InitSimulatorValues()
        {
            try
            {
                string ipAddress = SCADAConfigData.Instance.IpAddress;
                ushort tcpPort = SCADAConfigData.Instance.TcpPort;
                ModbusClient modbusClient = new ModbusClient(ipAddress, tcpPort);
                modbusClient.Connect();

                if (modbusClient.Connected)
                {
                    foreach (ISCADAModelPointItem pointItem in SCADAModel.Instance.CurrentScadaModel.Values)
                    {
                        if (pointItem is AnalogSCADAModelPointItem analogPointItem)
                        {
                            modbusClient.WriteSingleRegister(analogPointItem.Address - 1, analogPointItem.CurrentRawValue);
                        }
                        else if (pointItem is DiscreteSCADAModelPointItem discretePointItem)
                        {
                            bool commandingValue;
                            if (discretePointItem.CurrentValue == 0)
                            {
                                commandingValue = false;
                            }
                            else if (discretePointItem.CurrentValue == 1)
                            {
                                commandingValue = true;
                            }
                            else
                            {
                                Logger.LogError("Non-boolean value in write single coil command parameter.");
                                continue;
                            }

                            modbusClient.WriteSingleCoil(discretePointItem.Address - 1, commandingValue);
                        }
                    }

                    modbusClient.Disconnect();
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Exception caught in InitSimulatorValues().", e);
            }
        }

        #endregion Private Members
    }
}