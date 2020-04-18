using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Cloud.SCADA.FunctionExecutorService.CommandEnqueuers;
using OMS.Common.Cloud.WcfServiceFabricClients;
using OMS.Common.ScadaContracts;
using Outage.Common;

namespace OMS.Cloud.SCADA.FunctionExecutorService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class FunctionExecutorService : StatelessService
    {
        public FunctionExecutorCycle FunctionExecutorCycle { get; set; }
        public FunctionExecutorService(StatelessServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            //return new ServiceInstanceListener[0];
            return new List<ServiceInstanceListener>()
            {
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IReadCommandEnqueuer>(context,
                                                                            new ReadCommandEnqueuer(),
                                                                            TcpBindingHelper.CreateListenerBinding(),
                                                                            EndpointNames.ScadaReadCommandEnqueuerEndpoint);
                }, EndpointNames.ScadaReadCommandEnqueuerEndpoint),
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IWriteCommandEnqueuer>(context,
                                                                            new WriteCommandEnqueuer(),
                                                                            TcpBindingHelper.CreateListenerBinding(),
                                                                            EndpointNames.ScadaWriteCommandEnqueuerEndpoint);
                }, EndpointNames.ScadaWriteCommandEnqueuerEndpoint),
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IModelUpdateCommandEnqueuer>(context,
                                                                                    new ModelUpdateCommandEnqueuer(),
                                                                                    TcpBindingHelper.CreateListenerBinding(),
                                                                                    EndpointNames.ScadaModelUpdateCommandEnqueueurEndpoint);
                }, EndpointNames.ScadaModelUpdateCommandEnqueueurEndpoint)

            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            FunctionExecutorCycle functionExecutorCycle = new FunctionExecutorCycle();

            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await functionExecutorCycle.Start();
                    ServiceEventSource.Current.ServiceMessage(this.Context, "FunctionExecutorService::FunctionExecutorCycle.Start() Working-{0}", ++iterations);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.Message, "Error");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
