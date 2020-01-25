using OMS.Web.Adapter.Contracts;
using Outage.Common.ServiceContracts;
using Outage.Common.ServiceContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Web.Adapter.Topology
{
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
