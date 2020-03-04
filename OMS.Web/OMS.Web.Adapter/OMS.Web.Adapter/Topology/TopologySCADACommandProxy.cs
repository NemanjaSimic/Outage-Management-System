using Outage.Common.ServiceContracts.SCADA;

namespace OMS.Web.Adapter.Topology
{
    using global::Outage.Common;
    using OMS.Web.Adapter.Contracts;
    using System;
    using System.ServiceModel;

    [Obsolete("Use Outage.Common.ServiceProxies.Commanding.SwitchCommadningProxy.")]

    public class TopologySCADACommandProxy : ChannelFactory<ISCADACommand>, IScadaClient
    {
        private ISCADACommand proxy;

        [Obsolete("Use Outage.Common.ServiceProxies.Commanding.SwitchCommadningProxy.")]

        public TopologySCADACommandProxy(string address) : base(binding: new NetTcpBinding(SecurityMode.None), remoteAddress: address)
        {
            proxy = this.CreateChannel();
        }

        [Obsolete("Use Outage.Common.ServiceProxies.Commanding.SwitchCommadningProxy.")]

        public void SendCommand(long guid, int value)
        {
            proxy.SendDiscreteCommand(guid, (ushort)value, CommandOriginType.USER_COMMAND);
        }
    }
}
