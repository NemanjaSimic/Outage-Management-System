using OMS.Cloud.NMS.Provider.DistributedTransaction;
using OMS.Cloud.NMS.Provider.GDA;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Outage.Common;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceContracts.GDA;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Services.Communication.Wcf;

namespace OMS.Cloud.NMS.Provider
{
    public sealed class NetworkModelService : IDisposable
    {
        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private readonly NetworkModel networkModel = null;
        private List<ServiceInstanceListener> listeners = null;

        public List<ServiceInstanceListener> Listeners 
        {
            get { return listeners ?? (listeners = CreateListeners()); }
        }

        public NetworkModelService()
        {
            networkModel = new NetworkModel();
        }

        public void Dispose()
        {
            CloseListeners();
            GC.SuppressFinalize(this);
        }

        private List<ServiceInstanceListener> CreateListeners()
        {
            return new List<ServiceInstanceListener>()
            {
                //NetworkModelGDAEndpoint
                new ServiceInstanceListener(context => 
                    new WcfCommunicationListener<INetworkModelGDAContract>(context, 
                                                                           new GenericDataAccess(networkModel),
                                                                           WcfUtility.CreateTcpListenerBinding(),
                                                                           EndpointNames.NetworkModelGDAEndpoint),
                    EndpointNames.NetworkModelGDAEndpoint),

                //NetworkModelTransactionActorEndpoint
                new ServiceInstanceListener(context =>
                    new WcfCommunicationListener<ITransactionActorContract>(context, 
                                                                            new NMSTransactionActor(networkModel),
                                                                            WcfUtility.CreateTcpListenerBinding(),
                                                                            EndpointNames.NetworkModelTransactionActorEndpoint),
                    EndpointNames.NetworkModelTransactionActorEndpoint),
            };
        }

        private void CloseListeners()
        {
            networkModel.SaveNetworkModel();

            if (Listeners == null || Listeners.Count == 0)
            {
                throw new Exception("Network Model Services can not be closed because it is not initialized.");
            }

            Listeners.Clear();

            string message = "The Network Model Service is gracefully closed.";
            Logger.LogInfo(message);
            Console.WriteLine("\n\n{0}", message);
        }
    }
}
