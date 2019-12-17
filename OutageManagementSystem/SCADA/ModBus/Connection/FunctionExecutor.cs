using SCADA_Common;
using ModBus.FunctionParameters;
using ModBus.ModbusFuntions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SCADA_Config_Data.Repository;

namespace ModBus.Connection
{
    public delegate void UpdatePointDelegate(PointType type, ushort pointAddres, ushort newValue);

    public class FunctionExecutor
    {

        private TcpConnection connection;
        private ushort TcpPort;
        private Thread connectionProcess;
        private bool threadCancellationSignal = true;
        private int numberOfTries = 0;
        private IModBusFunction currentCommand;
        private ConnectionState _connectionState = ConnectionState.DISCONNECTED;
        private AutoResetEvent _commandEvent;
        private ConcurrentQueue<IModBusFunction> _commandQueue;
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
            if(_connectionState == ConnectionState.CONNECTED)
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
                            Dictionary<Tuple<PointType, ushort>, ushort> pointsToupdate = this.currentCommand.ParseResponse(message);

                            
                            foreach (var point in pointsToupdate)
                            {
                                UpdatePoints(point.Key.Item2, point.Value);
                                
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
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    this._connectionState = ConnectionState.DISCONNECTED;
                    this.connection.Disconnect();
                }
            }
        }

        private void UpdatePoints(ushort address, ushort newValue)
        {
            var point = DataModelRepository.Instance.Points.Values.Where(x => x.Address == address).First();
            point.CurrentValue = newValue;
        }


    }


}
