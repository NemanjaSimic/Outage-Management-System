using Outage.Common;
using Outage.Common.OutageService.Interface;
using Outage.Common.OutageService.Model;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.OMS;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceProxies.PubSub;
using OutageDatabase;
using OutageManagementService.Calling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OutageManagementService.Outage
{
    public class OutageService : IOutageContract
    {
        private ILogger logger;
       
        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public static OutageModel outageModel;
       
       

        public List<ActiveOutage> GetActiveOutages()
        {
            //TODO: Logic
            List<ActiveOutage> activeOutages = null;
            using (OutageContext db = new OutageContext())
            {
                activeOutages = db.ActiveOutages.ToList();
            }

            return activeOutages;
        }

        public List<ArchivedOutage> GetArchivedOutages()
        {
            List<ArchivedOutage> archivedOutages = null;
            using (OutageContext db = new OutageContext())
            {
                archivedOutages = db.ArchivedOutages.ToList();
            }


            return archivedOutages;
        }

        public bool ReportOutage(long elementGid)
        {
            return outageModel.ReportPotentialOutage(elementGid); //TODO: enum (error, noAffectedConsumers, success,...)
        }
    }
}
