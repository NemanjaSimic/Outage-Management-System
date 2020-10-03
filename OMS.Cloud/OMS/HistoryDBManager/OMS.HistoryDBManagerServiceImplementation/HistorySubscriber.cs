using Common.OMS;
using Common.OmsContracts.HistoryDBManager;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.CE.UIModels;
using Common.PubSubContracts.DataContracts.OMS;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.NmsContracts;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerImplementation
{
	public class HistorySubscriber : INotifySubscriberContract
	{
		private readonly string baseLogString;
		private readonly IReliableStateManager stateManager;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		private IHistoryDBManagerContract historyDBManager;

		private bool isActiveOutagesInitialized = true;

		private ReliableDictionaryAccess<long, ActiveOutageMessage> activeOutages;
		private ReliableDictionaryAccess<long, ActiveOutageMessage> ActiveOutages
		{
			get { return activeOutages; }
		}

		private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
		{
			if (e.Action == NotifyStateManagerChangedAction.Add)
			{
				var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
				string reliableStateName = operation.ReliableState.Name.AbsolutePath;
				if (reliableStateName == ReliableDictionaryNames.ActiveOutages)
				{
					activeOutages = await ReliableDictionaryAccess<long, ActiveOutageMessage>.Create(this.stateManager, ReliableDictionaryNames.ActiveOutages);
					this.isActiveOutagesInitialized = true;
				}
			}
		}

		public HistorySubscriber(IReliableStateManager stateManager, IHistoryDBManagerContract historyDBManager)
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);

			this.historyDBManager = historyDBManager;

			this.stateManager = stateManager;
			this.activeOutages = new ReliableDictionaryAccess<long, ActiveOutageMessage>(stateManager, ReliableDictionaryNames.ActiveOutages);

			string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
			Logger.LogDebug(debugMessage);
		}


		public Task<string> GetSubscriberName()
		{
			return Task.Run(() => { return MicroserviceNames.OmsHistoryDBManagerService; });
		}

		public Task<bool> IsAlive()
		{
			return Task.Run(() => { return true; });
		}

		public async Task Notify(IPublishableMessage message, string publisherName)
		{
			Logger.LogDebug($"{baseLogString} Notify method invoked. Publisher {publisherName}.");

			while (!isActiveOutagesInitialized)
			{
				await Task.Delay(1000);
			}

			if (message is ActiveOutageMessage activeOutageMessage)
			{
				await ActiveOutages.SetAsync(activeOutageMessage.OutageId, activeOutageMessage);

				foreach (var equipment in activeOutageMessage.OptimumIsolationPoints)
				{
					await this.historyDBManager.UpdateClosedSwitch(equipment.EquipmentId, activeOutageMessage.OutageId);
				}

				foreach (var equipment in activeOutageMessage.DefaultIsolationPoints)
				{
					await this.historyDBManager.UpdateClosedSwitch(equipment.EquipmentId, activeOutageMessage.OutageId);
				}

				foreach (var consumer in activeOutageMessage.AffectedConsumers)
				{
					await this.historyDBManager.UpdateConsumer(consumer.ConsumerId, activeOutageMessage.OutageId);
				}

			}
			else if (message is ArchivedOutageMessage archiveOutageMessage)
			{
				if (await ActiveOutages.ContainsKeyAsync(archiveOutageMessage.OutageId))
				{
					await ActiveOutages.TryRemoveAsync(archiveOutageMessage.OutageId);
				}
				else
				{
					Logger.LogWarning($"{baseLogString} WARNING Acrhived outage arrived, but there is no such in active outages. Outage Id {archiveOutageMessage.OutageId}.");
				}
			}
			else if (message is TopologyForUIMessage topology)
			{
				List<long> blackedOut = new List<long>();
				HashSet<long> energized = new HashSet<long>();

				Dictionary<long, ActiveOutageMessage> outages = await ActiveOutages.GetEnumerableDictionaryAsync();

				foreach (var element in topology.UIModel.Nodes.Values)
				{
					DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(element.Id);

					if (type == DMSType.ENERGYCONSUMER)
					{
						if (element.IsActive)
						{
							energized.Add(element.Id);
						}
						else
						{
							var outageId = await CheckConsumer(element.Id);
							await this.historyDBManager.OnConsumerBlackedOut(element.Id, outageId);

							//blackedOut.Add(element.Id);
						}
					}
					else if (type == DMSType.LOADBREAKSWITCH
				            || type == DMSType.BREAKER
				            || type == DMSType.FUSE
				            || type == DMSType.DISCONNECTOR)
					{
						if (IsOpened(element))
						{
							await this.historyDBManager.OnSwitchClosed(element.Id);
						}
						else
						{
							var outageId = await CheckSwitch(element.Id);
							await this.historyDBManager.OnSwitchOpened(element.Id, outageId);

						}
					}

					
				}

				//await this.historyDBManager.OnConsumerBlackedOut(blackedOut, null);
				await this.historyDBManager.OnConsumersEnergized(energized);
			}
		}

		private async Task<long> CheckConsumer(long gid)
		{
			Dictionary<long, ActiveOutageMessage> outages = await ActiveOutages.GetEnumerableDictionaryAsync();

			foreach (var outage in outages.Values)
			{
				if(outage.AffectedConsumers.Any(c => c.ConsumerId == gid))
				{
					return outage.OutageId;
				}
				
			}

			return -1;
		}

		private async Task<long> CheckSwitch(long gid)
		{
			Dictionary<long, ActiveOutageMessage> outages = await ActiveOutages.GetEnumerableDictionaryAsync();

			foreach (var outage in outages.Values)
			{
				if (outage.DefaultIsolationPoints.Any(e => e.EquipmentId == gid))
				{
					return outage.OutageId;
				}

				if (outage.OptimumIsolationPoints.Any(e => e.EquipmentId == gid))
				{
					return outage.OutageId;
				}
			}

			return -1;
		}

		private bool IsOpened(UINode node)
		{
			bool isOpened = false;

			foreach (var measurement in node.Measurements)
			{
				if (measurement.Type.Equals(DiscreteMeasurementType.SWITCH_STATUS.ToString())
					&& measurement.Value == 1)
				{
					isOpened = true;
					break;
				}
			}

			return isOpened;
		}
	}
}
