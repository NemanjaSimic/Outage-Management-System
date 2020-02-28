using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using Outage.Common.PubSub.OutageDataContract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OMSCommon.Mappers
{
    public class OutageMessageMapper
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private ConsumerMessageMapper consumerMapper;

        public OutageMessageMapper()
        {
            consumerMapper = new ConsumerMessageMapper(this);
        }

        public ActiveOutageMessage MapActiveOutage(ActiveOutage outage)
        {
            ActiveOutageMessage activeOutageMessage = new ActiveOutageMessage()
            {
                OutageId = outage.OutageId,
                OutageState = outage.OutageState,
                ReportTime = outage.ReportTime,
                IsolatedTime = outage.IsolatedTime,
                RepairedTime = outage.RepairedTime,
                OutageElementGid = outage.OutageElementGid,
                AffectedConsumers = consumerMapper.MapConsumers(outage.AffectedConsumers)
            };

            if (TryParseIsolationPointsFromCSVFormat(outage.DefaultIsolationPoints, out List<long> defaultIsolationPoints))
            {
                activeOutageMessage.DefaultIsolationPoints = defaultIsolationPoints;
            }

            if (TryParseIsolationPointsFromCSVFormat(outage.OptimumIsolationPoints, out List<long> optimumIsolationPoints))
            {
                activeOutageMessage.OptimumIsolationPoints = optimumIsolationPoints;
            }

            return activeOutageMessage;
        }

        public IEnumerable<ActiveOutageMessage> MapActiveOutages(IEnumerable<ActiveOutage> outages)
        {
            return outages.Select(o => MapActiveOutage(o)).ToList();
        }

        public ArchivedOutageMessage MapArchivedOutage(ArchivedOutage outage)
        {
            ArchivedOutageMessage archivedOutageMessage = new ArchivedOutageMessage
            {
                OutageId = outage.OutageId,
                ReportTime = outage.ReportTime,
                IsolatedTime = outage.IsolatedTime,
                RepairedTime = outage.RepairedTime,
                ArchiveTime = outage.ArchiveTime,
                OutageElementGid = outage.OutageElementGid,
                AffectedConsumers = consumerMapper.MapConsumers(outage.AffectedConsumers)
            };

            if(TryParseIsolationPointsFromCSVFormat(outage.DefaultIsolationPoints, out List<long> defaultIsolationPoints))
            {
                archivedOutageMessage.DefaultIsolationPoints = defaultIsolationPoints;
            }

            if (TryParseIsolationPointsFromCSVFormat(outage.OptimumIsolationPoints, out List<long> optimumIsolationPoints))
            {
                archivedOutageMessage.OptimumIsolationPoints = optimumIsolationPoints;
            }
            
            return archivedOutageMessage;
        }

        public IEnumerable<ArchivedOutageMessage> MapArchivedOutages(IEnumerable<ArchivedOutage> outages)
        {
            return outages.Select(o => MapArchivedOutage(o));
        }

        private IEnumerable<long> ParseIsolationPointsFromCSVFormat(string isolationPointsCSV)
        {
            if(isolationPointsCSV == null)
            {
                string message = "ParseIsolationPointsFromCSVFormat => argument is null";
                Logger.LogError(message);
                throw new ArgumentNullException(message);
            }
            
            List<long> isolationPoints = new List<long>();
            
            string[] points = isolationPointsCSV.Split('|');
            foreach (string point in points)
            {
                if(long.TryParse(point, out long isolationPointId))
                {
                    isolationPoints.Add(isolationPointId);
                }
            }

            return isolationPoints;
        }

        private bool TryParseIsolationPointsFromCSVFormat(string isolationPointsCSV, out List<long> isolationPoints)
        {
            isolationPoints = new List<long>();

            if (isolationPointsCSV == null)
            {
                string message = "ParseIsolationPointsFromCSVFormat => argument is null";
                Logger.LogError(message);
                return false;
            }

            string[] points = isolationPointsCSV.Split('|');
            foreach (string point in points)
            {
                if (long.TryParse(point, out long isolationPointId))
                {
                    isolationPoints.Add(isolationPointId);
                }
            }

            return true;
        }
    }
}
