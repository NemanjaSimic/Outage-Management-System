using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using OMS.Common.WcfClient.SCADA;

using SCADA.FunctionExecutorImplementation;
using SCADA.FunctionExecutorImplementation.CommandEnqueuers;

namespace SCADA.FunctionExecutorService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class FunctionExecutorService : StatelessService
    {
        private readonly ICloudLogger logger;

        private readonly ReadCommandEnqueuer readCommandEnqueuer;
        private readonly WriteCommandEnqueuer writeCommandEnqueuer;
        private readonly ModelUpdateCommandEnqueuer modelUpdateCommandEnqueuer;

        public FunctionExecutorService(StatelessServiceContext context)
            : base(context)
        {
            this.logger = CloudLoggerFactory.GetLogger();

            try
            {
                this.readCommandEnqueuer = new ReadCommandEnqueuer();
                this.writeCommandEnqueuer = new WriteCommandEnqueuer();
                this.modelUpdateCommandEnqueuer = new ModelUpdateCommandEnqueuer();

                string message = "Contract providers initialized.";
                logger.LogInformation(message);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Information] {message}");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Error] {e.Message}");
            }
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
                //ScadaReadCommandEnqueuerEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IReadCommandEnqueuer>(context,
                                                                              this.readCommandEnqueuer,
                                                                              WcfUtility.CreateTcpListenerBinding(),
                                                                              EndpointNames.ScadaReadCommandEnqueuerEndpoint);
                }, EndpointNames.ScadaReadCommandEnqueuerEndpoint),

                //ScadaWriteCommandEnqueuerEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IWriteCommandEnqueuer>(context,
                                                                               this.writeCommandEnqueuer,
                                                                               WcfUtility.CreateTcpListenerBinding(),
                                                                               EndpointNames.ScadaWriteCommandEnqueuerEndpoint);
                }, EndpointNames.ScadaWriteCommandEnqueuerEndpoint),

                //ScadaModelUpdateCommandEnqueueurEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IModelUpdateCommandEnqueuer>(context,
                                                                                     this.modelUpdateCommandEnqueuer,
                                                                                     WcfUtility.CreateTcpListenerBinding(),
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
            cancellationToken.ThrowIfCancellationRequested();

            FunctionExecutorCycle functionExecutorCycle;
            IScadaConfigData configData;

            try
            {
                functionExecutorCycle = new FunctionExecutorCycle();
                ScadaModelReadAccessClient readAccessClient = ScadaModelReadAccessClient.CreateClient();
                configData = await readAccessClient.GetScadaConfigData();

                string message = "FunctionExecutorCycle initialized.";
                logger.LogInformation(message);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Information] {message}");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Error] {e.Message}");
                throw e;
            }

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await functionExecutorCycle.Start();

                    string message = "FunctionExecutorCycle executed.";
                    logger.LogVerbose(message);
                    //ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Information] {message}");
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message, e);
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Error] {e.Message}");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(configData.FunctionExecutionInterval), cancellationToken);
            }
        }
    }
}
