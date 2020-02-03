using Outage.Common;
using Outage.Common.ServiceContracts.OMS;
using OutageDatabase;
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
            //TODO: Calculate affected consumers and 

            ActiveOutage activeOutage = new ActiveOutage { ElementGid = elementGid, ReportTime = DateTime.Now };
            ActiveOutage addedOutage = null;
            using (OutageContext db = new OutageContext())
            {
                try
                {
                    addedOutage = db.ActiveOutages.Add(activeOutage);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Logger.LogError("Error on adding outage in Active Outages database.", e);
                    return false;
                }
            }

            bool success = false;
            if (activeOutage != null)
            {
                success = true;
                //TODO: Publish added outage
            }
            return success;
        }
    }
}
