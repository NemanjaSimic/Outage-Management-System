﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.OMS;
using Common.OmsContracts.OutageLifecycle;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.NmsContracts;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.WcfClient.PubSub;
using OMS.OutageLifecycleImplementation.Algorithm;
using OMS.OutageLifecycleImplementation.ContractProviders;
using OMS.OutageLifecycleImplementation.Helpers;

namespace OMS.OutageLifecycleService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class OutageLifecycleService : StatefulService
    {
        private readonly string baseLogString;

		private readonly IPotentialOutageReportingContract potentialOutageReportingProvider;
		private readonly IOutageIsolationContract outageIsolationProvider;
		private readonly ICrewSendingContract crewSendingProvider;
		private readonly IOutageResolutionContract outageResolutionProvider;
		private readonly INotifySubscriberContract notifySubscriberProvider;

        private readonly int isolationAlgorithmCycleInterval;
        private readonly IsolationAlgorithmCycle isolationAlgorithmCycle;

        private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public OutageLifecycleService(StatefulServiceContext context)
            : base(context)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            try
            {
                var modelResourcesDesc = new ModelResourcesDesc();
                var lifecycleHelper = new OutageLifecycleHelper(modelResourcesDesc);

                this.potentialOutageReportingProvider = new PotentialOutageReportingProvider(StateManager, lifecycleHelper);
                this.outageIsolationProvider = new OutageIsolationProvider(StateManager, lifecycleHelper, modelResourcesDesc);
                this.crewSendingProvider = new CrewSendingProvider(lifecycleHelper);
                this.outageResolutionProvider = new OutageResolutionProvider(lifecycleHelper);
                this.notifySubscriberProvider = new NotifySubscriberProvider(StateManager);

                this.isolationAlgorithmCycleInterval = 1000;
                this.isolationAlgorithmCycle = new IsolationAlgorithmCycle(StateManager, this.isolationAlgorithmCycleInterval);

                string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
                Logger.LogInformation(infoMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[OMS.OutageLifecycleService | Information] {infoMessage}");
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Ctor => Exception caught: {e.Message}.";
                Logger.LogError(errorMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[OMS.OutageLifecycleService | Error] {errorMessage}");
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
			return new List<ServiceReplicaListener>
			{
				new ServiceReplicaListener (context =>
				{
					return new WcfCommunicationListener<IPotentialOutageReportingContract>(context,
																				           this.potentialOutageReportingProvider,
																				           WcfUtility.CreateTcpListenerBinding(),
																				           EndpointNames.OmsPotentialOutageReportingEndpoint);
				}, EndpointNames.OmsPotentialOutageReportingEndpoint),

				new ServiceReplicaListener (context =>
				{
					return new WcfCommunicationListener<IOutageIsolationContract>(context,
																				  this.outageIsolationProvider,
																				  WcfUtility.CreateTcpListenerBinding(),
																			      EndpointNames.OmsOutageIsolationEndpoint);
				}, EndpointNames.OmsOutageIsolationEndpoint),

				new ServiceReplicaListener (context =>
				{
					return new WcfCommunicationListener<ICrewSendingContract>(context,
																			  this.crewSendingProvider,
																			  WcfUtility.CreateTcpListenerBinding(),
																			  EndpointNames.OmsCrewSendingEndpoint);
				}, EndpointNames.OmsCrewSendingEndpoint),

				new ServiceReplicaListener (context =>
				{
					return new WcfCommunicationListener<IOutageResolutionContract>(context,
																			  this.outageResolutionProvider,
																			  WcfUtility.CreateTcpListenerBinding(),
																			  EndpointNames.OmsOutageResolutionEndpoint);
				}, EndpointNames.OmsOutageResolutionEndpoint),

				new ServiceReplicaListener (context =>
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
            await Initialize();

            while(true)
            {
                try
                {
                    await this.isolationAlgorithmCycle.Start();
                }
                catch (Exception e)
                {
                    Logger.LogError($"{baseLogString} RunAsync => Exception: {e.Message}");
                }

                await Task.Delay(this.isolationAlgorithmCycleInterval);
            }
        }

        private async Task Initialize()
        {
            try
            {
                InitializeReliableCollections();
                Logger.LogDebug($"{baseLogString} Initialize => ReliableDictionaries initialized.");

                var registerSubscriberClient = RegisterSubscriberClient.CreateClient();
                await registerSubscriberClient.SubscribeToTopic(Topic.SWITCH_STATUS, MicroserviceNames.OmsOutageLifecycleService);
                Logger.LogDebug($"{baseLogString} Initialize => Successfully subscribed to topics.");
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} Initialize => Exception: {e.Message}");
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
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, IsolationAlgorithm>>(ReliableDictionaryNames.StartedIsolationAlgorithms);
                        if(result.HasValue)
                        {
                            var gidToPointItemMap = result.Value;
                            await gidToPointItemMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, IsolationAlgorithm>>(tx, ReliableDictionaryNames.StartedIsolationAlgorithms);
                            await tx.CommitAsync();
                        }
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, DiscreteModbusData>>(ReliableDictionaryNames.MonitoredHeadBreakerMeasurements);
                        if(result.HasValue)
                        {
                            var gidToPointItemMap = result.Value;
                            await gidToPointItemMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, DiscreteModbusData>>(tx, ReliableDictionaryNames.MonitoredHeadBreakerMeasurements);
                            await tx.CommitAsync();
                        }
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, Dictionary<long, List<long>>>>(ReliableDictionaryNames.RecloserOutageMap);
                        if(result.HasValue)
                        {
                            var gidToPointItemMap = result.Value;
                            await gidToPointItemMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, Dictionary<long, List<long>>>>(tx, ReliableDictionaryNames.RecloserOutageMap);
                            await tx.CommitAsync();
                        }
                    }
                }),
            };

            Task.WaitAll(tasks);
        }
    }
}
