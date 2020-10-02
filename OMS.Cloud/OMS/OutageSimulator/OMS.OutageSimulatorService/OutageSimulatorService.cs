using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.OMS;
using Common.OMS.OutageSimulator;
using Common.OmsContracts.DataContracts.OutageSimulator;
using Common.OmsContracts.OutageSimulator;
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
using OMS.OutageSimulatorImplementation;
using OMS.OutageSimulatorImplementation.ContractProviders;
using OMS.OutageSimulatorImplementation.DataContracts;

namespace OMS.OutageSimulatorService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class OutageSimulatorService : StatefulService
    {
        private const int commandedValueIntervalUpperLimit = 60_000;

        private readonly string baseLogString;

        private readonly IOutageSimulatorContract outageSimulatorProvider;
        private readonly IOutageSimulatorUIContract outageSimulatorUIProvider;
        private readonly INotifySubscriberContract notifySubscriberProvider;

        private readonly SimulationControlCycle controlCycle;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public OutageSimulatorService(StatefulServiceContext context)
            : base(context)
        {
            this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);

            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            try
            {
                this.controlCycle = new SimulationControlCycle(StateManager, commandedValueIntervalUpperLimit);

                this.outageSimulatorProvider = new OutageSimulatorProvider(StateManager);
                this.outageSimulatorUIProvider = new OutageSimulatorUIProvider(StateManager);
                this.notifySubscriberProvider = new NotifySubscriberProvider(StateManager, MicroserviceNames.OmsOutageSimulatorService);
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
            //return new ServiceReplicaListener[0];
            return new ServiceReplicaListener[]
            {
                //OmsOutageSimulatorEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IOutageSimulatorContract>(context,
                                                                            this.outageSimulatorProvider,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.OmsOutageSimulatorEndpoint);
                }, EndpointNames.OmsOutageSimulatorEndpoint),

                //OmsOutageSimulatorUIEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IOutageSimulatorUIContract>(context,
                                                                            this.outageSimulatorUIProvider,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.OmsOutageSimulatorUIEndpoint);
                }, EndpointNames.OmsOutageSimulatorUIEndpoint),

                //PubSubNotifySubscriberEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<INotifySubscriberContract>(context,
                                                                            this.notifySubscriberProvider,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.PubSubNotifySubscriberEndpoint);
                }, EndpointNames.PubSubNotifySubscriberEndpoint),
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
                
                var registerSubscriber = RegisterSubscriberClient.CreateClient();
                await registerSubscriber.SubscribeToTopic(Topic.SWITCH_STATUS, MicroserviceNames.OmsOutageSimulatorService);

                debugMessage = $"{baseLogString} RunAsync => Subscribed to {Topic.SWITCH_STATUS} topic.";
                Logger.LogDebug(debugMessage);
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} RunAsync => Exception caught: {e.Message}.";
                Logger.LogInformation(errorMessage, e);
            }
            
            while (true)
            {
                try
                {
                    await controlCycle.Start();

                    var message = $"{baseLogString} RunAsync => ControlCycle executed.";
                    Logger.LogVerbose(message);
                }
                catch (Exception e)
                {
                    string errorMessage = $"{baseLogString} RunAsync (while) => Exception caught: {e.Message}.";
                    Logger.LogInformation(errorMessage, e);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(2_000), cancellationToken);
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
                        await StateManager.GetOrAddAsync<IReliableDictionary<long, SimulatedOutage>>(tx, ReliableDictionaryNames.SimulatedOutages);
                        await tx.CommitAsync();
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        await StateManager.GetOrAddAsync<IReliableDictionary<long, MonitoredIsolationPoint>>(tx, ReliableDictionaryNames.MonitoredIsolationPoints);
                        await tx.CommitAsync();
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        await StateManager.GetOrAddAsync<IReliableDictionary<long, CommandedValue>>(tx, ReliableDictionaryNames.CommandedValues);
                        await tx.CommitAsync();
                    }
                }),
            };

            Task.WaitAll(tasks);
        }
    }
}
