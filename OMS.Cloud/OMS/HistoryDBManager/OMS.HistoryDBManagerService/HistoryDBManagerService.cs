using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.OMS;
using Common.OmsContracts.HistoryDBManager;
using Common.OmsContracts.ModelAccess;
using Common.OmsContracts.Report;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.HistoryDBManagerImplementation;
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
     
        private HistoryDBManager historyDBManager;
        private ReportService reportService;
        private OutageModelAccess outageModelAccess;
        private ConsumerAccess consumerAccess;
        private EquipmentAccess equipmentAccess;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public HistoryDBManagerService(StatefulServiceContext context)
            : base(context)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            try
            {
                historyDBManager = new HistoryDBManager(this.StateManager);
                reportService = new ReportService();
                outageModelAccess = new OutageModelAccess();
                consumerAccess = new ConsumerAccess();
                equipmentAccess = new EquipmentAccess();

                string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
                Logger.LogInformation(infoMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[HistoryDBManagerService | Information] {infoMessage}");
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Ctor => Exception caught: {e.Message}.";
                Logger.LogError(errorMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[HistoryDBManagerService | Error] {errorMessage}");
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
                                                                                    historyDBManager,
                                                                                    WcfUtility.CreateTcpListenerBinding(),
                                                                                    EndpointNames.OmsHistoryDBManagerEndpoint);
                }, EndpointNames.OmsHistoryDBManagerEndpoint),

                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IReportingContract>(context,
                                                                            reportService,
                                                                            WcfUtility.CreateTcpClientBinding(),
                                                                            EndpointNames.OmsReportingEndpoint);
                }, EndpointNames.OmsReportingEndpoint),

                new ServiceReplicaListener(context =>
                {
                        return new WcfCommunicationListener<IOutageAccessContract>(context,
                                                                                outageModelAccess,
                                                                                WcfUtility.CreateTcpClientBinding(),
                                                                                EndpointNames.OmsOutageAccessEndpoint);
                }, EndpointNames.OmsOutageAccessEndpoint),
                
                new ServiceReplicaListener(context =>
                {
                            return new WcfCommunicationListener<IConsumerAccessContract>(context,
                                                                                    consumerAccess,
                                                                                    WcfUtility.CreateTcpClientBinding(),
                                                                                    EndpointNames.OmsConsumerAccessEndpoint);
                }, EndpointNames.OmsConsumerAccessEndpoint),
                
                new ServiceReplicaListener(context =>
                {
                        return new WcfCommunicationListener<IEquipmentAccessContract>(context,
                                                                                equipmentAccess,
                                                                                WcfUtility.CreateTcpClientBinding(),
                                                                                EndpointNames.OmsEquipmentAccessEndpoint);
                }, EndpointNames.OmsEquipmentAccessEndpoint),
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                InitializeReliableCollections();
                string debugMessage = $"{baseLogString} RunAsync => ReliableDictionaries initialized.";
                Logger.LogDebug(debugMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[HistoryDBManagerService | Information] {debugMessage}");
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
            };

            Task.WaitAll(tasks);
        }
    }
}
