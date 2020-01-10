﻿using EasyModbus;
using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.ServiceProxies.PubSub;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADACommon.PubSub;
using Outage.SCADA.SCADAData.Configuration;
using Outage.SCADA.SCADAData.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Outage.SCADA.ModBus.Connection
{
    public delegate void UpdatePointDelegate(PointType type, ushort pointAddres, ushort newValue);

    public class FunctionExecutor
    {
        private ILogger logger = LoggerWrapper.Instance; 
        private ModelResourcesDesc resourcesDesc = new ModelResourcesDesc();
        //private TcpConnection connection;
        private ushort TcpPort;
        private Thread connectionProcess;
        private bool threadCancellationSignal = false;
        private int numberOfTries = 0;

        private IModBusFunction currentCommand;
        //public ConnectionState connectionState = ConnectionState.DISCONNECTED;
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
                        logger.LogError(message, ex);
                        publisherProxy = null;
                    }
                    finally
                    {
                        numberOfTries++;
                        logger.LogDebug($"FunctionExecutor: PublisherProxy getter, try number: {numberOfTries}.");
                        Thread.Sleep(500);
                    }
                }

                return publisherProxy;
            }
        }
        #endregion

        #region Instance
        private static FunctionExecutor instance;
        private static readonly object lockSync = new object();

        public static FunctionExecutor Instance
        {
            get
            { 
                if(instance == null)
                {
                    lock(lockSync)
                    {
                        if(instance == null)
                        {
                            instance = new FunctionExecutor();
                        }
                    }
                }
                
                return instance; 
            }        
        }
        #endregion

        private FunctionExecutor()
        {
            ConfigData = SCADAConfigData.Instance;
            SCADAModel = SCADAModel.Instance;

            ModbusClient = new ModbusClient(ConfigData.IpAddress, ConfigData.TcpPort);

            commandQueue = new ConcurrentQueue<IModBusFunction>();
            commandEvent = new AutoResetEvent(false);
        }

        public event UpdatePointDelegate UpdatePointEvent;

        public void StartConnection()
        {
            ModbusClient.Connect();

            connectionProcess = new Thread(ConnectionProcessThread)
            {
                Name = "Communication with SIM"
            };

            connectionProcess.Start();
        }

        public void EnqueueCommand(ModbusFunction modbusFunction)

        {
            if (ModbusClient.Connected)
            {
                this.commandQueue.Enqueue(modbusFunction);
                this.commandEvent.Set();
            }
        }

        private void ConnectionProcessThread()
        {
            logger.LogInfo("ConnectionProcessThread is started");

            Console.WriteLine("Establishing connection...");
            logger.LogInfo("Establishing connection...");

            threadCancellationSignal = false;

            while (!this.threadCancellationSignal)
            {
                try
                {
                    if(ModbusClient == null)
                    {
                        ModbusClient = new ModbusClient(ConfigData.IpAddress, ConfigData.TcpPort);
                    }
                    
                    if(!ModbusClient.Connected)
                    {
                        ModbusClient.Connect();
                    }

                    logger.LogDebug("Connected and waiting for command event.");

                    this.commandEvent.WaitOne();

                    logger.LogDebug("Command event happened.");

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
                    //todo: log err
                }
            }

            logger.LogInfo("ConnectionProcessThred is stopped.");
        }

        private void PublishAnalogData(Dictionary<long, int> data)
        {
            IMultipleAnalogValueSCADAMessage scadaMessage = new MultipleAnalogValueSCADAMessage()
            {
                Values = data,
            };

            PublishData(scadaMessage, Topic.MEASUREMENT);
        }

        private void PublishDigitalData(Dictionary<long, bool> data)
        {
            IMultipleDiscreteValueSCADAMessage scadaMessage = new MultipleDiscreteValueSCADAMessage()
            {
                Values = data,
            };

            PublishData(scadaMessage, Topic.SWITCH_STATUS);
        }

        private void PublishData(IPublishableMessage scadaMessage, Topic topic)
        {
            SCADAPublication scadaPublication = new SCADAPublication()
            {
                Topic = topic,
                Message = scadaMessage,
            };

            using (PublisherProxy publisherProxy = PublisherProxy)
            {
                if (publisherProxy != null)
                {
                    publisherProxy.Publish(scadaPublication);
                    logger.LogInfo($"SCADA service published data from topic: {scadaPublication.Topic}");
                }
                else
                {
                    string errMsg = "PublisherProxy is null.";
                    logger.LogWarn(errMsg);
                    throw new NullReferenceException(errMsg);
                }
            }
        }

        //private ISCADAModelPointItem UpdatePoints(ushort address, ushort newValue)
        //{
        //    //throw new NotImplementedException("UpdatePoints");
        //    ConfigItem point = DataModelRepository.Instance.Points.Values.Where(x => x.Address == address).First();
        //    point.CurrentValue = newValue;

        //    return point;
        //}
    }
}