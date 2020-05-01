using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using System;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    public class ReadCommandEnqueuerClient : WcfSeviceFabricClientBase<IReadCommandEnqueuer>, IReadCommandEnqueuer
    {
        public ReadCommandEnqueuerClient(WcfCommunicationClientFactory<IReadCommandEnqueuer> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition)
        {
        }

        public static ReadCommandEnqueuerClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<ReadCommandEnqueuerClient, IReadCommandEnqueuer>(MicroserviceNames.ScadaFunctionExecutorService, servicePartition);
            }
            else
            {
                return factory.CreateClient<ReadCommandEnqueuerClient, IReadCommandEnqueuer>(serviceUri, servicePartition);
            }
        }

        #region IModelUpdateCommandEnqueuer
        public Task<bool> EnqueueReadCommand(IReadModbusFunction modbusFunctions)
        {
            return InvokeWithRetryAsync(client => client.Channel.EnqueueReadCommand(modbusFunctions));
        }
        #endregion
    }
}
