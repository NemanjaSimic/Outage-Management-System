using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.TmsContracts;
using TMS.TransactionManagerImplementation;
using TMS.TransactionManagerImplementation.ContractProviders;

namespace TMS.TransactionManagerService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TransactionManagerService : StatefulService
    {
        private readonly string baseLogString;
        private readonly ITransactionCoordinatorContract transactionCoordinatorProvider;
        private readonly ITransactionEnlistmentContract transactionEnlistmentProvider;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public TransactionManagerService(StatefulServiceContext context)
            : base(context)
        {
            this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);

            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            try
            {
                //DONE THIS WAY (in this order) BECAUSE: there is a mechanism that tracks the initialization process of reliable collections, which is set in constructors of these classes
                this.transactionCoordinatorProvider = new TransactionCoordinatorProvider(this.StateManager);
                this.transactionEnlistmentProvider = new TransactionEnlistmentProvider(this.StateManager);

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
            //return new ServiceReplicaListener[0];
            return new[]
            {
                //ScadaModelReadAccessEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<ITransactionCoordinatorContract>(context,
                                                                            this.transactionCoordinatorProvider,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.TmsTransactionCoordinatorEndpoint);
                }, EndpointNames.TmsTransactionCoordinatorEndpoint),

                //ScadaModelUpdateAccessEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<ITransactionEnlistmentContract>(context,
                                                                            this.transactionEnlistmentProvider,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.TmsTransactionEnlistmentEndpoint);
                }, EndpointNames.TmsTransactionEnlistmentEndpoint),
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
                        await StateManager.GetOrAddAsync<IReliableDictionary<string, HashSet<string>>>(tx, ReliableDictionaryNames.TransactionEnlistmentLedger);
                        await tx.CommitAsync();
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        await StateManager.GetOrAddAsync<IReliableDictionary<string, HashSet<string>>>(tx, ReliableDictionaryNames.ActiveTransactions);
                        await tx.CommitAsync();
                    }
                }),
            };

            Task.WaitAll(tasks);
        }
    }
}
