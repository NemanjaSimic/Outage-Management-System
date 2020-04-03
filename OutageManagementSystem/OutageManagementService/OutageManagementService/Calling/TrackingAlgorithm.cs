using Outage.Common.OutageService.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.Calling
{
    public class TrackingAlgorithm
    {
        private OutageModel outageModel;
        private List<long> potentialOutages;
        private List<long> outages;
        public TrackingAlgorithm(OutageModel outageModel)
        {
            this.outageModel = outageModel;
        }

        public void Start(ConcurrentQueue<long> calls)
        {
            this.potentialOutages = LocateSwitchesUsingCalls(calls.ToList());
            this.outages = new List<long>();
            HashSet<long> visited = new HashSet<long>();
            bool FoundOutage = false;
            long currentGid, previousGid;

            currentGid = this.potentialOutages[0];
            try
            {



                while (this.potentialOutages.Count > 0)
                {
                    currentGid = this.potentialOutages[0];
                    previousGid = currentGid;
                    this.outages.Add(currentGid);
                    this.outageModel.TopologyModel.GetElementByGid(currentGid, out IOutageTopologyElement topologyElement);
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

            foreach (var item in this.outages)
            {
                this.outageModel.ReportPotentialOutage(item);
            }
        }
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
            if (this.outageModel.TopologyModel.GetElementByGid(gid, out element))
            {

                while (!IsSwitch(element.DmsType))
                {
                    if (!this.outageModel.TopologyModel.GetElementByGid(element.FirstEnd, out element)) break;

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
                if (this.outageModel.TopologyModel.GetElementByGid(item, out topologyElement))
                {
                    if (topologyElement.DmsType != "ENERGYCONSUMER" && !visited.Contains(topologyElement.Id) && !topologyElement.IsRemote)
                        FoundOutage = TraceDFS(visited, topologyElement, FoundOutage);
                }
            }
            return FoundOutage;

        }
    }
}
