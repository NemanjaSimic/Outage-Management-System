using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.CE;
using Common.CE.Interfaces;
using Common.CeContracts;
using Common.CeContracts.TopologyProvider;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSub;
using ReliableDictionaryNames = Common.CE.ReliableDictionaryNames;
using CE.TopologyProviderImplementation;

namespace CE.TopologyProviderService
{
	/// <summary>
	/// An instance of this class is created for each service replica by the Service Fabric runtime.
	/// </summary>
	internal sealed class TopologyProviderService : StatefulService
	{
		private readonly string baseLogString;

		private readonly TopologyProvider topologyProvider;
		private readonly TopologyConverter topologyConverter;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public TopologyProviderService(StatefulServiceContext context)
			: base(context)
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

			try
			{
				this.topologyProvider = new TopologyProvider(this.StateManager);
				this.topologyConverter = new TopologyConverter();

				string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
				Logger.LogInformation(infoMessage);
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[TopologyProviderService | Information] {infoMessage}");
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} Ctor => Exception caught: {e.Message}.";
				Logger.LogError(errorMessage, e);
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[TopologyProviderService | Error] {errorMessage}");
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
					return new WcfCommunicationListener<ITopologyProviderContract>(context,
																			this.topologyProvider,
																			WcfUtility.CreateTcpListenerBinding(),
																			EndpointNames.CeTopologyProviderServiceEndpoint);
				}, EndpointNames.CeTopologyProviderServiceEndpoint),

				new ServiceReplicaListener(context =>
				{
					return new WcfCommunicationListener<ITopologyConverterContract>(context,
																			this.topologyConverter,
																			WcfUtility.CreateTcpListenerBinding(),
																			EndpointNames.CeTopologyConverterServiceEndpoint);
				}, EndpointNames.CeTopologyConverterServiceEndpoint),
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
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[TopologyProviderService | Information] {debugMessage}");
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} RunAsync => Exception caught: {e.Message}.";
                Logger.LogInformation(errorMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[TopologyProviderService | Error] {errorMessage}");
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
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, ITopology>>(ReliableDictionaryNames.TopologyCache);
                        if(result.HasValue)
                        {
                            var topologyCache = result.Value;
                            await topologyCache.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, ITopology>>(tx, ReliableDictionaryNames.TopologyCache);
                            await tx.CommitAsync();
                        }
                    }
                }),
                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, UIModel>>(ReliableDictionaryNames.TopologyCacheUI);
                        if(result.HasValue)
                        {
                            var topologyCacheUI = result.Value;
                            await topologyCacheUI.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, UIModel>>(tx, ReliableDictionaryNames.TopologyCacheUI);
                            await tx.CommitAsync();
                        }
                    }
                }),
                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, IOutageTopologyModel>>(ReliableDictionaryNames.TopologyCacheOMS);
                        if(result.HasValue)
                        {
                            var topologyCacheOMS = result.Value;
                            await topologyCacheOMS.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, IOutageTopologyModel>>(tx, ReliableDictionaryNames.TopologyCacheOMS);
                            await tx.CommitAsync();
                        }
                    }
                }) 
            };

            Task.WaitAll(tasks);
        }
    }
}
