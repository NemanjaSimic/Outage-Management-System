﻿using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.NMS
{
    public class NetworkModelGdaClient : WcfSeviceFabricClientBase<INetworkModelGDAContract>, INetworkModelGDAContract
    {
        public NetworkModelGdaClient(WcfCommunicationClientFactory<INetworkModelGDAContract> clientFactory, Uri serviceName) 
            : base(clientFactory, serviceName)
        {
        }

        public static NetworkModelGdaClient CreateClient(Uri nmsServiceName = null)
        {
            if (nmsServiceName == null && ConfigurationManager.AppSettings[MicroserviceNames.NmsGdaService] is string nmsGdaServiceName)
            {
                nmsServiceName = new Uri(nmsGdaServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<INetworkModelGDAContract>(CreateBinding(), null, partitionResolver);
            
            return new NetworkModelGdaClient(factory, nmsServiceName);
        }

        private static NetTcpBinding CreateBinding()
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

            var binding = WcfUtility.CreateTcpClientBinding();
            binding.SendTimeout = TimeSpan.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.CloseTimeout = TimeSpan.FromMinutes(1);

            return (NetTcpBinding)binding;
        }

        #region INetworkModelGDAContract
        public Task<UpdateResult> ApplyUpdate(Delta delta)
        {
            return InvokeWithRetryAsync(client => client.Channel.ApplyUpdate(delta));
        }

        public Task<int> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            return InvokeWithRetryAsync(client => client.Channel.GetExtentValues(entityType, propIds));
        }

        public Task<int> GetRelatedValues(long source, List<ModelCode> propIds, Association association)
        {
            return InvokeWithRetryAsync(client => client.Channel.GetRelatedValues(source, propIds, association));
        }

        public Task<ResourceDescription> GetValues(long resourceId, List<ModelCode> propIds)
        {
            return InvokeWithRetryAsync(client => client.Channel.GetValues(resourceId, propIds));
        }

        public Task<bool> IteratorClose(int id)
        {
            return InvokeWithRetryAsync(client => client.Channel.IteratorClose(id));
        }

        public Task<List<ResourceDescription>> IteratorNext(int n, int id)
        {
            return InvokeWithRetryAsync(client => client.Channel.IteratorNext(n, id));
        }

        public Task<int> IteratorResourcesLeft(int id)
        {
            return InvokeWithRetryAsync(client => client.Channel.IteratorResourcesLeft(id));
        }

        public Task<int> IteratorResourcesTotal(int id)
        {
            return InvokeWithRetryAsync(client => client.Channel.IteratorResourcesTotal(id));
        }

        public Task<bool> IteratorRewind(int id)
        {
            return InvokeWithRetryAsync(client => client.Channel.IteratorRewind(id));
        }
        #endregion
    }
}
