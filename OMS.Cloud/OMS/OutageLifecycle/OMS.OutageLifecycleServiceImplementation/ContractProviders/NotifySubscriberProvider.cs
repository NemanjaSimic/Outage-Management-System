using Common.CeContracts;
using Common.OMS;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.PubSubContracts.Interfaces;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.OMS.OutageLifecycle;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleImplementation.ContractProviders
{
    public class NotifySubscriberProvider : INotifySubscriberContract
	{
		private const string subscriberName = MicroserviceNames.OmsOutageLifecycleService;

		private readonly string baseLogString;
		private readonly IReliableStateManager stateManager;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		#region Reliable Dictionaries
		private bool isMonitoredHeadBreakerMeasurementsInitialized;
		private bool isOutageTopologyModelInitialized;
		private bool isCommandedElementsInitialized;
		private bool isPotentialOutagesQueueInitialized;

		private bool ReliableDictionariesInitialized
		{
			get
			{
				return isMonitoredHeadBreakerMeasurementsInitialized &&
					   isOutageTopologyModelInitialized &&
					   isCommandedElementsInitialized &&
					   isPotentialOutagesQueueInitialized;
			}
		}

		private ReliableDictionaryAccess<long, DiscreteModbusData> monitoredHeadBreakerMeasurements;
		private ReliableDictionaryAccess<long, DiscreteModbusData> MonitoredHeadBreakerMeasurements
		{
			get { return monitoredHeadBreakerMeasurements; }
		}

		private ReliableDictionaryAccess<string, OutageTopologyModel> outageTopologyModel;
		private ReliableDictionaryAccess<string, OutageTopologyModel> OutageTopologyModel
		{
			get { return outageTopologyModel; }
		}

		private ReliableDictionaryAccess<long, CommandedElement> commandedElements;
		private ReliableDictionaryAccess<long, CommandedElement> CommandedElements
		{
			get { return commandedElements; }
		}

		private ReliableQueueAccess<PotentialOutageCommand> potentialOutagesQueue;
		private ReliableQueueAccess<PotentialOutageCommand> PotentialOutagesQueue
		{
			get { return potentialOutagesQueue; }
		}

		private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs eventArgs)
		{
			try
			{
				await InitializeReliableCollections(eventArgs);
			}
			catch (FabricNotPrimaryException)
			{
				Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => NotPrimaryException. To be ignored.");
			}
			catch (FabricObjectClosedException)
			{
				Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => FabricObjectClosedException. To be ignored.");
			}
			catch (COMException)
			{
				Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => {typeof(COMException)}. To be ignored.");
			}
		}

		private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
		{
			if (e.Action == NotifyStateManagerChangedAction.Add)
			{
				var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
				string reliableStateName = operation.ReliableState.Name.AbsolutePath;

				if (reliableStateName == ReliableDictionaryNames.MonitoredHeadBreakerMeasurements)
				{
					this.monitoredHeadBreakerMeasurements = await ReliableDictionaryAccess<long, DiscreteModbusData>.Create(stateManager, ReliableDictionaryNames.MonitoredHeadBreakerMeasurements);
					this.isMonitoredHeadBreakerMeasurementsInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.MonitoredHeadBreakerMeasurements}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.OutageTopologyModel)
				{
					this.outageTopologyModel = await ReliableDictionaryAccess<string, OutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.OutageTopologyModel);
					this.isOutageTopologyModelInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.OutageTopologyModel}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.CommandedElements)
				{
					this.commandedElements = await ReliableDictionaryAccess<long, CommandedElement>.Create(stateManager, ReliableDictionaryNames.CommandedElements);
					this.isCommandedElementsInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.CommandedElements}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableQueueNames.PotentialOutages)
				{
					this.potentialOutagesQueue = await ReliableQueueAccess<PotentialOutageCommand>.Create(stateManager, ReliableQueueNames.PotentialOutages);
					this.isPotentialOutagesQueueInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableQueueNames.PotentialOutages}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
			}
		}
        #endregion Reliable Dictionaries

		public NotifySubscriberProvider(IReliableStateManager stateManager)
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

			this.isMonitoredHeadBreakerMeasurementsInitialized = false;
			this.isOutageTopologyModelInitialized = false;
			this.isCommandedElementsInitialized = false;
			this.isPotentialOutagesQueueInitialized = false;

			this.stateManager = stateManager;
			this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
		}

		#region INotifySubscriberContract
		public Task<string> GetSubscriberName()
		{
			return Task.Run(() => subscriberName);
		}

		public async Task Notify(IPublishableMessage message, string publisherName)
		{
			Logger.LogDebug($"{baseLogString} Notify method started.");

			while (!ReliableDictionariesInitialized)
            {
				await Task.Delay(1000);
            }

            try
            {
				if (message is MultipleDiscreteValueSCADAMessage multipleDiscreteValueSCADAMessage)
                {
					Logger.LogDebug($"{baseLogString} MultipleDiscreteValueSCADAMessage received.");
					var discreteData = multipleDiscreteValueSCADAMessage.Data;

                    #region HeadBreakers
                    var enumerableHeadBreakerMeasurements = await MonitoredHeadBreakerMeasurements.GetEnumerableDictionaryAsync();
					foreach (var headMeasurementGid in enumerableHeadBreakerMeasurements.Keys)
					{
						if (!discreteData.ContainsKey(headMeasurementGid))
						{
							continue;
						}

						await MonitoredHeadBreakerMeasurements.SetAsync(headMeasurementGid, discreteData[headMeasurementGid]);
					}
					#endregion HeadBreakers

					#region CommandedElements
					var measurementProviderClient = MeasurementProviderClient.CreateClient();
					var enumerableCommandedElements = await CommandedElements.GetEnumerableDictionaryAsync();
					foreach(var commandedElementGid in enumerableCommandedElements.Keys)
                    {
						var measurementGid = (await measurementProviderClient.GetMeasurementsOfElement(commandedElementGid)).FirstOrDefault();
						var measurement = await measurementProviderClient.GetDiscreteMeasurement(measurementGid);

						if(measurement is ArtificalDiscreteMeasurement)
                        {
							await CommandedElements.TryRemoveAsync(commandedElementGid);
							Logger.LogInformation($"{baseLogString} Notify => Command on element 0x{commandedElementGid:X16} executed (ArtificalDiscreteMeasurement). New value: {measurement.CurrentOpen}");
							continue;
						}

						if(!discreteData.ContainsKey(measurementGid))
                        {
							continue;
						}

						if(discreteData[measurementGid].Value == (ushort)enumerableCommandedElements[commandedElementGid].CommandingType)
                        {
							if((await CommandedElements.TryRemoveAsync(commandedElementGid)).HasValue)
                            {
								Logger.LogInformation($"{baseLogString} Notify => Command on element 0x{commandedElementGid:X16} executed. New value: {discreteData[measurementGid].Value}");
                            }
                        }
                    }
					#endregion CommandedElements
				}
				else if(message is OMSModelMessage omsModelMessage)
                {
					Logger.LogDebug($"{baseLogString} OMSModelMessage received. Count {omsModelMessage.OutageTopologyModel.OutageTopology.Count}");
					
					OutageTopologyModel topology = omsModelMessage.OutageTopologyModel;
					await OutageTopologyModel.SetAsync(ReliableDictionaryNames.OutageTopologyModel, topology);

					var reportingOutageClient = PotentialOutageReportingClient.CreateClient();

					while(true)
                    {
						var result = await PotentialOutagesQueue.TryDequeueAsync();

						if(!result.HasValue)
                        {
							break;
                        }

						var command = result.Value;

						await reportingOutageClient.ReportPotentialOutage(command.ElementGid, command.CommandOriginType, command.NetworkType);
						Logger.LogInformation($"{baseLogString} PotentianOutageCommand executed. ElementGid: 0x{command.ElementGid:X16}, OriginType: {command.CommandOriginType}");
					}
                }
				else
                {
					Logger.LogWarning($"{baseLogString} Notify => unexpected type of message: {message.GetType()}");
					return;
				}
			}
            catch (Exception e)
            {
				string errorMessage = $"{baseLogString} Notify => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);
			}
		}

		public Task<bool> IsAlive()
		{
			return Task.Run(() => true);
		}
		#endregion INotifySubscriberContract
	}
}
