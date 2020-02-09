using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.OMS;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceProxies.Outage
{
    public class OutageServiceProxy : BaseProxy<IOutageContract>, IOutageContract, IOutageService
    {
        public OutageServiceProxy(string endpointName)
            : base(endpointName)
        {
        }

        public List<ActiveOutage> GetActiveOutages()
        {
            List<ActiveOutage> outageModels = null;
            try
            {
                outageModels = Channel.GetActiveOutages();
            }
            catch (Exception e)
            {
                string message = "Exception in GetActiveOutages() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return outageModels;
        }

        public List<ArchivedOutage> GetArchivedOutages()
        {
            List<ArchivedOutage> outageModels = null;
            try
            {
                outageModels = Channel.GetArchivedOutages();
            }
            catch (Exception e)
            {
                string message = "Exception in GetArchivedOutages() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return outageModels;
        }

        public bool ReportOutage(long elementGid)
        {
            bool success;

            try
            {
                success = Channel.ReportOutage(elementGid);
            }
            catch (Exception e)
            {
                string message = "Exception in ReportOutage() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }
    }
}