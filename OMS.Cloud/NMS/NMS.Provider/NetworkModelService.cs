using OMS.Cloud.NMS.Provider.DistributedTransaction;
using OMS.Cloud.NMS.Provider.GDA;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Outage.Common;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceContracts.GDA;
using System;
using System.Collections.Generic;
using NetTcpBinding = System.ServiceModel.NetTcpBinding;
using SecurityMode = System.ServiceModel.SecurityMode;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System.ServiceModel.Channels;

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
                {
                    return new WcfCommunicationListener<INetworkModelGDAContract>(context,
                                                                           new GenericDataAccess(networkModel),
                                                                           CreateBinding(),
                                                                           EndpointNames.NetworkModelGDAEndpoint);
                }, EndpointNames.NetworkModelGDAEndpoint),

                //NetworkModelTransactionActorEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<ITransactionActorContract>(context,
                                                                           new NMSTransactionActor(networkModel),
                                                                           CreateBinding(),
                                                                           EndpointNames.NetworkModelTransactionActorEndpoint);
                }, EndpointNames.NetworkModelTransactionActorEndpoint),
            };
        }

        private NetTcpBinding CreateBinding()
        {
            //NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            //{
            //    SendTimeout = TimeSpan.MaxValue,
            //    ReceiveTimeout = TimeSpan.MaxValue,
            //    OpenTimeout = TimeSpan.FromMinutes(1),
            //    CloseTimeout = TimeSpan.FromMinutes(1),
            //    MaxConnections = int.MaxValue,
            //    MaxReceivedMessageSize = 1024 * 1024 * 1024,
            //};

            //binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            //binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.MaxReceivedMessageSize;

            var binding = WcfUtility.CreateTcpListenerBinding();
            binding.SendTimeout = TimeSpan.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.CloseTimeout = TimeSpan.FromMinutes(1);

            return (NetTcpBinding)binding;
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
