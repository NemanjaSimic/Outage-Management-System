using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using CE.ModelProviderImplementation;
using Common.CE;
using Common.CeContracts;
using Common.CeContracts.ModelProvider;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.TmsContracts;
using OMS.Common.TmsContracts.Notifications;

namespace CE.ModelProviderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ModelProviderService : StatefulService
	{
		private readonly string baseLogString;
		private readonly CeTransactionActor ceTransactionActor;
		private readonly CeNetworkNotifyModelUpdate ceNetworkNotifyModelUpdate;
		private readonly ModelProvider modelProvider;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		public ModelProviderService(StatefulServiceContext context)
			: base(context)
		{
			this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);

			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

			try
			{
				var modelManager = new ModelManager(this.StateManager);
				this.modelProvider = new ModelProvider(this.StateManager, modelManager);
				this.ceTransactionActor = new CeTransactionActor();
				this.ceNetworkNotifyModelUpdate = new CeNetworkNotifyModelUpdate();

				string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
				Logger.LogInformation(infoMessage);
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} Ctor => Exception caught: {e.Message}.";
				Logger.LogError(errorMessage, e);
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
					return new WcfCommunicationListener<ICeModelProviderContract>(context,
																			this.modelProvider,
																			WcfUtility.CreateTcpListenerBinding(),
																			EndpointNames.CeModelProviderServiceEndpoint);
				}, EndpointNames.CeModelProviderServiceEndpoint),
				
				new ServiceReplicaListener(context =>
				{
					return new WcfCommunicationListener<ITransactionActorContract>(context,
																			this.ceTransactionActor,
																			WcfUtility.CreateTcpListenerBinding(),
																			EndpointNames.TmsTransactionActorEndpoint);
				}, EndpointNames.TmsTransactionActorEndpoint),

				new ServiceReplicaListener(context =>
				{
					return new WcfCommunicationListener<INotifyNetworkModelUpdateContract>(context,
																			this.ceNetworkNotifyModelUpdate,
																			WcfUtility.CreateTcpListenerBinding(),
																			EndpointNames.TmsNotifyNetworkModelUpdateEndpoint);
				}, EndpointNames.TmsNotifyNetworkModelUpdateEndpoint),
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
				//InitializeReliableCollections();
				string debugMessage = $"{baseLogString} RunAsync => ReliableDictionaries initialized.";
				Logger.LogDebug(debugMessage);
					
				await modelProvider.ImportDataInCache();
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} RunAsync => Exception caught: {e.Message}.";
				Logger.LogInformation(errorMessage, e);
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
						await StateManager.GetOrAddAsync<IReliableDictionary<short, List<long>>>(tx, ReliableDictionaryNames.EnergySourceCache);
						await tx.CommitAsync();
					}
				}),

				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						await StateManager.GetOrAddAsync<IReliableDictionary<short, Dictionary<long, TopologyElement>>>(tx, ReliableDictionaryNames.ElementCache);
						await tx.CommitAsync();
					}
				}),

				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						await StateManager.GetOrAddAsync<IReliableDictionary<short, Dictionary<long, List<long>>>>(tx, ReliableDictionaryNames.ElementConnectionCache);
						await tx.CommitAsync();
					}
				}),

				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						await StateManager.GetOrAddAsync<IReliableDictionary<short, HashSet<long>>>(tx, ReliableDictionaryNames.RecloserCache);
						await tx.CommitAsync();
					}
				}),
				
				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						var energySources = await StateManager.GetOrAddAsync<IReliableDictionary<string, List<long>>>(tx, ReliableDictionaryNames.EnergySources);
						await tx.CommitAsync();
					}
				}),

				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						var reclosers = await StateManager.GetOrAddAsync<IReliableDictionary<string, HashSet<long>>>(tx, ReliableDictionaryNames.Reclosers);
						await tx.CommitAsync();
					}
				}),

				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						await StateManager.GetOrAddAsync<IReliableDictionary<long, IMeasurement>>(tx, ReliableDictionaryNames.Measurements);
						await tx.CommitAsync();
					}
				}),

				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						await StateManager.GetOrAddAsync<IReliableDictionary<long, TopologyElement>>(tx, ReliableDictionaryNames.TopologyElements);
						await tx.CommitAsync();
					}
				}),

				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						await StateManager.GetOrAddAsync<IReliableDictionary<long, float>>(tx, ReliableDictionaryNames.BaseVoltages);
						await tx.CommitAsync();
					}
				}),

				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						await StateManager.GetOrAddAsync<IReliableDictionary<long, List<long>>>(tx, ReliableDictionaryNames.ElementConnections);
						await tx.CommitAsync();
					}
				}),

				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						await StateManager.GetOrAddAsync<IReliableDictionary<long, long>>(tx, ReliableDictionaryNames.MeasurementToConnectedTerminalMap);
						await tx.CommitAsync();
					}
				}),

				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						await StateManager.GetOrAddAsync<IReliableDictionary<long, List<long>>>(tx, ReliableDictionaryNames.TerminalToConnectedElementsMap);
						await tx.CommitAsync();
					}
				}),
			};

			Task.WaitAll(tasks);
		}
	}
}
