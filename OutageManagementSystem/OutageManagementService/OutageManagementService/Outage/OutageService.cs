using Outage.Common;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.OMS;
using OutageDatabase;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System;
using OMSCommon.OutageDatabaseModel;
using OMSCommon.Mappers;

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
        public IEnumerable<ActiveOutageMessage> GetActiveOutages()
        {
            OutageMessageMapper mapper = new OutageMessageMapper();

            List<ActiveOutageMessage> activeOutages = new List<ActiveOutageMessage>();

            using (OutageContext db = new OutageContext())
            {
                activeOutages.AddRange(mapper.MapActiveOutages(db.ActiveOutages.Include(a => a.AffectedConsumers)));
            }

            return activeOutages;
        }

        public IEnumerable<ArchivedOutageMessage> GetArchivedOutages()
        {
            OutageMessageMapper mapper = new OutageMessageMapper();

            List<ArchivedOutageMessage> archivedOutages = new List<ArchivedOutageMessage>();

            using (OutageContext db = new OutageContext())
            {
                archivedOutages.AddRange(mapper.MapArchivedOutages(db.ArchivedOutages.Include(a => a.AffectedConsumers)));
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
                outageModel.IsolateOutage(outageId);
                result = true;
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
