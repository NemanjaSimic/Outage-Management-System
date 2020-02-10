using Outage.Common;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.OMS;
using OutageDatabase;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

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
                activeOutages = db.ActiveOutages.Include(a => a.AffectedConsumers).ToList();
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
