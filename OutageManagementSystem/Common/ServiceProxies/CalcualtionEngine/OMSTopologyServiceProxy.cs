using Outage.Common.OutageService.Interface;
using Outage.Common.ServiceContracts.CalculationEngine;
using System;
using System.ServiceModel;

namespace Outage.Common.ServiceProxies.CalcualtionEngine
{
    public class OMSTopologyServiceProxy : BaseProxy<ITopologyOMSService>, ITopologyOMSService, IDisposable
    {
        public OMSTopologyServiceProxy(string endpointName)
            : base(endpointName)
        {

        }

        public IOutageTopologyModel GetOMSModel()
        {
            try
            {
                return Channel.GetOMSModel();
            }
            catch (Exception ex)
            {
                LoggerWrapper.Instance.LogError($"Failed to get oms model. Exception message: {ex.Message}");
                throw;
            }
        }
    }
}
