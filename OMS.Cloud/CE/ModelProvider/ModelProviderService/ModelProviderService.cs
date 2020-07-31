using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CECommon.Interfaces;
using Common.CE;
using Common.CeContracts.ModelProvider;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ModelProviderImplementation;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;

namespace ModelProviderService
{
	/// <summary>
	/// An instance of this class is created for each service replica by the Service Fabric runtime.
	/// </summary>
	internal sealed class ModelProviderService : StatefulService
	{
		private readonly string baseLogString;

		private readonly ModelProvider modelProvider;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		public ModelProviderService(StatefulServiceContext context)
			: base(context)
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

			try
			{
				this.modelProvider = new ModelProvider(this.StateManager);

				string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
				Logger.LogInformation(infoMessage);
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[ModelProviderService | Information] {infoMessage}");
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} Ctor => Exception caught: {e.Message}.";
				Logger.LogError(errorMessage, e);
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[ModelProviderService | Error] {errorMessage}");
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
					return new WcfCommunicationListener<IModelProviderContract>(context,
																			this.modelProvider,
																			WcfUtility.CreateTcpListenerBinding(),
																			EndpointNames.ModelProviderServiceEndpoint);
				}, EndpointNames.ModelProviderServiceEndpoint)
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
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[ModelProviderService | Information] {debugMessage}");
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} RunAsync => Exception caught: {e.Message}.";
				Logger.LogInformation(errorMessage, e);
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[ModelProviderService | Error] {errorMessage}");
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
						var result = await StateManager.TryGetAsync<IReliableDictionary<short, List<long>>>(ReliableDictionaryNames.EnergySourceCache);
						if(result.HasValue)
						{
							var topologyCache = result.Value;
							await topologyCache.ClearAsync();
							await tx.CommitAsync();
						}
						else
						{
							await StateManager.GetOrAddAsync<IReliableDictionary<short, List<long>>>(tx, ReliableDictionaryNames.EnergySourceCache);
							await tx.CommitAsync();
						}
					}
				}),
				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						var result = await StateManager.TryGetAsync<IReliableDictionary<short, Dictionary<long, ITopologyElement>>>(ReliableDictionaryNames.ElementCache);
						if(result.HasValue)
						{
							var topologyCacheUI = result.Value;
							await topologyCacheUI.ClearAsync();
							await tx.CommitAsync();
						}
						else
						{
							await StateManager.GetOrAddAsync<IReliableDictionary<short, Dictionary<long, ITopologyElement>>>(tx, ReliableDictionaryNames.ElementCache);
							await tx.CommitAsync();
						}
					}
				}),
				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						var result = await StateManager.TryGetAsync<IReliableDictionary<short, Dictionary<long, List<long>>>>(ReliableDictionaryNames.ElementConnectionCache);
						if(result.HasValue)
						{
							var topologyCacheOMS = result.Value;
							await topologyCacheOMS.ClearAsync();
							await tx.CommitAsync();
						}
						else
						{
							await StateManager.GetOrAddAsync<IReliableDictionary<short, Dictionary<long, List<long>>>>(tx, ReliableDictionaryNames.ElementConnectionCache);
							await tx.CommitAsync();
						}
					}
				}),
				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						var result = await StateManager.TryGetAsync<IReliableDictionary<short, HashSet<long>>>(ReliableDictionaryNames.RecloserCache);
						if(result.HasValue)
						{
							var topologyCacheOMS = result.Value;
							await topologyCacheOMS.ClearAsync();
							await tx.CommitAsync();
						}
						else
						{
							await StateManager.GetOrAddAsync<IReliableDictionary<short, HashSet<long>>>(tx, ReliableDictionaryNames.RecloserCache);
							await tx.CommitAsync();
						}
					}
				})
			};

			Task.WaitAll(tasks);
		}
	}
}
