using Outage.Common.ServiceContracts.SCADA;

namespace OMS.Web.Adapter.Topology
{
    using OMS.Web.Adapter.Contracts;
    using System.ServiceModel;

    public class TopologySCADACommandProxy : ChannelFactory<ISCADACommand>, IScadaClient
    {
        private ISCADACommand proxy;

        public TopologySCADACommandProxy(string address) : base(binding: new NetTcpBinding(SecurityMode.None), remoteAddress: address)
        {
            proxy = this.CreateChannel();
        }
        public void SendCommand(long guid, int value)
        {
            proxy.SendDiscreteCommand(guid, (ushort)value);
        }
    }
}
