using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Outage.SCADA.ModBus.Connection
{
    public class TcpConnection : IConnection
    {
        private IPEndPoint remoteEP;
        private Socket socket;
        private ushort TcpPort;

        public TcpConnection(ushort tcpPort)
        {
            this.TcpPort = tcpPort;
            remoteEP = CreateRemoteEndpoint();
        }

        private IPEndPoint CreateRemoteEndpoint()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = null;
            foreach (IPAddress ip in ipHostInfo.AddressList)
                if ("127.0.0.1".Equals(ip.ToString()))
                    ipAddress = ip;
            return new IPEndPoint(ipAddress, this.TcpPort);
        }

        public bool CheckState()
        {
            return socket.Poll(30000, SelectMode.SelectWrite);
            //return socket.Connected;
        }

        public bool Connected()
        {
            return socket.Connected;
        }

        public void Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Blocking = false;
            try
            {
                socket.Connect(remoteEP);
            }
            catch (SocketException se)
            {
                if (se.ErrorCode != 10035)
                    throw se;
            }
        }

        public void Disconnect()
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            socket.Close();
            socket = null;
        }

        public byte[] RecvBytes(int numberOfBytes)
        {
            int numberOfReceivedBytes = 0;
            byte[] retVal = new byte[numberOfBytes];
            int numbOfReceived;
            //recv dok god ne dodje broj byte koji se ocekuje
            while (numberOfReceivedBytes < numberOfBytes)
            {
                numbOfReceived = 0;
                if (socket.Poll(1623, SelectMode.SelectRead))
                {
                    numbOfReceived = socket.Receive(retVal, numberOfReceivedBytes, (int)numberOfBytes - numberOfReceivedBytes, SocketFlags.None);
                    if (numbOfReceived > 0)
                    {
                        numberOfReceivedBytes += numbOfReceived;
                    }
                }
            }

            return retVal;
        }

        public void SendBytes(byte[] bytesToSend)
        {
            int sent = 0;
            while (bytesToSend.Count() > sent)
            {
                if (socket.Poll(1623, SelectMode.SelectWrite))
                {
                    sent += socket.Send(bytesToSend, sent, bytesToSend.Length - sent, SocketFlags.None);
                }
            }
        }
    }
}