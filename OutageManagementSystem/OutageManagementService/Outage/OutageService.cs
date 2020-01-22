using Outage.Common;
using Outage.Common.ServiceContracts.OMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.Outage
{
    public class OutageService : IOutageContract
    {
        private ILogger logger;
        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }
        public List<OutageData> GetActiveOutages()
        {
            //TODO: Logic
            return new List<OutageData>();
        }

        public List<OutageData> GetArchivedOutages()
        {
            //TODO: Logic
            return new List<OutageData>();
        }

        public bool ReportOutage(long elementGid)
        {
            //TODO: Logic
            return true;
        }
    }
}
