using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.DistributedTransactionContracts;

namespace TMS.TransactionManagerService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TransactionManagerService : StatefulService
    {
        private readonly ITransactionCoordinatorContract transactionCoordinatorProvider;
        private readonly ITransactionEnlistmentContract transactionEnlistmentProvider;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public TransactionManagerService(StatefulServiceContext context)
            : base(context)
        { }

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
                                                                            EndpointNames.TransactionCoordinatorEndpoint);
                }, EndpointNames.TransactionCoordinatorEndpoint),

                //ScadaModelUpdateAccessEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<ITransactionEnlistmentContract>(context,
                                                                            this.transactionEnlistmentProvider,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.TransactionEnlistmentEndpoint);
                }, EndpointNames.TransactionEnlistmentEndpoint),
            };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {

        }
    }
}
