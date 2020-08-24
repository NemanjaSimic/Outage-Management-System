using Common.OmsContracts.HistoryDBManager;
using Common.OmsContracts.OutageLifecycle;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.Interfaces;
using OMS.Common.WcfClient.OMS;
using OMS.Common.WcfClient.OMS.Lifecycle;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReliableDictionaryNames = Common.OMS.ReliableDictionaryNames;

namespace OMS.ModelProviderImplementation
{
    public class OutageModel: INotifySubscriberContract
	{
		private readonly string baseLogString;
		private readonly IReliableStateManager stateManager;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		
		private IHistoryDBManagerContract historyDBManagerClient;
		private IReportOutageContract reportOutageClient;

		
		public OutageModel(IReliableStateManager stateManager)
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

			isTopologyModelInitialized = false;
			isCommandedElementsInitialized = false;
			isOptimumIsolatioPointsInitialized = false;
			isPotentialOutageInitialized = false;

			this.stateManager = stateManager;
			this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
		}

		#region ReliableDictionaryAccess
		private bool isTopologyModelInitialized;
		private bool isCommandedElementsInitialized;
		private bool isOptimumIsolatioPointsInitialized;
		private bool isPotentialOutageInitialized;
		public bool ReliableDictionariesInitialized
		{
			get
			{
				return isTopologyModelInitialized &&
					   isCommandedElementsInitialized &&
					   isOptimumIsolatioPointsInitialized &&
					   isPotentialOutageInitialized;
			}
		}

		private ReliableDictionaryAccess<long, OutageTopologyModel> topologyModel;
		public ReliableDictionaryAccess<long, OutageTopologyModel> TopologyModel
		{
			get
			{
				return topologyModel ?? (ReliableDictionaryAccess<long, OutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.OutageTopologyModel).Result);
			}

		}

		private ReliableDictionaryAccess<long,long> commandedElements;
		public ReliableDictionaryAccess<long,long> CommandedElements
		{
			get { return commandedElements; }
		}

		private ReliableDictionaryAccess<long,long> optimumIsloationPoints;
		public ReliableDictionaryAccess<long,long> OptimumIsolatioPoints
		{
			get { return optimumIsloationPoints; }
		
		}

		private ReliableDictionaryAccess<long,CommandOriginType> potentialOutage;
		public ReliableDictionaryAccess<long, CommandOriginType> PotentialOutage
		{
			get { return potentialOutage; }
		}

		private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
		{
			if (e.Action == NotifyStateManagerChangedAction.Add)
			{
				var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
				string reliableStateName = operation.ReliableState.Name.AbsolutePath;

				if (reliableStateName == ReliableDictionaryNames.OutageTopologyModel)
				{
					topologyModel = await ReliableDictionaryAccess<long, OutageTopologyModel>.Create(this.stateManager, ReliableDictionaryNames.OutageTopologyModel);
					this.isTopologyModelInitialized = true;
				}
				else if (reliableStateName == ReliableDictionaryNames.CommandedElements)
				{
					commandedElements = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.CommandedElements);
					this.isCommandedElementsInitialized = true;
				}
				else if (reliableStateName == ReliableDictionaryNames.OptimumIsolatioPoints)
				{
					optimumIsloationPoints = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.OptimumIsolatioPoints);
					this.isOptimumIsolatioPointsInitialized = true;
				}
				else if (reliableStateName == ReliableDictionaryNames.PotentialOutage)
				{
					potentialOutage = await ReliableDictionaryAccess<long, CommandOriginType>.Create(this.stateManager, ReliableDictionaryNames.PotentialOutage);
					this.isPotentialOutageInitialized = true;
				}
			}
		}
		#endregion

		#region INotifySubscriberContract
		private readonly string subscriberUri = MicroserviceNames.OmsModelProviderService;

		public async Task Notify(IPublishableMessage message, string publisherName)
		{
			while (!ReliableDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

			//if OMSModelMessage
			if (message is OMSModelMessage omsModelMessage)
			{
				this.historyDBManagerClient = HistoryDBManagerClient.CreateClient();
				this.reportOutageClient = ReportOutageClient.CreateClient();

				OutageTopologyModel topology = omsModelMessage.OutageTopologyModel;
				await TopologyModel.SetAsync(0, topology);
				
				HashSet<long> energizedConsumers = new HashSet<long>();
				foreach (var element in topology.OutageTopology.Values)
				{
					if (element.DmsType.Equals(DMSType.ENERGYCONSUMER.ToString()))
					{
						if (element.IsActive)
						{
							energizedConsumers.Add(element.Id);
						}
					}
				}

				await historyDBManagerClient.OnConsumersEnergized(energizedConsumers);
				var potentialOutages = await PotentialOutage.GetEnumerableDictionaryAsync(); 

				Task[] reportOutageTasks = new Task[potentialOutages.Count];
				int index = 0;
				foreach (var item in potentialOutages)
				{
					reportOutageTasks[index] = reportOutageClient.ReportPotentialOutage(item.Key, item.Value);
					reportOutageTasks[index].Start();
					index++;
				}
				
				Task.WaitAll(reportOutageTasks);
				await PotentialOutage.ClearAsync();
			}
			else
			{
				Logger.LogWarning("OutageModel::Notify => UNKNOWN message type. OMSModelMessage expected.");
			}			
		}

		public async Task<string> GetSubscriberName()
		{
			return this.subscriberUri;
		}

		public Task<bool> IsAlive()
		{
			return Task.Run(() => { return true; });
		}
		#endregion
	}
}
