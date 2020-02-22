using Outage.Common;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.OMS;
using OutageDatabase;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System;

namespace OutageManagementService.Outage
{
    public class OutageService : IOutageAccessContract, IReportPotentialOutageContract, IOutageLifecycleUICommandingContract
    {
        private ILogger logger;
       
        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public static OutageModel outageModel;


        #region IOutageAccessContract
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
        #endregion

        #region IReportPotentialOutageContract
        public bool ReportPotentialOutage(long elementGid)
        {
            bool result;

            try
            {
                result = outageModel.ReportPotentialOutage(elementGid); //TODO: enum (error, noAffectedConsumers, success,...)
            }
            catch (Exception e)
            {
                result = false;
                string message = "ReportPotentialOutage => exception caught";
                Logger.LogError(message, e);
                //todo throw;
            }

            return result;
        }
        #endregion

        #region IOutageLifecycleUICommandingContract
        public bool IsolateOutage(long outageId)
        {
            bool result;

            try
            {
                result = outageModel.IsolateOutage(outageId);
            }
            catch (Exception e)
            {
                result = false;
                string message = "IsolateOutage => exception caught";
                Logger.LogError(message, e);
                //todo: throw;
            }

            return result;
        }

        public bool SendRepairCrew(long outageId)
        {
            bool result;

            try
            {
                result = outageModel.SendRepairCrew(outageId);
            }
            catch (Exception e)
            {
                result = false;
                string message = "SendRepairCrew => exception caught";
                Logger.LogError(message, e);
                //todo throw;
            }

            return result;
        }

        public bool SendLocationIsolationCrew(long outageId)
        {
            bool result;

            try
            {
                result = outageModel.SendLocationIsolationCrew(outageId);
            }
            catch (Exception e)
            {
                result = false;
                string message = "SendLocationIsolationCrew => exception caught";
                Logger.LogError(message, e);
                //todo throw;
            }

            return result;
        }

        public bool ValidateResolveConditions(long outageId)
        {
            bool result;

            try
            {
                result = outageModel.ValidateResolveConditions(outageId);
            }
            catch (Exception e)
            {
                result = false;
                string message = "ValidateResolveConditions => exception caught";
                Logger.LogError(message, e);
                //todo throw;
            }

            return result;
        }

        public bool ResolveOutage(long outageId)
        {
            bool result;

            try
            {
                result = outageModel.ResolveOutage(outageId);
            }
            catch (Exception e)
            {
                result = false;
                string message = "ResolveOutage => exception caught";
                Logger.LogError(message, e);
                //todo throw;
            }

            return result;
        }
        #endregion
    }
}
