using Outage.Common.ServiceContracts.OMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies.Outage
{
    public class OutageLifecycleProxy : BaseProxy<IOutageLifecycleContract>, IOutageLifecycleContract
    {
        public OutageLifecycleProxy(string endpointName)
           : base(endpointName)
        {
        }

        public bool IsolateOutage(long outageId)
        {
            throw new NotImplementedException();
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

        public bool ResolveOutage(long outageId)
        {
            bool success;

            try
            {
                success = Channel.ResolveOutage(outageId);
            }
            catch (Exception e)
            {
                string message = "Exception in ResolveOutage() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }

        public bool SendLocationIsolationCrew(long outageId)
        {
            bool success;

            try
            {
                success = Channel.SendLocationIsolationCrew(outageId);
            }
            catch (Exception e)
            {
                string message = "Exception in SendLocationIsolationCrew() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }

        public bool SendRepairCrew(long outageId)
        {
            bool success;

            try
            {
                success = Channel.SendRepairCrew(outageId);
            }
            catch (Exception e)
            {
                string message = "Exception in SendRepairCrew() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }

        public bool ValidateResolveConditions(long outageId)
        {
            bool success;

            try
            {
                success = Channel.ValidateResolveConditions(outageId);
            }
            catch (Exception e)
            {
                string message = "Exception in ValidateResolveConditions() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }
    }
}
