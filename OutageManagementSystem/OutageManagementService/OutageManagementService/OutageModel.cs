using CECommon.Interfaces;
using CECommon.Model;
using Outage.Common;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.OMS;
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

        public UIModel topology = new UIModel();
        public TopologyModel topologyModel = new TopologyModel();
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
        private TopologyServiceProxy topologyProxy = null;

        private TopologyServiceProxy GetTopologyProxy()
        {
            int numberOfTries = 0;
            int sleepInterval = 500;

            while (numberOfTries <= int.MaxValue)
            {
                try
                {
                    if (topologyProxy != null)
                    {
                        topologyProxy.Abort();
                        topologyProxy = null;
                    }

                    topologyProxy = new TopologyServiceProxy(EndpointNames.TopologyServiceEndpoint);
                    topologyProxy.Open();

                    if (topologyProxy.State == CommunicationState.Opened)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Exception on TopologyServiceProxy initialization. Message: {ex.Message}";
                    Logger.LogError(message, ex);
                    topologyProxy = null;
                }
                finally
                {
                    numberOfTries++;
                    Logger.LogDebug($"OutageModel: TopologyServiceProxy getter, try number: {numberOfTries}.");

                    if (numberOfTries >= 100)
                    {
                        sleepInterval = 1000;
                    }

                    Thread.Sleep(sleepInterval);
                }
            }

            return topologyProxy;
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
            using (TopologyServiceProxy topologyServiceProxy = GetTopologyProxy())
            {
                if (topologyServiceProxy != null)
                {
                    topology = topologyServiceProxy.GetTopology();
                    //PrintUI(topology);
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
                        activeOutageInDb = db.ActiveOutages.Add(new ActiveOutage { AffectedConsumers = affectedConsumers, ElementGid = gid, ReportTime = DateTime.Now }); 
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
                        success = true;
                    }
                    catch (Exception) //TODO: Exception over proxy or enum...
                    {
                    }
                    
                }
            }
            else
            {
                success = false;
            }

            return success;
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
                    ITopologyElement topologyElement = topologyModel.TopologyElements[currentNode];

                    if (topologyElement.SecondEnd.Count == 0 && topologyElement.DmsType == "ENERGYSOURCE") 
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
