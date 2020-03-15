using CloudOMS.NetworkModelService.NMS.Provider.DistributedTransaction;
using CloudOMS.NetworkModelService.NMS.Provider.GDA;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Outage.Common;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceContracts.GDA;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.ServiceModel;

namespace CloudOMS.NetworkModelService.NMS.Provider
{
    public sealed class NetworkModelService : IDisposable
    {
        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private NetworkModel networkModel = null;
        public List<ServiceInstanceListener> Listeners { get; private set; }

        public NetworkModelService(StatelessServiceContext context)
        {
            networkModel = new NetworkModel();
            InitializeListeners(networkModel, context);
        }

        public void Dispose()
        {
            CloseListeners();
            GC.SuppressFinalize(this);
        }

        private void InitializeListeners(NetworkModel networkModel, StatelessServiceContext contex)
        {
            Listeners = new List<ServiceInstanceListener>()
            {
                //NetworkModelGDAEndpoint
                new ServiceInstanceListener(c =>
                {
                    var host = c.NodeContext.IPAddressOrFQDN;
                    var endpointConfig = c.CodePackageActivationContext.GetEndpoint(EndpointNames.NetworkModelGDAEndpoint);
                    int port = endpointConfig.Port;
                    string schema = endpointConfig.Protocol.ToString();
                    string uri = $"{schema}://{host}:{port}";
                    return new WcfCommunicationListener<INetworkModelGDAContract>(
                                wcfServiceObject: new GenericDataAccess(networkModel),
                                serviceContext: c,
                                listenerBinding: new NetTcpBinding(),
                                address: new EndpointAddress(uri)
                               );
                }),

                //NetworkModelTransactionActorEndpoint
                new ServiceInstanceListener(c =>
                {
                    var host = c.NodeContext.IPAddressOrFQDN;
                    var endpointConfig = c.CodePackageActivationContext.GetEndpoint(EndpointNames.NetworkModelTransactionActorEndpoint);
                    int port = endpointConfig.Port;
                    string schema = endpointConfig.Protocol.ToString();
                    string uri = $"{schema}://{host}:{port}";
                    return new WcfCommunicationListener<ITransactionActorContract>(
                                wcfServiceObject: new NMSTransactionActor(networkModel),
                                serviceContext: c,
                                listenerBinding: new NetTcpBinding(),
                                address: new EndpointAddress(uri)
                               );
                }),
                //new ServiceInstanceListener(c => new FabricTransportServiceRemotingListener(contex, new GenericDataAccess(networkModel)), EndpointNames.NetworkModelGDAEndpoint),
                //new ServiceInstanceListener(c => new FabricTransportServiceRemotingListener(contex, new NMSTransactionActor(networkModel)), EndpointNames.NetworkModelTransactionActorEndpoint),
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
