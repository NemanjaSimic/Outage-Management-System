using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.SCADA
{
    public class ReadCommandEnqueuerClient : WcfSeviceFabricClientBase<IReadCommandEnqueuerContract>, IReadCommandEnqueuerContract
    {
        private static readonly string microserviceName = MicroserviceNames.ScadaFunctionExecutorService;
        private static readonly string listenerName = EndpointNames.ScadaReadCommandEnqueuerEndpoint;

        public ReadCommandEnqueuerClient(WcfCommunicationClientFactory<IReadCommandEnqueuerContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static ReadCommandEnqueuerClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<ReadCommandEnqueuerClient, IReadCommandEnqueuerContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<ReadCommandEnqueuerClient, IReadCommandEnqueuerContract>(serviceUri, servicePartition);
            }
        }

        #region IModelUpdateCommandEnqueuer
        public Task<bool> EnqueueReadCommand(IReadModbusFunction modbusFunctions)
        {
            return MethodWrapperAsync<bool>("EnqueueReadCommand", new object[1] { modbusFunctions });
            //return InvokeWithRetryAsync(client => client.Channel.EnqueueReadCommand(modbusFunctions));
        }
        #endregion
    }
}
