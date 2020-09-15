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
    internal sealed class FunctionExecutorService : StatefulService
    {
        private readonly string baseLogString;
        private readonly FunctionExecutorCycle functionExecutorCycle;
        private readonly ReadCommandEnqueuer readCommandEnqueuer;
        private readonly WriteCommandEnqueuer writeCommandEnqueuer;
        private readonly ModelUpdateCommandEnqueuer modelUpdateCommandEnqueuer;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public FunctionExecutorService(StatefulServiceContext context)
            : base(context)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            try
            {
                this.functionExecutorCycle = new FunctionExecutorCycle(StateManager);
                this.readCommandEnqueuer = new ReadCommandEnqueuer(StateManager);
                this.writeCommandEnqueuer = new WriteCommandEnqueuer(StateManager);
                this.modelUpdateCommandEnqueuer = new ModelUpdateCommandEnqueuer(StateManager);

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
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            //return new ServiceInstanceListener[0];
            return new List<ServiceReplicaListener>()
            {
                //ScadaReadCommandEnqueuerEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IReadCommandEnqueuerContract>(context,
                                                                              this.readCommandEnqueuer,
                                                                              WcfUtility.CreateTcpListenerBinding(),
                                                                              EndpointNames.ScadaReadCommandEnqueuerEndpoint);
                }, EndpointNames.ScadaReadCommandEnqueuerEndpoint),

                //ScadaWriteCommandEnqueuerEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IWriteCommandEnqueuerContract>(context,
                                                                               this.writeCommandEnqueuer,
                                                                               WcfUtility.CreateTcpListenerBinding(),
                                                                               EndpointNames.ScadaWriteCommandEnqueuerEndpoint);
                }, EndpointNames.ScadaWriteCommandEnqueuerEndpoint),

                //ScadaModelUpdateCommandEnqueueurEndpoint
                new ServiceReplicaListener(context =>
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
            IScadaConfigData configData;

            try
            {
                InitializeReliableCollections();
                string debugMessage = $"{baseLogString} RunAsync => ReliableDictionaries initialized.";
                Logger.LogDebug(debugMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[SCADA.FunctionExecutorService | Information] {debugMessage}");

                IScadaModelReadAccessContract readAccessClient = ScadaModelReadAccessClient.CreateClient();
                configData = await readAccessClient.GetScadaConfigData();

                string infoMessage = $"{baseLogString} RunAsync => FunctionExecutorCycle initialized.";
                Logger.LogInformation(infoMessage);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[SCADA.FunctionExecutorService | Information] {infoMessage}");
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} RunAsync => exception {e.Message}";
                Logger.LogError(errorMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[SCADA.FunctionExecutorService | Error] {errorMessage}");
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
                }
                catch (Exception e)
                {
                    string errorMessage = $"{baseLogString} RunAsync => exception {e.Message}";
                    Logger.LogError(errorMessage, e);
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"[SCADA.FunctionExecutorService | Error] {errorMessage}");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(configData.FunctionExecutionInterval), cancellationToken);
                functionExecutionCycleCount++;
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
                        var result = await StateManager.TryGetAsync<IReliableQueue<IReadModbusFunction>>(ReliableQueueNames.ReadCommandQueue);
                        if(result.HasValue)
                        {
                            var gidToPointItemMap = result.Value;
                            await gidToPointItemMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableQueue<IReadModbusFunction>>(tx, ReliableQueueNames.ReadCommandQueue);
                            await tx.CommitAsync();
                        }
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableQueue<IWriteModbusFunction>>(ReliableQueueNames.WriteCommandQueue);
                        if(result.HasValue)
                        {
                            var gidToPointItemMap = result.Value;
                            await gidToPointItemMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableQueue<IWriteModbusFunction>>(tx, ReliableQueueNames.WriteCommandQueue);
                            await tx.CommitAsync();
                        }
                    }
                }),

                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableQueue<IWriteModbusFunction>>(ReliableQueueNames.ModelUpdateCommandQueue);
                        if(result.HasValue)
                        {
                            var addressToGidMap = result.Value;
                            await addressToGidMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableQueue<IWriteModbusFunction>>(tx, ReliableQueueNames.ModelUpdateCommandQueue);
                            await tx.CommitAsync();
                        }
                    }
                }),
            };

            Task.WaitAll(tasks);
        }
    }
}
