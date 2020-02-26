using EasyModbus;
using Outage.Common;
using Outage.Common.ServiceProxies.PubSub;
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
using Outage.Common.Exceptions.SCADA;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceContracts.PubSub;
using System.Text;

namespace Outage.SCADA.ModBus.Connection
{

    public class FunctionExecutor : IReadCommandEnqueuer, IWriteCommandEnqueuer, IModelUpdateCommandEnqueuer
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private Thread functionExecutorThread;
        private bool threadCancellationSignal = false;

        private AutoResetEvent commandEvent;
        private AutoResetEvent modelUpdateQueueEmptyEvent;
        private AutoResetEvent writeCommandQueueEmptyEvent;

        private ConcurrentQueue<IModbusFunction> modelUpdateQueue;
        private ConcurrentQueue<IModbusFunction> writeCommandQueue;
        private ConcurrentQueue<IModbusFunction> readCommandQueue;
        private ProxyFactory proxyFactory;

        public ISCADAConfigData ConfigData { get; private set; }
        public SCADAModel SCADAModel { get; private set; }
        public ModbusClient ModbusClient { get; private set; }

        private Dictionary<long, IModbusData> measurementsCache;
        public Dictionary<long, IModbusData> MeasurementsCache
        {
            get { return measurementsCache ?? (measurementsCache = new Dictionary<long, IModbusData>()); }
        }

        public FunctionExecutor(SCADAModel scadaModel)
        {
            this.modelUpdateQueue = new ConcurrentQueue<IModbusFunction>();
            this.writeCommandQueue = new ConcurrentQueue<IModbusFunction>();
            this.readCommandQueue = new ConcurrentQueue<IModbusFunction>();
            this.commandEvent = new AutoResetEvent(true);
            this.modelUpdateQueueEmptyEvent = new AutoResetEvent(false);
            this.writeCommandQueueEmptyEvent = new AutoResetEvent(false);
            this.proxyFactory = new ProxyFactory();

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
                if (ModbusClient != null && !ModbusClient.Connected)
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

        public bool EnqueueReadCommand(IModbusFunction modbusFunction)
        {
            bool success;

            if(!(modbusFunction is IReadAnalogModusFunction || modbusFunction is IReadDiscreteModbusFunction))
            {
                string message = "EnqueueReadCommand => trying to enqueue modbus function that implements neither IReadDiscreteModbusFunction nor IReadDiscreteModbusFunction interface.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if(!modelUpdateQueue.IsEmpty)
            {
                this.modelUpdateQueueEmptyEvent.WaitOne();
            }

            if(!writeCommandQueue.IsEmpty)
            {
                this.writeCommandQueueEmptyEvent.WaitOne();
            }

            try
            {
                this.readCommandQueue.Enqueue(modbusFunction);
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

        public bool EnqueueWriteCommand(IModbusFunction modbusFunction)
        {
            bool success;

            if (modbusFunction is IReadAnalogModusFunction || modbusFunction is IReadDiscreteModbusFunction)
            {
                string message = "EnqueueWriteCommand => trying to enqueue modbus function that implements IReadDiscreteModbusFunction or IReadDiscreteModbusFunction interface.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if (!modelUpdateQueue.IsEmpty)
            {
                this.modelUpdateQueueEmptyEvent.WaitOne();
            }

            try
            {
                this.writeCommandQueue.Enqueue(modbusFunction);
                this.readCommandQueue = new ConcurrentQueue<IModbusFunction>();
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

            Dictionary<long, AnalogModbusData> analogData = new Dictionary<long, AnalogModbusData>();
            Dictionary<long, DiscreteModbusData> discreteData = new Dictionary<long, DiscreteModbusData>();
            MeasurementsCache.Clear();

            try
            {
                Dictionary<long, ISCADAModelPointItem> currentScadaModel = SCADAModel.CurrentScadaModel;

                foreach (long measurementGID in measurementGids)
                {
                    ISCADAModelPointItem scadaPointItem = currentScadaModel[measurementGID];
                    IModbusFunction modbusFunction;

                    if (scadaPointItem is IAnalogSCADAModelPointItem analogSCADAModelPointItem)
                    {
                        modbusFunction = FunctionFactory.CreateModbusFunction(new ModbusWriteCommandParameters(length,
                                                                                                               (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER,
                                                                                                               analogSCADAModelPointItem.Address,
                                                                                                               analogSCADAModelPointItem.CurrentRawValue));

                        //TODO: SetAlarm to private method -> call on current value setter
                        analogSCADAModelPointItem.SetAlarms();
                        AnalogModbusData analogModbusData = new AnalogModbusData(analogSCADAModelPointItem.CurrentEguValue, analogSCADAModelPointItem.Alarm);
                        analogData.Add(measurementGID, analogModbusData);
                    }
                    else if (scadaPointItem is IDiscreteSCADAModelPointItem discreteSCADAModelPointItem)
                    {
                        modbusFunction = FunctionFactory.CreateModbusFunction(new ModbusWriteCommandParameters(length,
                                                                                                               (byte)ModbusFunctionCode.WRITE_SINGLE_COIL,
                                                                                                               discreteSCADAModelPointItem.Address,
                                                                                                               discreteSCADAModelPointItem.CurrentValue));
                        
                        //TODO: SetAlarm to private method -> call on current value setter
                        discreteSCADAModelPointItem.SetAlarms();
                        DiscreteModbusData discreteModbusData = new DiscreteModbusData(discreteSCADAModelPointItem.CurrentValue, discreteSCADAModelPointItem.Alarm);
                        discreteData.Add(measurementGID, discreteModbusData);
                    }
                    else
                    {
                        Logger.LogWarn("Unknown type of ISCADAModelPointItem.");
                        continue;
                    }

                    this.modelUpdateQueue.Enqueue(modbusFunction);
                }

                MakeAnalogEntryToMeasurementCache(analogData, true);
                MakeDiscreteEntryToMeasurementCache(discreteData, false);

                success = true;
                this.writeCommandQueue = new ConcurrentQueue<IModbusFunction>();
                this.readCommandQueue = new ConcurrentQueue<IModbusFunction>();
                this.commandEvent.Set();
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

                    this.modelUpdateQueueEmptyEvent.Set();

                    //WRITE COMMANDS
                    while (writeCommandQueue.TryDequeue(out IModbusFunction currentCommand))
                    {
                        ExecuteCommand(currentCommand);
                    }

                    this.writeCommandQueueEmptyEvent.Set();

                    //READ COMMANDS - acquisition
                    while (readCommandQueue.TryDequeue(out IModbusFunction currentCommand))
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

            if (ModbusClient.Connected)
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
                MakeAnalogEntryToMeasurementCache(readAnalogCommand.Data, true);
            }
            else if (command is IReadDiscreteModbusFunction readDiscreteCommand)
            {
                MakeDiscreteEntryToMeasurementCache(readDiscreteCommand.Data, true);
            }
        }

        private void MakeAnalogEntryToMeasurementCache(Dictionary<long, AnalogModbusData> data, bool permissionToPublishData)
        {
            Dictionary<long, AnalogModbusData> publicationData = new Dictionary<long, AnalogModbusData>();

            if (data == null)
            {
                string message = $"WriteToMeasurementsCache() => readAnalogCommand.Data is null.";
                Logger.LogError(message);
                throw new NullReferenceException(message);
            }

            foreach (long gid in data.Keys)
            {
                if (!MeasurementsCache.ContainsKey(gid))
                {
                    MeasurementsCache.Add(gid, data[gid]);
                    publicationData.Add(gid, data[gid]);
                }
                else if (MeasurementsCache[gid] is AnalogModbusData analogCacheItem && analogCacheItem.Value != data[gid].Value)
                {
                    MeasurementsCache[gid] = data[gid];
                    publicationData.Add(gid, MeasurementsCache[gid] as AnalogModbusData);
                }
            }

            //if data is empty that means that there are no new values in the current acquisition cycle
            if (permissionToPublishData && publicationData.Count > 0)
            {
                SCADAMessage scadaMessage = new MultipleAnalogValueSCADAMessage(publicationData);
                PublishScadaData(Topic.MEASUREMENT, scadaMessage);
            }
        }

        private void MakeDiscreteEntryToMeasurementCache(Dictionary<long, DiscreteModbusData> data, bool permissionToPublishData)
        {
            Dictionary<long, DiscreteModbusData> publicationData = new Dictionary<long, DiscreteModbusData>();

            if (data == null)
            {
                string message = $"WriteToMeasurementsCache() => readAnalogCommand.Data is null.";
                Logger.LogError(message);
                throw new NullReferenceException(message);
            }

            foreach (long gid in data.Keys)
            {
                if (!MeasurementsCache.ContainsKey(gid))
                {
                    MeasurementsCache.Add(gid, data[gid]);
                    publicationData.Add(gid, data[gid]);
                }
                else if (MeasurementsCache[gid] is DiscreteModbusData discreteCacheItem && discreteCacheItem.Value != data[gid].Value)
                {
                    Logger.LogDebug($"Value changed. Old value: {discreteCacheItem.Value}; new value: {data[gid].Value}");
                    MeasurementsCache[gid] = data[gid];
                    publicationData.Add(gid, MeasurementsCache[gid] as DiscreteModbusData);
                }
            }

            //if data is empty that means that there are no new values in the current acquisition cycle
            if (permissionToPublishData && publicationData.Count > 0)
            {
                SCADAMessage scadaMessage = new MultipleDiscreteValueSCADAMessage(publicationData);
                PublishScadaData(Topic.SWITCH_STATUS, scadaMessage);
            }
        }

        private void PublishScadaData(Topic topic, SCADAMessage scadaMessage)
        {
            SCADAPublication scadaPublication = new SCADAPublication(topic, scadaMessage);

            using (PublisherProxy publisherProxy = proxyFactory.CreateProxy<PublisherProxy, IPublisher>(EndpointNames.PublisherEndpoint))
            {
                if (publisherProxy == null)
                {
                    string errMsg = "PublisherProxy is null.";
                    Logger.LogWarn(errMsg);
                    throw new NullReferenceException(errMsg);    
                }

                publisherProxy.Publish(scadaPublication, "SCADA_PUBLISHER");
                Logger.LogInfo($"SCADA service published data from topic: {scadaPublication.Topic}");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("MeasurementCache content: ");

                foreach(long gid in MeasurementsCache.Keys)
                {
                    IModbusData data = MeasurementsCache[gid];

                    if(data is AnalogModbusData analogModbusData)
                    { 
                        sb.AppendLine($"Analog data line: [gid] 0x{gid:X16}, [value] {analogModbusData.Value}, [alarm] {analogModbusData.Alarm}");
                    }
                    else if(data is DiscreteModbusData discreteModbusData)
                    {
                        sb.AppendLine($"Analog data line: [gid] 0x{gid:X16}, [value] {discreteModbusData.Value}, [alarm] {discreteModbusData.Alarm}");
                    }
                    else
                    {
                        sb.AppendLine($"UNKNOWN data type: {data.GetType()}");
                    }
                }

                Logger.LogDebug(sb.ToString());
            }
        }

        #endregion Private Members
    }
}