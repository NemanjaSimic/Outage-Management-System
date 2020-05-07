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
using OutageDatabase.Repository;
using OutageManagementService.LifeCycleServices;
using Outage.Common.OutageService;
using OutageManagementService.Report;
using Outage.Common.OutageService.Interface;

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
        public static SwitchClosed SwitchClosed { get; set; }
        public static ReportOutageService reportOutageService;
        public static IsolateOutageService isolateOutageService;
        public static ResolveOutageService resolveOutageService;
        public static ValidateResolveConditionsService validateResolveConditionsService;
        public static SendRepairCrewService sendRepairCrewService;
        public static SendLocationIsolationCrewService sendLocationIsolationCrewService;

        #region IOutageAccessContract
        public IEnumerable<ActiveOutageMessage> GetActiveOutages()
        {
            OutageMessageMapper mapper = new OutageMessageMapper();

            List<ActiveOutageMessage> activeOutages = new List<ActiveOutageMessage>();

            using (UnitOfWork db = new UnitOfWork())
            {
                activeOutages.AddRange(mapper.MapOutageEntitiesToActive(db.OutageRepository.GetAllActive()));
            }

            return activeOutages;
        }

        public IEnumerable<ArchivedOutageMessage> GetArchivedOutages()
        {
            OutageMessageMapper mapper = new OutageMessageMapper();

            List<ArchivedOutageMessage> archivedOutages = new List<ArchivedOutageMessage>();

            using (UnitOfWork db = new UnitOfWork())
            {
                archivedOutages.AddRange(mapper.MapOutageEntitiesToArchived(db.OutageRepository.GetAllArchived()));
            }

            return archivedOutages;
        }
        #endregion

        #region IReportPotentialOutageContract
        public bool ReportPotentialOutage(long elementGid, CommandOriginType commandOriginType)
        {
            bool result;

            try
            {

                result = reportOutageService.ReportPotentialOutage(elementGid); //TODO: enum (error, noAffectedConsumers, success,...)

                //result = outageModel.ReportPotentialOutage(elementGid, commandOriginType); //TODO: enum (error, noAffectedConsumers, success,...)

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
                isolateOutageService.IsolateOutage(outageId);
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
                result = sendRepairCrewService.SendRepairCrew(outageId);
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
                result = sendLocationIsolationCrewService.SendLocationIsolationCrew(outageId);
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
                result = validateResolveConditionsService.ValidateResolveConditions(outageId);
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
                result = resolveOutageService.ResolveOutage(outageId);
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

        public OutageReport GenerateReport(ReportOptions options)
        {
            try
            {
                var reportService = new ReportingService();
                var report = reportService.GenerateReport(options);
                return report;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void OnSwitchClose(long elementGid)
        {
            SwitchClosed?.Invoke(elementGid);
        }
    }
}
