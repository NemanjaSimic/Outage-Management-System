using Outage.Common;
using Outage.Common.ServiceContracts;
using Outage.Common.UI;
using System;
using System.ServiceModel;

namespace TopologyServiceClientMock
{
    public class TopologyServiceProxy : ClientBase<ITopologyServiceContract>, ITopologyServiceContract, IDisposable
    {
        public TopologyServiceProxy(string endpointName)
            : base(endpointName)
        {

        }
        public UIModel GetTopology()
        {
            try
            {
                return Channel.GetTopology();
            }
            catch (Exception ex)
            {
                LoggerWrapper.Instance.LogError($"Failed to get topology. Exception message: {ex.Message}");
                throw;
            }
        }

    }
}
