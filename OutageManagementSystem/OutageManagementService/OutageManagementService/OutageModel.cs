using CECommon.Interfaces;
using CECommon.Model;
using Outage.Common;
using Outage.Common.OutageService.Interface;
using Outage.Common.OutageService.Model;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.OMS;
using Outage.Common.ServiceProxies.CalcualtionEngine;
using Outage.Common.ServiceProxies.PubSub;
using Outage.Common.UI;
using OutageDatabase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TopologyServiceClientMock;

namespace OutageManagementService
{
    public class OutageModel
    {
        private OutageTopologyModel topologyModel;

        public OutageTopologyModel TopologyModel
        {
            get
            {
                return topologyModel;
            }
            protected set
            {
                topologyModel = value;
            }
        }

        private ILogger logger;
        public ConcurrentQueue<long> EmailMsg;
        public List<long> CalledOutages;
        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private PublisherProxy publisherProxy = null;

        private PublisherProxy GetPublisherProxy()
        {
            //TODO: diskusija statefull vs stateless

            int numberOfTries = 0;
            int sleepInterval = 500;

            while (numberOfTries <= int.MaxValue)
            {
                try
                {
                    if (publisherProxy != null)
                    {
                        publisherProxy.Abort();
                        publisherProxy = null;
                    }

                    publisherProxy = new PublisherProxy(EndpointNames.PublisherEndpoint);
                    publisherProxy.Open();

                    if (publisherProxy.State == CommunicationState.Opened)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Exception on PublisherProxy initialization. Message: {ex.Message}";
                    Logger.LogError(message, ex);
                    publisherProxy = null;
                }
                finally
                {
                    numberOfTries++;
                    Logger.LogDebug($"OutageModel: PublisherProxy getter, try number: {numberOfTries}.");

                    if (numberOfTries >= 100)
                    {
                        sleepInterval = 1000;
                    }

                    Thread.Sleep(sleepInterval);
                }
            }

            return publisherProxy;
        }

        #region Proxies
        private OMSTopologyServiceProxy omsTopologyServiceProxy = null;

        private OMSTopologyServiceProxy GetTopologyProxy()
        {
            int numberOfTries = 0;
            int sleepInterval = 500;

            while (numberOfTries <= int.MaxValue)
            {
                try
                {
                    if (omsTopologyServiceProxy != null)
                    {
                        omsTopologyServiceProxy.Abort();
                        omsTopologyServiceProxy = null;
                    }

                    omsTopologyServiceProxy = new OMSTopologyServiceProxy(EndpointNames.TopologyOMSServiceEndpoint);
                    omsTopologyServiceProxy.Open();

                    if (omsTopologyServiceProxy.State == CommunicationState.Opened)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Exception on OMSTopologyServiceProxy initialization. Message: {ex.Message}";
                    Logger.LogWarn(message, ex);
                    omsTopologyServiceProxy = null;
                }
                finally
                {
                    numberOfTries++;
                    Logger.LogDebug($"OutageModel: OMSTopologyServiceProxy getter, try number: {numberOfTries}.");

                    if (numberOfTries >= 100)
                    {
                        sleepInterval = 1000;
                    }

                    Thread.Sleep(sleepInterval);
                }
            }

            return omsTopologyServiceProxy;
        }
        #endregion

        public OutageModel()
        {
            EmailMsg = new ConcurrentQueue<long>();
            CalledOutages = new List<long>();
            ImportTopologyModel();
        }

        private void ImportTopologyModel()
        {
            using (OMSTopologyServiceProxy omsTopologyProxy = GetTopologyProxy())
            {
                if (omsTopologyProxy != null)
                {
                    TopologyModel = (OutageTopologyModel)omsTopologyProxy.GetOMSModel();
                }
                else
                {
                    string message = "From method ImportTopologyModel(): TopologyServiceProxy is null.";
                    logger.LogError(message);
                    throw new NullReferenceException(message);
                }
            }
        }

        public bool ReportPotentialOutage(long gid)
        {
            bool success = false;

            List<long> affectedConsumers = new List<long>();

            //TODO: special case: potenitial outage is remote (and closed)...

            affectedConsumers = GetAffectedConsumers(gid);

            if (affectedConsumers.Count > 0)
            {
                ActiveOutage activeOutageInDb = null;
                using (OutageContext db = new OutageContext())
                {
                    try
                    {
                        activeOutageInDb = db.ActiveOutages.Add(new ActiveOutage { AffectedConsumers = GetAffectedConsumersString(affectedConsumers), ElementGid = gid, ReportTime = DateTime.Now });
                        db.SaveChanges();
                        Logger.LogDebug($"Outage on element with gid: 0x{activeOutageInDb.ElementGid:x16} is successfully stored in database");
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Error while adding active outage into database. ", e);
                        success = false;
                    }
                }
                //TODO: Publish
                if (activeOutageInDb != null)
                {
                    try
                    {
                        PublishActiveOutage(Topic.ACTIVE_OUTAGE, activeOutageInDb);
                        Logger.LogInfo($"Outage on element with gid: 0x{activeOutageInDb.ElementGid:x16} is successfully reported");
                        success = true;
                    }
                    catch (Exception e) //TODO: Exception over proxy or enum...
                    {
                        Logger.LogError("Error occured while trying to publish outage.", e);
                    }
                    
                }
            }
            else
            {
                Logger.LogInfo("There is no affected consumers, so reported outage is not valid.");
                success = false;
            }

            return success;
        }

        private string GetAffectedConsumersString(List<long> affectedConsumers)
        {
            StringBuilder sb = new StringBuilder();

            for(int i = 0; i < affectedConsumers.Count; i++)
            {
                sb.Append(affectedConsumers[i]);
                if (i < affectedConsumers.Count - 1)
                {
                    sb.Append("|");
                }
            }

            return sb.ToString();
        }

        private void PublishActiveOutage(Topic topic, OutageMessage outageMessage)
        {
            OutagePublication outagePublication = new OutagePublication(topic, outageMessage);

            using (PublisherProxy publisherProxy = GetPublisherProxy())
            {
                if (publisherProxy != null)
                {
                    publisherProxy.Publish(outagePublication);
                    Logger.LogInfo($"Outage service published data from topic: {outagePublication.Topic}");
                }
                else
                {
                    string errMsg = "Publisher proxy is null";
                    Logger.LogWarn(errMsg);
                    throw new NullReferenceException(errMsg);
                }
            }
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
                    IOutageTopologyElement topologyElement = topologyModel.OutageTopology[currentNode];

                    if (topologyElement.SecondEnd.Count == 0 && topologyElement.DmsType == "ENERGYCONSUMER") 
                    {
                        affectedConsumers.Add(currentNode);
                    }

                    foreach(long adjNode in topologyElement.SecondEnd)
                    {
                        nodesToBeVisited.Push(adjNode);
                    }
                }
            }

            return affectedConsumers;
        }
    }
}
