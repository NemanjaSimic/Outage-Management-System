﻿using System;
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
using OMS.Common.NmsContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.ScadaContracts.ModelProvider;

using SCADA.ModelProviderImplementation;
using SCADA.ModelProviderImplementation.ContractProviders;

namespace SCADA.ModelProviderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ModelProviderService : StatefulService
    {
        private readonly ICloudLogger logger;

        private readonly ScadaModel scadaModel;
        private readonly ModelReadAccessProvider modelReadAccessProvider;
        private readonly ModelUpdateAccessProvider modelUpdateAccessProvider;
        private readonly IntegrityUpdateProvider integrityUpdateProvider;

        public ModelProviderService(StatefulServiceContext context)
            : base(context)
        {
            logger = CloudLoggerFactory.GetLogger();

            try
            {
                //DONE THIS WAY (in this order) BECAUSE: there is a mechanism that tracks the initialization process of reliable collections, which is set in constructors of these classes
                this.scadaModel = new ScadaModel(this.StateManager, new ModelResourcesDesc(), new EnumDescs());
                this.modelReadAccessProvider = new ModelReadAccessProvider(this.StateManager);
                this.modelUpdateAccessProvider = new ModelUpdateAccessProvider(this.StateManager);
                this.integrityUpdateProvider = new IntegrityUpdateProvider(this.StateManager);
 
                string message = "Contract providers initialized.";
                logger.LogInformation(message);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[ModelProviderService | Information] {message}");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[ModelProviderService | Error] {e.Message}");
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
                    return new WcfCommunicationListener<IScadaModelReadAccessContract>(context,
                                                                            this.modelReadAccessProvider,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.ScadaModelReadAccessEndpoint);
                }, EndpointNames.ScadaModelReadAccessEndpoint),

                //ScadaModelUpdateAccessEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaModelUpdateAccessContract>(context,
                                                                            this.modelUpdateAccessProvider,
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.ScadaModelUpdateAccessEndpoint);
                }, EndpointNames.ScadaModelUpdateAccessEndpoint),

                //SCADAIntegrityUpdateEndpoint
                new ServiceReplicaListener(context =>
                {
                    return new WcfCommunicationListener<IScadaIntegrityUpdateContract>(context,
                                                                           this.integrityUpdateProvider,
                                                                           WcfUtility.CreateTcpListenerBinding(),
                                                                           EndpointNames.ScadaIntegrityUpdateEndpoint);
                }, EndpointNames.ScadaIntegrityUpdateEndpoint),

                ////SCADAModelUpdateNotifierEndpoint
                //new ServiceReplicaListener(context =>
                //{
                //    return new WcfCommunicationListener<IModelUpdateNotificationContract>(context,
                //                                                            new ScadaModelUpdateNotification(scadaModel),
                //                                                            WcfUtility.CreateTcpListenerBinding(),
                //                                                            EndpointNames.SCADAModelUpdateNotifierEndpoint);
                //}, EndpointNames.SCADAModelUpdateNotifierEndpoint),

                ////SCADATransactionActorEndpoint
                //new ServiceReplicaListener(context =>
                //{
                //    return new WcfCommunicationListener<ITransactionActorContract>(context,
                //                                                            new ScadaTransactionActor(scadaModel),
                //                                                            WcfUtility.CreateTcpListenerBinding(),
                //                                                            EndpointNames.SCADATransactionActorEndpoint);
                //}, EndpointNames.SCADATransactionActorEndpoint),
            };
        }

        protected async override Task RunAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                InitializeReliableCollections();
                string message = "ReliableDictionaries initialized.";
                logger.LogInformation(message);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[ModelProviderService | Information] {message}");

                await scadaModel.InitializeScadaModel();
                message = "ScadaModel initialized.";
                logger.LogInformation(message);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[ModelProviderService | Information] {message}");
            }
            catch (Exception e)
            {
                logger.LogInformation(e.Message, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[ModelProviderService | Error] {e.Message}");
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
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, IScadaModelPointItem>>(ReliableDictionaryNames.GidToPointItemMap);
                        if(result.HasValue)
                        {
                            var gidToPointItemMap = result.Value;
                            await gidToPointItemMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, IScadaModelPointItem>>(tx, ReliableDictionaryNames.GidToPointItemMap);
                            await tx.CommitAsync();
                        }
                    }
                }),
                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<short, Dictionary<ushort, long>>>(ReliableDictionaryNames.AddressToGidMap);
                        if(result.HasValue)
                        {
                            var addressToGidMap = result.Value;
                            await addressToGidMap.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<short, Dictionary<ushort, long>>>(tx, ReliableDictionaryNames.AddressToGidMap);
                            await tx.CommitAsync();
                        }
                    }
                }),
                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, CommandDescription>>(ReliableDictionaryNames.CommandDescriptionCache);
                        if(result.HasValue)
                        {
                            var commandDescriptionCache = result.Value;
                            await commandDescriptionCache.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, CommandDescription>>(tx, ReliableDictionaryNames.CommandDescriptionCache);
                            await tx.CommitAsync();
                        }
                    }
                }),
                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<long, ModbusData>>(ReliableDictionaryNames.MeasurementsCache);
                        if(result.HasValue)
                        {
                            var measurementsCache = result.Value;
                            await measurementsCache.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<long, ModbusData>>(tx, ReliableDictionaryNames.MeasurementsCache);
                            await tx.CommitAsync();
                        }
                    }
                }),
                Task.Run(async() =>
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        var result = await StateManager.TryGetAsync<IReliableDictionary<string, bool>>(ReliableDictionaryNames.InfoCache);
                        if(result.HasValue)
                        {
                            var measurementsCache = result.Value;
                            await measurementsCache.ClearAsync();
                            await tx.CommitAsync();
                        }
                        else
                        {
                            await StateManager.GetOrAddAsync<IReliableDictionary<string, bool>>(tx, ReliableDictionaryNames.InfoCache);
                            await tx.CommitAsync();
                        }
                    }
                }),
            };

            Task.WaitAll(tasks);
        }
    }
}