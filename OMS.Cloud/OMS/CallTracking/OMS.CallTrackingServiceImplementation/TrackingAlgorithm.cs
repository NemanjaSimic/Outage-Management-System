using Common.OmsContracts.ModelProvider;
using Common.OmsContracts.OutageLifecycle;
using Common.PubSubContracts.DataContracts.CE;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts.Interfaces;
using OMS.Common.WcfClient.OMS;
using OMS.Common.WcfClient.OMS.Lifecycle;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.CallTrackingImplementation
{
    public class TrackingAlgorithm
	{

        private OutageTopologyModel outageTopologyModel;

        //TODO: Mozda reliable dic/queue
        private List<long> potentialOutages;
        private List<long> outages;
        private IReportOutageContract reportOutageClient;
        private IOutageModelReadAccessContract outageModelReadAccessClient;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public TrackingAlgorithm()
		{
            reportOutageClient = ReportOutageClient.CreateClient();
            outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
		}


        public async Task Start(List<long> calls)
        {
			Logger.LogDebug("Starting tracking algorithm.");

			//on every start tracking algorithm get up to date outage topology model
			outageTopologyModel = await outageModelReadAccessClient.GetTopologyModel();

            this.potentialOutages = LocateSwitchesUsingCalls(calls);
            this.outages = new List<long>();
            HashSet<long> visited = new HashSet<long>();
            bool foundOutage = false;
            long currentGid, previousGid;

            currentGid = this.potentialOutages[0];
            try
            {

                while (this.potentialOutages.Count > 0)
                {
                    currentGid = this.potentialOutages[0];
                    previousGid = currentGid;
                    this.outages.Add(currentGid);
                    outageTopologyModel.GetElementByGid(currentGid, out OutageTopologyElement topologyElement);
                    this.potentialOutages.Remove(currentGid);
                    while (topologyElement.DmsType != "ENERGYSOURCE" && !topologyElement.IsRemote && this.potentialOutages.Count > 0)
                    {
                        foundOutage = false;
                        if (TraceDFS(visited, topologyElement, foundOutage))
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
                string message = $"Tracing algorithm failed with error: {ex.Message}";
                Logger.LogError(message);
                Console.WriteLine(message);
            }

            foreach (var item in this.outages)
            {
                await reportOutageClient.ReportPotentialOutage(item, CommandOriginType.NON_SCADA_OUTAGE);
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

        private OutageTopologyElement GetSwitch(long gid)
        {
            OutageTopologyElement element;
            if (outageTopologyModel.GetElementByGid(gid, out element))
            {

                while (!IsSwitch(element.DmsType))
                {
                    if (!outageTopologyModel.GetElementByGid(element.FirstEnd, out element))
                    {
                        break; 
                    }

                }
            }
            return element;
        }

        private List<long> LocateSwitchesUsingCalls(List<long> registeredCalls)
        {
            OutageTopologyElement topologyElement;
            List<long> potentialSwitches = new List<long>();
            foreach (var call in registeredCalls)
            {
                topologyElement = GetSwitch(call);
                //Ako je switch remote odmah posle poziva,izbaciti iz liste? ili nesto drugo?
                if (topologyElement != null && !topologyElement.IsRemote)
				{
                    if (!potentialSwitches.Contains(topologyElement.Id))
                    {
                        potentialSwitches.Add(topologyElement.Id);
                    }
				}

            }

            return potentialSwitches;
        }

        public bool TraceDFS(HashSet<long> visited, OutageTopologyElement topologyElement, bool foundOutage)
        {

            if (this.potentialOutages.Count == 0)
            {
                return foundOutage;
            }

            if (this.potentialOutages.Contains(topologyElement.Id))
            {
                potentialOutages.Remove(topologyElement.Id);
                foundOutage = true;
            }
            visited.Add(topologyElement.Id);
            foreach (var item in topologyElement.SecondEnd)
            {
                if (outageTopologyModel.GetElementByGid(item, out topologyElement))
                {
                    if (topologyElement.DmsType != "ENERGYCONSUMER" && !visited.Contains(topologyElement.Id) && !topologyElement.IsRemote)
                        foundOutage = TraceDFS(visited, topologyElement, foundOutage);
                }
            }
            return foundOutage;

        }
    }
}
