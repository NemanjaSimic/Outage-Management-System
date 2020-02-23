using Outage.Common.ServiceContracts.OMS;
using System;

namespace Outage.Common.ServiceProxies.Outage
{
    public class OutageLifecycleUICommandingProxy : BaseProxy<IOutageLifecycleUICommandingContract>, IOutageLifecycleUICommandingContract
    {
        public OutageLifecycleUICommandingProxy(string endpointName) 
            : base(endpointName)
        {
        }

        public bool IsolateOutage(long outageId)
        {
            bool success;

            try
            {
                success = Channel.IsolateOutage(outageId);
            }
            catch (Exception e)
            {
                string message = "Exception in IsolateOutage() proxy method.";
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
    }
}
