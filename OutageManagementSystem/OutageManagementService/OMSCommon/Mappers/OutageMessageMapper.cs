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
        private EquipmentMapper equipmentMapper;

        public OutageMessageMapper()
        {
            consumerMapper = new ConsumerMessageMapper(this);
            equipmentMapper = new EquipmentMapper(this);
        }

        public OutageMessage MapOutageEntity(OutageEntity outage)
        {
            OutageMessage outageMessage;

            if (outage.OutageState != OutageState.ARCHIVED)
            {
                outageMessage = new ActiveOutageMessage()
                {
                    OutageId = outage.OutageId,
                    OutageState = outage.OutageState,
                    ReportTime = outage.ReportTime,
                    IsolatedTime = outage.IsolatedTime,
                    RepairedTime = outage.RepairedTime,
                    OutageElementGid = outage.OutageElementGid,
                    DefaultIsolationPoints = equipmentMapper.MapEquipments(outage.DefaultIsolationPoints),
                    OptimumIsolationPoints = equipmentMapper.MapEquipments(outage.OptimumIsolationPoints),
                    AffectedConsumers = consumerMapper.MapConsumers(outage.AffectedConsumers),
                    IsResolveConditionValidated = outage.IsResolveConditionValidated,
                };
            }
            else
            {
                outageMessage = new ArchivedOutageMessage
                {
                    OutageId = outage.OutageId,
                    ReportTime = outage.ReportTime,
                    IsolatedTime = outage.IsolatedTime,
                    RepairedTime = outage.RepairedTime,
                    ArchivedTime = outage.ArchivedTime ?? DateTime.UtcNow,
                    OutageElementGid = outage.OutageElementGid,
                    DefaultIsolationPoints = equipmentMapper.MapEquipments(outage.DefaultIsolationPoints),
                    OptimumIsolationPoints = equipmentMapper.MapEquipments(outage.OptimumIsolationPoints),
                    AffectedConsumers = consumerMapper.MapConsumers(outage.AffectedConsumers)
                };
            }
            

            return outageMessage;
        }

        public ActiveOutageMessage MapOutageEntityToActive(OutageEntity outage)
        {
            ActiveOutageMessage outageMessage;

            if (outage.OutageState != OutageState.ARCHIVED)
            {
                outageMessage = new ActiveOutageMessage()
                {
                    OutageId = outage.OutageId,
                    OutageState = outage.OutageState,
                    ReportTime = outage.ReportTime,
                    IsolatedTime = outage.IsolatedTime,
                    RepairedTime = outage.RepairedTime,
                    OutageElementGid = outage.OutageElementGid,
                    DefaultIsolationPoints = equipmentMapper.MapEquipments(outage.DefaultIsolationPoints),
                    OptimumIsolationPoints = equipmentMapper.MapEquipments(outage.OptimumIsolationPoints),
                    AffectedConsumers = consumerMapper.MapConsumers(outage.AffectedConsumers),
                    IsResolveConditionValidated = outage.IsResolveConditionValidated,
                };
            }
            else
            {
                throw new ArgumentException($"MapOutageEntityToActive => Outage state: { OutageState.ARCHIVED}.");
            }


            return outageMessage;
        }

        public ArchivedOutageMessage MapOutageEntityToArchived(OutageEntity outage)
        {
            ArchivedOutageMessage outageMessage;

            if (outage.OutageState == OutageState.ARCHIVED)
            {
                outageMessage = new ArchivedOutageMessage
                {
                    OutageId = outage.OutageId,
                    ReportTime = outage.ReportTime,
                    IsolatedTime = outage.IsolatedTime,
                    RepairedTime = outage.RepairedTime,
                    ArchivedTime = outage.ArchivedTime ?? DateTime.UtcNow,
                    OutageElementGid = outage.OutageElementGid,
                    DefaultIsolationPoints = equipmentMapper.MapEquipments(outage.DefaultIsolationPoints),
                    OptimumIsolationPoints = equipmentMapper.MapEquipments(outage.OptimumIsolationPoints),
                    AffectedConsumers = consumerMapper.MapConsumers(outage.AffectedConsumers)
                };
            }
            else
            {
                throw new ArgumentException($"MapOutageEntityToArchived => Outage state: {outage.OutageState}, but {OutageState.ARCHIVED} was expected.");
            }


            return outageMessage;
        }

        public List<ConsumerHistorical> MapOutageToConsumerHistorical(List<Consumer> affectedConsumers, long outageId, DatabaseOperation databaseOperation)
        {
            List<ConsumerHistorical> retVal = new List<ConsumerHistorical>(affectedConsumers.Count);
            foreach (Consumer consumer in affectedConsumers)
            {
                retVal.Add(
                    new ConsumerHistorical()
                    {
                        ConsumerId = consumer.ConsumerId,
                        OutageId = outageId,
                        OperationTime = DateTime.Now,
                        DatabaseOperation = databaseOperation
                    }
                );
            }
            return retVal;
        }

        public List<EquipmentHistorical> MapOutageToEquipmentHistorical(List<Equipment> affectedEquipment, long outageId, DatabaseOperation databaseOperation)
        {
            List<EquipmentHistorical> retVal = new List<EquipmentHistorical>(affectedEquipment.Count);
            foreach (Equipment equipment in affectedEquipment)
            {
                retVal.Add(new EquipmentHistorical
                {
                    EquipmentId = equipment.EquipmentId,
                    OutageId = outageId,
                    OperationTime = DateTime.Now,
                    DatabaseOperation = databaseOperation
                });
            }

            return retVal;
        }

        public IEnumerable<OutageMessage> MapOutageEntities(IEnumerable<OutageEntity> outages)
        {
            return outages.Select(o => MapOutageEntity(o)).ToList();
        }

        public IEnumerable<ActiveOutageMessage> MapOutageEntitiesToActive(IEnumerable<OutageEntity> outages)
        {
            return outages.Where(o => o.OutageState != OutageState.ARCHIVED).Select(o => MapOutageEntityToActive(o)).ToList();
        }

        public IEnumerable<ArchivedOutageMessage> MapOutageEntitiesToArchived(IEnumerable<OutageEntity> outages)
        {
            return outages.Where(o => o.OutageState == OutageState.ARCHIVED).Select(o => MapOutageEntityToArchived(o)).ToList();
        }

        #region Obsolete
        [Obsolete]
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
        [Obsolete]
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
        #endregion
    }
}
