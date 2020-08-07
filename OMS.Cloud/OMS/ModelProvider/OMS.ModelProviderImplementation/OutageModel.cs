using Common.CE;
using Common.OMS;
using Common.OmsContracts.HistoryDBManager;
using Common.OmsContracts.ModelProvider;
using Common.OmsContracts.OutageLifecycle;
using Common.PubSub;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSub;
using OMS.Common.PubSubContracts;
using OMS.Common.WcfClient.OMS;
using OMS.Common.WcfClient.OMS.Lifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliableDictionaryNames = Common.OMS.ReliableDictionaryNames;

namespace OMS.ModelProviderImplementation
{
    public class OutageModel: INotifySubscriberContract
	{
		private readonly IReliableStateManager stateManager;
		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		private IHistoryDBManagerContract historyDBManagerClient;
		private IOutageModelReadAccessContract outageModelReadAccessClient;
		private IOutageModelUpdateAccessContract outageModelUpdateAccessClient;
		private IReportOutageContract reportOutageClient;
		public OutageModel(IReliableStateManager stateManager)
		{
			this.stateManager = stateManager;
			this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
			this.subscriberUri = "OutageModel"; //TODO: Service defines, name

			this.historyDBManagerClient = HistoryDBManagerClient.CreateClient();
			this.outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
			this.outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
			this.reportOutageClient = ReportOutageClient.CreateClient();
		}
		#region ReliableDictionaryAccess

		private ReliableDictionaryAccess<long, IOutageTopologyModel> topologyModel;

		public ReliableDictionaryAccess<long, IOutageTopologyModel> TopologyModel
		{
			get
			{
				return topologyModel ?? (ReliableDictionaryAccess<long, IOutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.OutageTopologyModel).Result);
			}

		}

		private ReliableDictionaryAccess<long,long> commandedElements;

		public ReliableDictionaryAccess<long,long> CommandedElements
		{
			get { return commandedElements ?? (commandedElements = ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.CommandedElements).Result); }
		}

		private ReliableDictionaryAccess<long,long> optimumIsloationPoints;

		public ReliableDictionaryAccess<long,long> OptimumIsolatioPoints
		{
			get { return optimumIsloationPoints ?? (optimumIsloationPoints =  ReliableDictionaryAccess<long,long>.Create(this.stateManager,ReliableDictionaryNames.OptimumIsolatioPoints).Result); }
		
		}

		private ReliableDictionaryAccess<long,CommandOriginType> potentialOutage;

		public ReliableDictionaryAccess<long, CommandOriginType> PotentialOutage
		{
			get { return potentialOutage ?? (potentialOutage = ReliableDictionaryAccess<long,CommandOriginType>.Create(this.stateManager,ReliableDictionaryNames.PotentialOutage).Result); }
		}
		#endregion

		private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
		{
			if(e.Action == NotifyStateManagerChangedAction.Add)
			{
				var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
				string reliableStateName = operation.ReliableState.Name.AbsolutePath;
				if(reliableStateName == ReliableDictionaryNames.OutageTopologyModel)
				{
					topologyModel = await ReliableDictionaryAccess<long, IOutageTopologyModel>.Create(this.stateManager, ReliableDictionaryNames.OutageTopologyModel);
				}else if(reliableStateName == ReliableDictionaryNames.CommandedElements)
				{
					commandedElements = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.CommandedElements);
				}else if(reliableStateName == ReliableDictionaryNames.OptimumIsolatioPoints)
				{
					optimumIsloationPoints = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.OptimumIsolatioPoints);
				}else if(reliableStateName == ReliableDictionaryNames.PotentialOutage)
				{
					potentialOutage = await ReliableDictionaryAccess<long, CommandOriginType>.Create(this.stateManager, ReliableDictionaryNames.PotentialOutage);
				}

			}
		}
		#region INotifySubscriberContract
		private readonly string subscriberUri;

		public async Task Notify(IPublishableMessage message, string publisherName)
		{
			//if OMSModelMessage
			if (message is OMSModelMessage omsModelMessage)
			{
				var topology = omsModelMessage.OutageTopologyModel;
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
				Dictionary<long, CommandOriginType> potentialOutages = await outageModelReadAccessClient.GetPotentialOutage();

				Task[] reportOutageTasks = new Task[potentialOutages.Count];
				int index = 0;
				foreach (var item in potentialOutages)
				{
					reportOutageTasks[index] = reportOutageClient.ReportPotentialOutage(item.Key, item.Value);
					reportOutageTasks[index].Start();
					index++;
				}
				Task.WaitAll(reportOutageTasks);
				await outageModelUpdateAccessClient.UpdatePotentialOutage(0, 0, ModelUpdateOperationType.CLEAR);
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
		#endregion
	}
}
