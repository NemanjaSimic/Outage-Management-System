using Outage.Common;
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
        private ISubscriber subscriber;
        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private OutageModel outageModel;
        private CallTracker callTracker;

        public OutageService()
        {
            outageModel = new OutageModel();
            callTracker = new CallTracker("CallTrackerSubscriber", outageModel);
            SubscribeOnEmailService();
        }

        private void SubscribeOnEmailService()
        {
            subscriber = new SubscriberProxy(callTracker, EndpointNames.SubscriberEndpoint);
            subscriber.Subscribe(Topic.OUTAGE_EMAIL);
            
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
            return outageModel.ReportPotentialOutage(elementGid); //TODO: enum (error, noAffectedConsumers, success,...)
        }
    }
}
