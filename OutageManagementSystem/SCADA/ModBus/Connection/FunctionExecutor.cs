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

        private IModBusFunction currentCommand;
        private AutoResetEvent commandEvent;
        private ConcurrentQueue<IModBusFunction> commandQueue;

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

            ModbusClient = new ModbusClient(ConfigData.IpAddress, ConfigData.TcpPort);

            commandQueue = new ConcurrentQueue<IModBusFunction>();
            commandEvent = new AutoResetEvent(false);
        }

        //[Obsolete("Is it usefull?")]
        //public event UpdatePointDelegate UpdatePointEvent;

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

        public void EnqueueCommand(ModbusFunction modbusFunction)
        {
            try
            {
                if (ModbusClient != null && ModbusClient.Connected)
                {
                    this.commandQueue.Enqueue(modbusFunction);
                    this.commandEvent.Set();
                }
            }
            catch (Exception e)
            {
                string message = "Exception caught in EnqueueCommand() method.";
                Logger.LogError(message, e);
            }
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

                    Logger.LogDebug("Connected and waiting for command event.");

                    this.commandEvent.WaitOne();

                    Logger.LogDebug("Command event happened.");

                    while (commandQueue.TryDequeue(out this.currentCommand))
                    {
                        currentCommand.Execute(ModbusClient);

                        if (currentCommand is IReadAnalogModBusFunction readAnalogCommand)
                        {
                            PublishAnalogData(readAnalogCommand.Data);
                        }
                        else if (currentCommand is IReadDigitalModBusFunction readDigitalCommand)
                        {
                            PublishDigitalData(readDigitalCommand.Data);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = "Exception caught in FunctionExecutorThread.";
                    Logger.LogError(message, ex);
                }
            }

            Logger.LogInfo("FunctionExecutorThread is stopped.");
        }

        private void PublishAnalogData(Dictionary<long, int> data)
        {
            SCADAMessage scadaMessage = new MultipleAnalogValueSCADAMessage(data);
            PublishScadaData(Topic.MEASUREMENT, scadaMessage);
        }

        private void PublishDigitalData(Dictionary<long, bool> data)
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

        #endregion Private Members
    }
}