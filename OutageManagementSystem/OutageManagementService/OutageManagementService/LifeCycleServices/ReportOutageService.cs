using OMSCommon.Mappers;
using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using Outage.Common.OutageService.Interface;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.LifeCycleServices
{
    public class ReportOutageService
    {
        public OutageModel outageModel { get; set; }
        private UnitOfWork dbContext;

        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }
        public OutageMessageMapper outageMessageMapper;


        public ReportOutageService(OutageModel outageModel)
        {
            this.outageModel = outageModel;
            dbContext = outageModel.dbContext;
            outageMessageMapper = new OutageMessageMapper();
        }

        public bool ReportPotentialOutage(long gid)
        {
            bool success = false;
            Logger.LogDebug($"Reporting outage for gid: 0x{gid:X16}");

            if (outageModel.commandedElements.Contains(gid) || outageModel.optimumIsolationPoints.Contains(gid))
            {
                return false;
            }

            List<long> affectedConsumersIds = new List<long>();


            affectedConsumersIds = GetAffectedConsumers(gid);

            if (affectedConsumersIds.Count == 0)
            {
                Logger.LogInfo("There is no affected consumers => outage report is not valid.");
                return false;
            }

            OutageEntity activeOutageDbEntity = null;

            if (dbContext.OutageRepository.Find(o => o.OutageElementGid == gid && o.OutageState != OutageState.ARCHIVED).FirstOrDefault() != null)
            {
                Logger.LogWarn($"Malfunction on element with gid: 0x{gid:x16} has already been reported.");
                return false;
            }

            List<Consumer> consumerDbEntities = outageModel.GetAffectedConsumersFromDatabase(affectedConsumersIds);

            if (consumerDbEntities.Count != affectedConsumersIds.Count)
            {
                Logger.LogWarn("Some of affected consumers are not present in database");
                return false;
            }

            long recloserId;

            try
            {
                recloserId = outageModel.GetRecloserForHeadBreaker(gid);
            }
            catch (Exception e)
            {
                throw e;
            }

            List<Equipment> defaultIsolationPoints = outageModel.GetEquipmentEntity(new List<long> { gid, recloserId });

            OutageEntity createdActiveOutage = new OutageEntity
            {
                AffectedConsumers = consumerDbEntities,
                OutageState = OutageState.CREATED,
                ReportTime = DateTime.UtcNow,
                DefaultIsolationPoints = defaultIsolationPoints,
            };

            activeOutageDbEntity = dbContext.OutageRepository.Add(createdActiveOutage);

            try
            {
                dbContext.Complete();
                Logger.LogDebug($"Outage on element with gid: 0x{activeOutageDbEntity.OutageElementGid:x16} is successfully stored in database.");
                success = true;
            }
            catch (Exception e)
            {
                string message = "OutageModel::ReportPotentialOutage method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message})");

                //TODO: da li je dobar handle?
                dbContext.Dispose();
                dbContext = new UnitOfWork();
                success = false;
            }

            if (success && activeOutageDbEntity != null)
            {
                try
                {
                    success = outageModel.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(activeOutageDbEntity));

                    if (success)
                    {
                        Logger.LogInfo($"Outage on element with gid: 0x{activeOutageDbEntity.OutageElementGid:x16} is successfully published");
                    }
                }
                catch (Exception e) //TODO: Exception over proxy or enum...
                {
                    Logger.LogError("OutageModel::ReportPotentialOutage => exception on PublishActiveOutage()", e);
                    success = false;
                }
            }
            return success;
        }

        private List<long> GetAffectedConsumers(long potentialOutageGid)
        {
            List<long> affectedConsumers = new List<long>();
            Stack<long> nodesToBeVisited = new Stack<long>();
            nodesToBeVisited.Push(potentialOutageGid);
            HashSet<long> visited = new HashSet<long>();


            while (nodesToBeVisited.Count > 0)
            {
                long currentNode = nodesToBeVisited.Pop();

                if (!visited.Contains(currentNode))
                {
                    visited.Add(currentNode);

                    if (outageModel.TopologyModel.OutageTopology.ContainsKey(currentNode))
                    {
                        IOutageTopologyElement topologyElement = outageModel.TopologyModel.OutageTopology[currentNode];

                        if (topologyElement.SecondEnd.Count == 0 && topologyElement.DmsType == "ENERGYCONSUMER" /*&& !topologyElement.IsActive*/)
                        {
                            affectedConsumers.Add(currentNode);
                        }

                        foreach (long adjNode in topologyElement.SecondEnd)
                        {
                            nodesToBeVisited.Push(adjNode);
                        }
                    }
                    else
                    {
                        //TOOD
                        string message = $"GID: 0x{currentNode:X16} not found in topologyModel.OutageTopology dictionary....";
                        Logger.LogError(message);
                        Console.WriteLine(message);
                    }
                }
            }

            return affectedConsumers;
        }

    }
}
