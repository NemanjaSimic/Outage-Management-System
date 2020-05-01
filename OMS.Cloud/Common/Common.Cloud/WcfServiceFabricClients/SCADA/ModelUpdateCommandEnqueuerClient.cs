using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    public class ModelUpdateCommandEnqueuerClient : WcfSeviceFabricClientBase<IModelUpdateCommandEnqueuer>, IModelUpdateCommandEnqueuer
    {
        public ModelUpdateCommandEnqueuerClient(WcfCommunicationClientFactory<IModelUpdateCommandEnqueuer> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition)
        {
        }

        public static ModelUpdateCommandEnqueuerClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<ModelUpdateCommandEnqueuerClient, IModelUpdateCommandEnqueuer>(MicroserviceNames.ScadaFunctionExecutorService, servicePartition);
            }
            else
            {
                return factory.CreateClient<ModelUpdateCommandEnqueuerClient, IModelUpdateCommandEnqueuer>(serviceUri, servicePartition);
            }
        }

        #region IModelUpdateCommandEnqueuer
        public Task<bool> EnqueueModelUpdateCommands(List<IWriteModbusFunction> modbusFunctions)
        {
            return InvokeWithRetryAsync(client => client.Channel.EnqueueModelUpdateCommands(modbusFunctions));
        }
        #endregion
    }
}
