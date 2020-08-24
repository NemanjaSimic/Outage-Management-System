using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using CE.MeasurementProviderImplementation;
using Common.CE;
using Common.CeContracts;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts;
using OMS.Common.WcfClient.PubSub;

namespace CE.MeasurementProviderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class MeasurementProviderService : StatefulService
	{
		private readonly string baseLogString;

		private readonly MeasurementProvider measurementProvider;
		private readonly MeasurementMap measurementMap;
		private readonly SwitchStatusCommanding switchStatusCommanding;
		private readonly ScadaSubscriber scadaSubscriber;

		private IRegisterSubscriberContract registerSubscriberClient;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		public MeasurementProviderService(StatefulServiceContext context)
			: base(context)
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

			try
			{
				this.measurementProvider = new MeasurementProvider(this.StateManager);
				this.measurementMap = new MeasurementMap();
				this.switchStatusCommanding = new SwitchStatusCommanding();
				this.scadaSubscriber = new ScadaSubscriber();

				string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
				Logger.LogInformation(infoMessage);
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[MeasurementProviderService | Information] {infoMessage}");
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} Ctor => Exception caught: {e.Message}.";
				Logger.LogError(errorMessage, e);
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[MeasurementProviderService | Error] {errorMessage}");
			}
		}

		/// <summary>
		/// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
		/// </summary>
		/// <remarks>
		/// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
		/// </remarks>
		/// <returns>A collection of listeners.</returns>
		protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
		{
			return new[]
			{
				new ServiceReplicaListener(context =>
				{
					return new WcfCommunicationListener<IMeasurementProviderContract>(context,
																			this.measurementProvider,
																			WcfUtility.CreateTcpListenerBinding(),
																			EndpointNames.CeMeasurementProviderEndpoint);
				}, EndpointNames.CeMeasurementProviderEndpoint),

				new ServiceReplicaListener(context =>
				{
					return new WcfCommunicationListener<IMeasurementMapContract>(context,
																			this.measurementMap,
																			WcfUtility.CreateTcpListenerBinding(),
																			EndpointNames.CeMeasurementMapEndpoint);
				}, EndpointNames.CeMeasurementMapEndpoint),

				new ServiceReplicaListener(context =>
				{
					return new WcfCommunicationListener<ISwitchStatusCommandingContract>(context,
																			this.switchStatusCommanding,
																			WcfUtility.CreateTcpListenerBinding(),
																			EndpointNames.CeSwitchStatusCommandingEndpoint);
				}, EndpointNames.CeSwitchStatusCommandingEndpoint),
				new ServiceReplicaListener(context =>
				{
					return new WcfCommunicationListener<INotifySubscriberContract>(context,
																			 this.scadaSubscriber,
																			 WcfUtility.CreateTcpListenerBinding(),
																			 EndpointNames.PubSubNotifySubscriberEndpoint);
				}, EndpointNames.PubSubNotifySubscriberEndpoint)
			};
		}

		/// <summary>
		/// This is the main entry point for your service replica.
		/// This method executes when this replica of your service becomes primary and has write status.
		/// </summary>
		/// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
		protected override async Task RunAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				InitializeReliableCollections();
				string debugMessage = $"{baseLogString} RunAsync => ReliableDictionaries initialized.";
				Logger.LogDebug(debugMessage);
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[MeasurementProviderService | Information] {debugMessage}");

				this.registerSubscriberClient = RegisterSubscriberClient.CreateClient();
				await this.registerSubscriberClient.SubscribeToTopic(Topic.MEASUREMENT, MicroserviceNames.CeMeasurementProviderService);
				await this.registerSubscriberClient.SubscribeToTopic(Topic.SWITCH_STATUS, MicroserviceNames.CeMeasurementProviderService);
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} RunAsync => Exception caught: {e.Message}.";
				Logger.LogInformation(errorMessage, e);
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[MeasurementProviderService | Error] {errorMessage}");
			}
		}

		private void InitializeReliableCollections()
		{
			Task[] tasks = new Task[]
			{
				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						var result = await StateManager.TryGetAsync<IReliableDictionary<short, Dictionary<long, AnalogMeasurement>>>(ReliableDictionaryNames.AnalogMeasurementsCache);
						if(result.HasValue)
						{
							var topologyCache = result.Value;
							await topologyCache.ClearAsync();
							await tx.CommitAsync();
						}
						else
						{
							await StateManager.GetOrAddAsync<IReliableDictionary<short, Dictionary<long, AnalogMeasurement>>>(tx, ReliableDictionaryNames.AnalogMeasurementsCache);
							await tx.CommitAsync();
						}
					}
				}),
				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						var result = await StateManager.TryGetAsync<IReliableDictionary<short, Dictionary<long, DiscreteMeasurement>>>(ReliableDictionaryNames.DiscreteMeasurementsCache);
						if(result.HasValue)
						{
							var topologyCacheUI = result.Value;
							await topologyCacheUI.ClearAsync();
							await tx.CommitAsync();
						}
						else
						{
							await StateManager.GetOrAddAsync<IReliableDictionary<short, Dictionary<long, DiscreteMeasurement>>>(tx, ReliableDictionaryNames.DiscreteMeasurementsCache);
							await tx.CommitAsync();
						}
					}
				}),
				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						var result = await StateManager.TryGetAsync<IReliableDictionary<short, Dictionary<long, List<long>>>>(ReliableDictionaryNames.ElementsToMeasurementMapCache);
						if(result.HasValue)
						{
							var topologyCacheOMS = result.Value;
							await topologyCacheOMS.ClearAsync();
							await tx.CommitAsync();
						}
						else
						{
							await StateManager.GetOrAddAsync<IReliableDictionary<short, Dictionary<long, List<long>>>>(tx, ReliableDictionaryNames.ElementsToMeasurementMapCache);
							await tx.CommitAsync();
						}
					}
				}),
				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						var result = await StateManager.TryGetAsync<IReliableDictionary<short, Dictionary<long, long>>>(ReliableDictionaryNames.MeasurementsToElementMapCache);
						if(result.HasValue)
						{
							var topologyCacheOMS = result.Value;
							await topologyCacheOMS.ClearAsync();
							await tx.CommitAsync();
						}
						else
						{
							await StateManager.GetOrAddAsync<IReliableDictionary<short, Dictionary<long, long>>>(tx, ReliableDictionaryNames.MeasurementsToElementMapCache);
							await tx.CommitAsync();
						}
					}
				})
			};

			Task.WaitAll(tasks);
		}
	}
}
