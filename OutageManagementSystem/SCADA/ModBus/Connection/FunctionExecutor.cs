using Outage.Common;
using Outage.Common.ServiceProxies.PubSub;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADA_Common;
using Outage.SCADA.SCADA_Common.PubSub;
using Outage.SCADA.SCADA_Config_Data.Configuration;
using Outage.SCADA.SCADA_Config_Data.Repository;
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
        private ModelResourcesDesc resourcesDesc = new ModelResourcesDesc();
        private TcpConnection connection;
        private ushort TcpPort;
        private Thread connectionProcess;
        private bool threadCancellationSignal = true;
        private int numberOfTries = 0;
        private IModBusFunction currentCommand;
        private ConnectionState _connectionState = ConnectionState.DISCONNECTED;
        private AutoResetEvent _commandEvent;
        private ConcurrentQueue<IModBusFunction> _commandQueue;


        private PublisherProxy publisherProxy = null;

        public PublisherProxy PublisherProxy
        {
            get
            {
                //TODO: diskusija statefull vs stateless

                if (publisherProxy != null)
                {
                    publisherProxy.Abort();
                    publisherProxy = null;
                }

                publisherProxy = new PublisherProxy(EndpointNames.PublisherEndpoint);
                publisherProxy.Open();

                return publisherProxy;
            }
        }


        public FunctionExecutor(ushort tcpPort)
        {
            this.TcpPort = tcpPort;
            _commandQueue = new ConcurrentQueue<IModBusFunction>();
            connection = new TcpConnection(this.TcpPort);
            _commandEvent = new AutoResetEvent(false);
            connectionProcess = new Thread(ConnectionProcessThread);
            connectionProcess.Name = "Communication with SIM";
            connectionProcess.Start();
        }

        public event UpdatePointDelegate UpdatePointEvent;

        public void EnqueueCommand(ModbusFunction modbusFunction)

        {
            if (_connectionState == ConnectionState.CONNECTED)
            {
                this._commandQueue.Enqueue(modbusFunction);
                this._commandEvent.Set();
            }
        }

        private void ConnectionProcessThread()
        {
            while (this.threadCancellationSignal)
            {
                try
                {
                    if (this._connectionState == ConnectionState.DISCONNECTED)
                    {
                        Console.WriteLine("Establishing connection");
                        this.numberOfTries = 0;
                        this.connection.Connect();
                        while (numberOfTries < 10)
                        {
                            if (this.connection.CheckState())
                            {
                                this._connectionState = ConnectionState.CONNECTED;
                                this.numberOfTries = 0;
                                Console.WriteLine("Connected");
                                break;
                            }
                            else
                            {
                                numberOfTries++;
                                if (this.numberOfTries == 10)
                                {
                                    this.connection.Disconnect();
                                    this._connectionState = ConnectionState.DISCONNECTED;
                                }
                            }
                        }
                    }
                    else
                    {
                        this._commandEvent.WaitOne();
                        while (_commandQueue.TryDequeue(out this.currentCommand))
                        {
                            this.connection.SendBytes(this.currentCommand.PackRequest());
                            byte[] message;
                            byte[] header = this.connection.RecvBytes(7);
                            int payLoadSize = 0;
                            unchecked
                            {
                                payLoadSize = IPAddress.NetworkToHostOrder((short)BitConverter.ToInt16(header, 4));
                            }
                            byte[] payload = this.connection.RecvBytes(payLoadSize - 1);
                            message = new byte[header.Length + payload.Length];
                            Buffer.BlockCopy(header, 0, message, 0, 7);
                            Buffer.BlockCopy(payload, 0, message, 7, payload.Length);
                            Dictionary<Tuple<PointType, ushort>, ushort> pointsToUpdate = this.currentCommand.ParseResponse(message);

                            foreach (Tuple<PointType, ushort> pointKey in pointsToUpdate.Keys)
                            {
                                ushort address = pointKey.Item2;
                                ushort newValue = pointsToUpdate[pointKey];

                                ConfigItem point = UpdatePoints(address, newValue);

                                ModelCode modelCode = resourcesDesc.GetModelCodeFromId(point.Gid);

                                Topic topic;

                                if (modelCode == ModelCode.ANALOG)
                                {
                                    topic = Topic.MEASUREMENT;
                                }
                                else if(modelCode == ModelCode.DISCRETE)
                                {
                                    topic = Topic.SWITCH_STATUS;
                                }
                                else
                                {
                                    throw new Exception("UNKNOWN type"); //TODO: log i bolji komentar
                                }

                                SCADAMessage scadaMessage = new SCADAMessage()
                                {
                                    Gid = point.Gid,
                                    Value = point.CurrentValue,
                                };

                                SCADAPublication scadaPublication = new SCADAPublication()
                                {
                                    Topic = topic,
                                    Message = scadaMessage,
                                };

                                PublisherProxy.Publish(scadaPublication);
                            }
                        }
                    }
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != 10054)
                    {
                        throw se;
                    }
                    currentCommand = null;
                    this._connectionState = ConnectionState.DISCONNECTED;
                    this.connection.Disconnect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    this._connectionState = ConnectionState.DISCONNECTED;
                    this.connection.Disconnect();
                }
            }
        }

        private ConfigItem UpdatePoints(ushort address, ushort newValue)
        {
            ConfigItem point = DataModelRepository.Instance.Points.Values.Where(x => x.Address == address).First();
            point.CurrentValue = newValue;

            return point;
        }
    }
}