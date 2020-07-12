﻿using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.ScadaContracts.ModelProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.SCADA
{
    public class ScadaModelReadAccessClient : WcfSeviceFabricClientBase<IScadaModelReadAccessContract>, IScadaModelReadAccessContract
    {
        private static readonly string microserviceName = MicroserviceNames.ScadaModelProviderService;
        private static readonly string listenerName = EndpointNames.ScadaModelReadAccessEndpoint;

        public ScadaModelReadAccessClient(WcfCommunicationClientFactory<IScadaModelReadAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static ScadaModelReadAccessClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = new ServicePartitionKey(0);

            if (serviceUri == null)
            {
                return factory.CreateClient<ScadaModelReadAccessClient, IScadaModelReadAccessContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<ScadaModelReadAccessClient, IScadaModelReadAccessContract>(serviceUri, servicePartition);
            }
        }

        #region IScadaModelAccessContract
        public Task<Dictionary<short, Dictionary<ushort, long>>> GetAddressToGidMap()
        {
            return MethodWrapperAsync<Dictionary<short, Dictionary<ushort, long>>>("GetAddressToGidMap", new object[0]);
            //return InvokeWithRetryAsync(client => client.Channel.GetAddressToGidMap());
        }

        public Task<Dictionary<short, Dictionary<ushort, IScadaModelPointItem>>> GetAddressToPointItemMap()
        {
            return MethodWrapperAsync<Dictionary<short, Dictionary<ushort, IScadaModelPointItem>>>("GetAddressToPointItemMap", new object[0]);
            //return InvokeWithRetryAsync(client => client.Channel.GetAddressToPointItemMap());
        }

        public Task<Dictionary<long, CommandDescription>> GetCommandDescriptionCache()
        {
            return MethodWrapperAsync<Dictionary<long, CommandDescription>>("GetCommandDescriptionCache", new object[0]);
            //return InvokeWithRetryAsync(client => client.Channel.GetCommandDescriptionCache());
        }

        public Task<Dictionary<long, IScadaModelPointItem>> GetGidToPointItemMap()
        {
            return MethodWrapperAsync<Dictionary<long, IScadaModelPointItem>>("GetGidToPointItemMap", new object[0]);
            //return InvokeWithRetryAsync(client => client.Channel.GetGidToPointItemMap());
        }

        public Task<bool> GetIsScadaModelImportedIndicator()
        {
            return MethodWrapperAsync<bool>("GetIsScadaModelImportedIndicator", new object[0]);
            //return InvokeWithRetryAsync(client => client.Channel.GetIsScadaModelImportedIndicator());
        }

        public Task<IScadaConfigData> GetScadaConfigData()
        {
            return MethodWrapperAsync<IScadaConfigData>("GetScadaConfigData", new object[0]);
            //return InvokeWithRetryAsync(client => client.Channel.GetScadaConfigData());
        }
        #endregion
    }
}