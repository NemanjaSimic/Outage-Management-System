using Outage.Common;
using Outage.Common.ServiceContracts.OMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.Calling
{
    public class CallingService : ICallingContract
    {
        private ILogger logger;
        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }
        public void ReportMalfunction(long consumerGid)
        {
            //TODO: Logic
            Logger.LogInfo($"Malfunction reported by consumer {consumerGid}.");
        }
    }
}
