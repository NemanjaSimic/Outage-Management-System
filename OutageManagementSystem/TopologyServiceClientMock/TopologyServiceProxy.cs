using CECommon.Model.UI;
using CECommon.ServiceContracts;
using System;
using System.ServiceModel;

namespace TopologyServiceClientMock
{
    public class TopologyServiceProxy : ClientBase<ITopologyServiceContract>, ITopologyServiceContract, IDisposable
    {
        private ITopologyServiceContract proxy;

        public TopologyServiceProxy(string endPointName) : base (endPointName) 
        {
            proxy = this.CreateChannel();
        }
        public UIModel GetTopology()
        {
            return proxy.GetTopology();
        }

        public void Dispose()
        {
            if (proxy != null)
            {
                proxy = null;
            }
            this.Close();
        }
    }
}
