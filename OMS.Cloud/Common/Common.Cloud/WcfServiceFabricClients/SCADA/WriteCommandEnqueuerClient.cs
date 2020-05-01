using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    public class WriteCommandEnqueuerClient : WcfSeviceFabricClientBase<IWriteCommandEnqueuer>, IWriteCommandEnqueuer
    {
        public WriteCommandEnqueuerClient(WcfCommunicationClientFactory<IWriteCommandEnqueuer> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition)
        {
        }

        public static WriteCommandEnqueuerClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<WriteCommandEnqueuerClient, IWriteCommandEnqueuer>(MicroserviceNames.ScadaFunctionExecutorService, servicePartition);
            }
            else
            {
                return factory.CreateClient<WriteCommandEnqueuerClient, IWriteCommandEnqueuer>(serviceUri, servicePartition);
            }
        }

        #region IModelUpdateCommandEnqueuer
        public Task<bool> EnqueueWriteCommand(IWriteModbusFunction modbusFunctions)
        {
            return InvokeWithRetryAsync(client => client.Channel.EnqueueWriteCommand(modbusFunctions));
        }
        #endregion
    }
}
