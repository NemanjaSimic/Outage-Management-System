using Common.OMS;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ModelProviderImplementation
{
    public class OutageModel
    {
		private readonly IReliableStateManager stateManager;
		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		public OutageModel(IReliableStateManager stateManager)
		{
			this.stateManager = stateManager;
			this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
		}
		private bool IsTopologyModelInitialized;

		private ReliableDictionaryAccess<long, IOutageTopologyModel> topologyModel;

		public ReliableDictionaryAccess<long, IOutageTopologyModel> TopologyModel
		{
			get
			{
				if (topologyModel == null)
				{
					topologyModel = ReliableDictionaryAccess<long, IOutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.OutageTopologyModel).Result;
					//Get topologyModel from CE
					IOutageTopologyModel model = null;
					topologyModel.SetAsync(0, model);
				}
				return topologyModel;
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
	}
}
