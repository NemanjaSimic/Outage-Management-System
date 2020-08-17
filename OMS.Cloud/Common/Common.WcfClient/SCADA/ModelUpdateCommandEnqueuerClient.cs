using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.SCADA
{
    public class ModelUpdateCommandEnqueuerClient : WcfSeviceFabricClientBase<IModelUpdateCommandEnqueuerContract>, IModelUpdateCommandEnqueuerContract
    {
        private static readonly string microserviceName = MicroserviceNames.ScadaFunctionExecutorService;
        private static readonly string listenerName = EndpointNames.ScadaModelUpdateCommandEnqueueurEndpoint; 

        public ModelUpdateCommandEnqueuerClient(WcfCommunicationClientFactory<IModelUpdateCommandEnqueuerContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static IModelUpdateCommandEnqueuerContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ModelUpdateCommandEnqueuerClient, IModelUpdateCommandEnqueuerContract>(microserviceName);
        }

        public static IModelUpdateCommandEnqueuerContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ModelUpdateCommandEnqueuerClient, IModelUpdateCommandEnqueuerContract>(serviceUri, servicePartitionKey);
        }

        #region IModelUpdateCommandEnqueuer
        public Task<bool> EnqueueModelUpdateCommands(List<IWriteModbusFunction> modbusFunctions)
        {
            //return MethodWrapperAsync<bool>("EnqueueModelUpdateCommands", new object[1] { modbusFunctions });
            return InvokeWithRetryAsync(client => client.Channel.EnqueueModelUpdateCommands(modbusFunctions));
        }
        #endregion

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
    }
}
