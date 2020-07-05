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
    public class WriteCommandEnqueuerClient : WcfSeviceFabricClientBase<IWriteCommandEnqueuerContract>, IWriteCommandEnqueuerContract
    {
        private static readonly string microserviceName = MicroserviceNames.ScadaFunctionExecutorService;
        private static readonly string listenerName = EndpointNames.ScadaWriteCommandEnqueuerEndpoint;

        public WriteCommandEnqueuerClient(WcfCommunicationClientFactory<IWriteCommandEnqueuerContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static WriteCommandEnqueuerClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<WriteCommandEnqueuerClient, IWriteCommandEnqueuerContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<WriteCommandEnqueuerClient, IWriteCommandEnqueuerContract>(serviceUri, servicePartition);
            }
        }

        #region IModelUpdateCommandEnqueuer
        public Task<bool> EnqueueWriteCommand(IWriteModbusFunction modbusFunctions)
        {
            return MethodWrapperAsync<bool>("EnqueueWriteCommand", new object[1] { modbusFunctions });
            //return InvokeWithRetryAsync(client => client.Channel.EnqueueWriteCommand(modbusFunctions));
        }
        #endregion
    }
}
