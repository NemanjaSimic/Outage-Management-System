using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using OMS.Common.ScadaContracts.ModelProvider;
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
        private readonly string baseLogString;
        private readonly ReadCommandEnqueuer readCommandEnqueuer;
        private readonly WriteCommandEnqueuer writeCommandEnqueuer;
        private readonly ModelUpdateCommandEnqueuer modelUpdateCommandEnqueuer;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public FunctionExecutorService(StatelessServiceContext context)
            : base(context)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            try
            {
                //CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.ReadCommandQueue, out CloudQueue readCommandQueue);
                //CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.WriteCommandQueue, out CloudQueue writeCommandQueue);
                //CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.ModelUpdateCommandQueue, out CloudQueue modelUpdateCommandQueue);

                //this.readCommandEnqueuer = new ReadCommandEnqueuer(readCommandQueue, writeCommandQueue, modelUpdateCommandQueue);
                //this.writeCommandEnqueuer = new WriteCommandEnqueuer(readCommandQueue, writeCommandQueue, modelUpdateCommandQueue);
                //this.modelUpdateCommandEnqueuer = new ModelUpdateCommandEnqueuer(readCommandQueue, writeCommandQueue, modelUpdateCommandQueue);

                this.readCommandEnqueuer = new ReadCommandEnqueuer();
                this.writeCommandEnqueuer = new WriteCommandEnqueuer();
                this.modelUpdateCommandEnqueuer = new ModelUpdateCommandEnqueuer();

                string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
                Logger.LogInformation(infoMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Information] {infoMessage}");
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Ctor => exception {e.Message}";
                Logger.LogError(errorMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Error] {errorMessage}");
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
                    return new WcfCommunicationListener<IReadCommandEnqueuerContract>(context,
                                                                              this.readCommandEnqueuer,
                                                                              WcfUtility.CreateTcpListenerBinding(),
                                                                              EndpointNames.ScadaReadCommandEnqueuerEndpoint);
                }, EndpointNames.ScadaReadCommandEnqueuerEndpoint),

                //ScadaWriteCommandEnqueuerEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IWriteCommandEnqueuerContract>(context,
                                                                               this.writeCommandEnqueuer,
                                                                               WcfUtility.CreateTcpListenerBinding(),
                                                                               EndpointNames.ScadaWriteCommandEnqueuerEndpoint);
                }, EndpointNames.ScadaWriteCommandEnqueuerEndpoint),

                //ScadaModelUpdateCommandEnqueueurEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IModelUpdateCommandEnqueuerContract>(context,
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
                IScadaModelReadAccessContract readAccessClient = ScadaModelReadAccessClient.CreateClient();
                configData = await readAccessClient.GetScadaConfigData();

                string infoMessage = $"{baseLogString} RunAsync => FunctionExecutorCycle initialized.";
                Logger.LogInformation(infoMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Information] {infoMessage}");
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} RunAsync => exception {e.Message}";
                Logger.LogError(errorMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Error] {errorMessage}");
                throw e;
            }

            var functionExecutionCycleCount = 0;

            while (true)
            {
                string message = $"{baseLogString} RunAsync => FunctionExecutionCycleCount: {functionExecutionCycleCount}";

                if (functionExecutionCycleCount % 100 == 0)
                {
                    Logger.LogInformation(message);
                }
                else if (functionExecutionCycleCount % 10 == 0)
                {
                    Logger.LogDebug(message);
                }
                else
                {
                    Logger.LogVerbose(message);
                }

                try
                {
                    await functionExecutorCycle.Start();

                    string verboseMessage = $"{baseLogString} RunAsync => FunctionExecutorCycle executed.";
                    Logger.LogVerbose(verboseMessage);
                    //ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Information] {message}");
                }
                catch (Exception e)
                {
                    string errorMessage = $"{baseLogString} RunAsync => exception {e.Message}";
                    Logger.LogError(errorMessage, e);
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"[FunctionExecutorService | Error] {errorMessage}");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(configData.FunctionExecutionInterval), cancellationToken);
                functionExecutionCycleCount++;
            }
        }
    }
}
