using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.OMS;
using Common.OmsContracts.HistoryDBManager;
using Common.OmsContracts.ModelAccess;
using Common.OmsContracts.Report;
using Common.PubSubContracts.DataContracts.OMS;
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
using OMS.HistoryDBManagerImplementation;
using OMS.HistoryDBManagerImplementation.DistributedTransaction;
using OMS.HistoryDBManagerImplementation.ModelAccess;
using OMS.HistoryDBManagerImplementation.Reporting;

namespace OMS.HistoryDBManagerService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class HistoryDBManagerService : StatefulService
    {
        private readonly string baseLogString;
     
        private readonly IHistoryDBManagerContract historyDBManagerProvider;
        private readonly IReportingContract reportServiceProvider;
        private readonly IOutageAccessContract outageModelAccessProvider;
        private readonly IConsumerAccessContract consumerAccessProvider;
        private readonly IEquipmentAccessContract equipmentAccessProvider;
        private readonly INotifyNetworkModelUpdateContract notifyNetworkModelUpdateProvider;
        private readonly ITransactionActorContract transactionActorProvider;
        private readonly HistorySubscriber historySubscriber;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public HistoryDBManagerService(StatefulServiceContext context)
            : base(context)
        {
            this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);

            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            try
            {
                this.historyDBManagerProvider = new HistoryDBManager(this.StateManager);
                this.reportServiceProvider = new ReportService();
                this.outageModelAccessProvider = new OutageModelAccess();
                this.consumerAccessProvider = new ConsumerAccess();
                this.equipmentAccessProvider = new EquipmentAccess();
                this.notifyNetworkModelUpdateProvider = new OmsHistoryNotifyNetworkModelUpdate(this.StateManager);
                this.transactionActorProvider = new OmsHistoryTransactionActor(this.StateManager);
                this.historySubscriber = new HistorySubscriber(this.StateManager, this.historyDBManagerProvider);


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
            return new List<ServiceReplicaListener>()
            {
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IHistoryDBManagerContract>(context,
                                                                                   this.historyDBManagerProvider,
                                                                                   WcfUtility.CreateTcpListenerBinding(),
                                                                                   EndpointNames.OmsHistoryDBManagerEndpoint);
                }, EndpointNames.OmsHistoryDBManagerEndpoint),

                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IReportingContract>(context,
                                                                            this.reportServiceProvider,
                                                                            WcfUtility.CreateTcpClientBinding(),
                                                                            EndpointNames.OmsReportingEndpoint);
                }, EndpointNames.OmsReportingEndpoint),

                new ServiceReplicaListener(context =>
                {
                        return new WcfCommunicationListener<IOutageAccessContract>(context,
                                                                                   this.outageModelAccessProvider,
                                                                                   WcfUtility.CreateTcpClientBinding(),
                                                                                   EndpointNames.OmsOutageAccessEndpoint);
                }, EndpointNames.OmsOutageAccessEndpoint),
                
                new ServiceReplicaListener(context =>
                {
                            return new WcfCommunicationListener<IConsumerAccessContract>(context,
                                                                                         this.consumerAccessProvider,
                                                                                         WcfUtility.CreateTcpClientBinding(),
                                                                                         EndpointNames.OmsConsumerAccessEndpoint);
                }, EndpointNames.OmsConsumerAccessEndpoint),
                
                new ServiceReplicaListener(context =>
                {
                        return new WcfCommunicationListener<IEquipmentAccessContract>(context,
                                                                                      this.equipmentAccessProvider,
                                                                                      WcfUtility.CreateTcpClientBinding(),
                                                                                      EndpointNames.OmsEquipmentAccessEndpoint);
                }, EndpointNames.OmsEquipmentAccessEndpoint),

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
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<INotifySubscriberContract>(context,
                                                                             this.historySubscriber,
                                                                             WcfUtility.CreateTcpListenerBinding(),
                                                                             EndpointNames.PubSubNotifySubscriberEndpoint);
                }, EndpointNames.PubSubNotifySubscriberEndpoint)
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                InitializeReliableCollections();
                string debugMessage = $"{baseLogString} RunAsync => ReliableDictionaries initialized.";
                Logger.LogDebug(debugMessage);

                var registerSubscriberClient = RegisterSubscriberClient.CreateClient();
                await registerSubscriberClient.SubscribeToTopic(Topic.ACTIVE_OUTAGE, MicroserviceNames.OmsHistoryDBManagerService);
                await registerSubscriberClient.SubscribeToTopic(Topic.TOPOLOGY, MicroserviceNames.OmsHistoryDBManagerService);
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
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, long>>(ReliableDictionaryNames.OpenedSwitches);
                        if(result.HasValue)
                        {
                            var gidToPointItemMap = result.Value;
                            await gidToPointItemMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, long>>(tx, ReliableDictionaryNames.OpenedSwitches);
                            await tx.CommitAsync();
                        }
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, long>>(ReliableDictionaryNames.UnenergizedConsumers);
                        if(result.HasValue)
                        {
                            var gidToPointItemMap = result.Value;
                            await gidToPointItemMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, long>>(tx, ReliableDictionaryNames.UnenergizedConsumers);
                            await tx.CommitAsync();
                        }
                    }
                }),


                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<byte, List<long>>>(ReliableDictionaryNames.HistoryModelChanges);
                        if(result.HasValue)
                        {
                            var modelChanges = result.Value;
                            await modelChanges.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<byte, List<long>>>(tx, ReliableDictionaryNames.HistoryModelChanges);
                            await tx.CommitAsync();
                        }
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, ActiveOutageMessage>>(ReliableDictionaryNames.ActiveOutages);
                        if(result.HasValue)
                        {
                            var modelChanges = result.Value;
                            await modelChanges.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, ActiveOutageMessage>>(tx, ReliableDictionaryNames.ActiveOutages);
                            await tx.CommitAsync();
                        }
                    }
                }),

            };

            Task.WaitAll(tasks);
        }
    }
}
