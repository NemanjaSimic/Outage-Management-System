using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.OMS;
using Common.OmsContracts.ModelProvider;
using Common.PubSubContracts.DataContracts.CE;
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
using OMS.Common.TmsContracts;
using OMS.Common.TmsContracts.Notifications;
using OMS.Common.WcfClient.PubSub;
using OMS.ModelProviderImplementation;
using OMS.ModelProviderImplementation.ContractProviders;
using OMS.ModelProviderImplementation.DistributedTransaction;

namespace OMS.ModelProviderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ModelProviderService : StatefulService
    {
        private readonly string baseLogString;
        private readonly OutageModel outageModel;

        private readonly IOutageModelReadAccessContract outageModelReadAccessProvider;
        private readonly IOutageModelUpdateAccessContract outageModelUpdateAccessProvider;
        private readonly INotifyNetworkModelUpdateContract notifyNetworkModelUpdateProvider;
        private readonly ITransactionActorContract transactionActorProvider;

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
                this.outageModel = new OutageModel(this.StateManager);

                this.outageModelReadAccessProvider = new OutageModelReadAccessProvider(this.StateManager);
                this.outageModelUpdateAccessProvider = new OutageModelUpdateAccessProvider(this.StateManager);
                this.notifyNetworkModelUpdateProvider = new OmsModelProviderNotifyNetworkModelUpdate(this.StateManager);
                this.transactionActorProvider = new OmsModelProviderTransactionActor(this.StateManager);

                string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
                Logger.LogInformation(infoMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[OMS.ModelProviderService | Information] {infoMessage}");
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Ctor => Exception caught: {e.Message}.";
                Logger.LogError(errorMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[OMS.ModelProviderService | Error] {errorMessage}");
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
                    return new WcfCommunicationListener<IOutageModelReadAccessContract>(context,
                                                                                        this.outageModelReadAccessProvider,
                                                                                        WcfUtility.CreateTcpListenerBinding(),
                                                                                        EndpointNames.OmsModelReadAccessEndpoint);
                }, EndpointNames.OmsModelReadAccessEndpoint),

                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IOutageModelUpdateAccessContract>(context,
                                                                                          this.outageModelUpdateAccessProvider,
                                                                                          WcfUtility.CreateTcpListenerBinding(),
                                                                                          EndpointNames.OmsModelUpdateAccessEndpoint);
                }, EndpointNames.OmsModelUpdateAccessEndpoint),

                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<INotifySubscriberContract>(context,
                                                                                   this.outageModel,
                                                                                   WcfUtility.CreateTcpListenerBinding(),
                                                                                   EndpointNames.PubSubNotifySubscriberEndpoint);
                }, EndpointNames.PubSubNotifySubscriberEndpoint),

                new ServiceReplicaListener(context =>
                {
                        return new WcfCommunicationListener<INotifyNetworkModelUpdateContract>(context,
                                                                                               this.notifyNetworkModelUpdateProvider,
                                                                                               WcfUtility.CreateTcpClientBinding(),
                                                                                               EndpointNames.TmsNotifyNetworkModelUpdateEndpoint);
                }, EndpointNames.TmsNotifyNetworkModelUpdateEndpoint),

                new ServiceReplicaListener(context =>
                {
                        return new WcfCommunicationListener<ITransactionActorContract>(context,
                                                                                       this.transactionActorProvider,
                                                                                       WcfUtility.CreateTcpClientBinding(),
                                                                                       EndpointNames.TmsTransactionActorEndpoint);
                }, EndpointNames.TmsTransactionActorEndpoint),
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {   
            try
			{
                InitializeReliableCollections();
                string debugMessage = $"{baseLogString} RunAsync => ReliableDictionaries initialized.";
                Logger.LogDebug(debugMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[OMS.ModelProviderService | Information] {debugMessage}");

                var registerSubscriberClient = RegisterSubscriberClient.CreateClient();
                await registerSubscriberClient.SubscribeToTopic(Topic.OMS_MODEL, MicroserviceNames.OmsModelProviderService);
                debugMessage = $"{baseLogString} RunAsync => Successfully subscribed to topics.";
                Logger.LogDebug(debugMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[OMS.ModelProviderService | Information] {debugMessage}");

                await outageModel.InitializeOutageModel();
                debugMessage = $"{baseLogString} RunAsync => outageModel.InitializeOutageModel() done.";
                Logger.LogDebug(debugMessage);
            }
            catch (Exception e)
			{
                Logger.LogError($"{baseLogString} RunAsync => Exception: {e.Message}");
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
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, OutageTopologyModel>>(ReliableDictionaryNames.OutageTopologyModel);
                        if(result.HasValue)
                        {
                            var gidToPointItemMap = result.Value;
                            await gidToPointItemMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, OutageTopologyModel>>(tx, ReliableDictionaryNames.OutageTopologyModel);
                            await tx.CommitAsync();
                        }
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, long>>(ReliableDictionaryNames.CommandedElements);
                        if(result.HasValue)
                        {
                            var gidToPointItemMap = result.Value;
                            await gidToPointItemMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, long>>(tx, ReliableDictionaryNames.CommandedElements);
                            await tx.CommitAsync();
                        }
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, long>>(ReliableDictionaryNames.OptimumIsolatioPoints);
                        if(result.HasValue)
                        {
                            var addressToGidMap = result.Value;
                            await addressToGidMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, long>>(tx, ReliableDictionaryNames.OptimumIsolatioPoints);
                            await tx.CommitAsync();
                        }
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, CommandOriginType>>(ReliableDictionaryNames.PotentialOutage);
                        if(result.HasValue)
                        {
                            var addressToGidMap = result.Value;
                            await addressToGidMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, CommandOriginType>>(tx, ReliableDictionaryNames.PotentialOutage);
                            await tx.CommitAsync();
                        }
                    }
                }),
            };

            Task.WaitAll(tasks);
        }
    }
}
