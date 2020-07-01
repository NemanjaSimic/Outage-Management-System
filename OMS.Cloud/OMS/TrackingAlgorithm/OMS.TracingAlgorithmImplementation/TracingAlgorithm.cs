using Common.OMS;
using Common.OmsContracts.TracingAlgorithm;
using Microsoft.Azure.Storage.Queue;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.OMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.TracingAlgorithmImplementation
{
    public class TracingAlgorithm : ITracingAlgorithmContract
    {
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        private List<long> potentialOutages;
        private List<long> outages;
        private IOutageTopologyModel topologyModel;
        private OutageModelReadAccessClient modelReadAccessClient;
        public TracingAlgorithm()
        {
            potentialOutages = new List<long>();
            outages = new List<long>();
            modelReadAccessClient = OutageModelReadAccessClient.CreateClient();      

        }
        public async Task StartTracingAlgorithm(List<long> calls)
        {
            topologyModel = await this.modelReadAccessClient.GetTopologyModel();
            this.potentialOutages = LocateSwitchesUsingCalls(calls);
            this.outages = new List<long>();
            HashSet<long> visited = new HashSet<long>();
            bool FoundOutage = false;
            long currentGid, previousGid;

            try
            {
                while (this.potentialOutages.Count > 0)
                {
                    currentGid = this.potentialOutages[0];
                    previousGid = currentGid;
                    this.outages.Add(currentGid);
                    this.topologyModel.GetElementByGid(currentGid, out IOutageTopologyElement topologyElement);
                    this.potentialOutages.Remove(currentGid);
                    while (topologyElement.DmsType != "ENERGYSOURCE" && !topologyElement.IsRemote && this.potentialOutages.Count > 0)
                    {
                        FoundOutage = false;
                        if (TraceDFS(visited, topologyElement, FoundOutage))
                        {
                            this.outages.Remove(previousGid);
                            this.outages.Add(currentGid);
                            previousGid = currentGid;
                        }
                        topologyElement = GetSwitch(topologyElement.FirstEnd);
                        if (topologyElement == null) break;
                        currentGid = topologyElement.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Tracing algorithm failed with error: {0}", ex.Message);
            }

            //TO DO : ReportPotentialOutage
        }

        #region Private Methods
        private bool IsSwitch(string dmsType)
        {
            bool retVal = false;
            switch (dmsType)
            {
                case "FUSE":
                case "DISCONNECTOR":
                case "BREAKER":
                case "LOADBREAKSWITCH":
                case "ENERGYSOURCE":
                    retVal = true;
                    break;
                default:
                    break;
            }
            return retVal;
        }
        private IOutageTopologyElement GetSwitch(long gid)
        {
            IOutageTopologyElement element;
            if (this.topologyModel.GetElementByGid(gid, out element))
            {

                while (!IsSwitch(element.DmsType))
                {
                    if (!this.topologyModel.GetElementByGid(element.FirstEnd, out element)) break;

                }
            }
            return element;
        }
        private List<long> LocateSwitchesUsingCalls(List<long> registeredCalls)
        {
            IOutageTopologyElement topologyElement;
            List<long> outage = new List<long>();
            foreach (var call in registeredCalls)
            {
                topologyElement = GetSwitch(call);
                //Ako je switch remote odmah posle poziva,izbaciti iz liste? ili nesto drugo?
                if (topologyElement != null && !topologyElement.IsRemote)
                    if (!outage.Contains(topologyElement.Id)) outage.Add(topologyElement.Id);

            }

            return outage;
        }

        #endregion




        public bool TraceDFS(HashSet<long> visited, IOutageTopologyElement topologyElement, bool FoundOutage)
        {

            if (this.potentialOutages.Count == 0) return FoundOutage;

            if (this.potentialOutages.Contains(topologyElement.Id))
            {
                potentialOutages.Remove(topologyElement.Id);
                FoundOutage = true;
            }
            visited.Add(topologyElement.Id);
            foreach (var item in topologyElement.SecondEnd)
            {
                if (this.topologyModel.GetElementByGid(item, out topologyElement))
                {
                    if (topologyElement.DmsType != "ENERGYCONSUMER" && !visited.Contains(topologyElement.Id) && !topologyElement.IsRemote)
                        FoundOutage = TraceDFS(visited, topologyElement, FoundOutage);
                }
            }
            return FoundOutage;

        }
        private List<long> ConvertCloudQueueToList(CloudQueue cloudQueue)
        {
            List<long> retVal = new List<long>();
            int? count = cloudQueue.ApproximateMessageCount;

            if (count > 0)
            {
                var messages = cloudQueue.GetMessages((int)count);
                cloudQueue.Clear();
                foreach (var item in messages)
                {
                    retVal.Add(long.Parse(item.AsString));
                }
            }

            return retVal;
        }
    }
}
