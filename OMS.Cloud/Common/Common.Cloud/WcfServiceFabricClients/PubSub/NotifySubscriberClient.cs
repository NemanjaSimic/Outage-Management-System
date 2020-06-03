using Common.PubSub;
using Common.PubSubContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Outage.Common;
using System;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.PubSub
{
    public class NotifySubscriberClient : WcfSeviceFabricClientBase<INotifySubscriberContract>, INotifySubscriberContract
    {
        private static readonly string listenerName = EndpointNames.NotifySubscriberEndpoint;

        public NotifySubscriberClient(WcfCommunicationClientFactory<INotifySubscriberContract> clientFactory, Uri serviceName, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceName, servicePartition, listenerName)
        {
        }

        public static NotifySubscriberClient CreateClient(Uri serviceUri, ServiceType serviceType)
        {
            ServicePartitionKey servicePartition;

            if (serviceType == ServiceType.STATEFUL_SERVICE)
            {
                servicePartition = new ServicePartitionKey(0);
            }
            else if(serviceType == ServiceType.STATELESS_SERVICE)
            {
                servicePartition = ServicePartitionKey.Singleton;
            }
            else
            {
                throw new Exception("CreateClient<NotifySubscriberClient> => UNKNOWN value of ServiceType.");
            }

            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<NotifySubscriberClient, INotifySubscriberContract>(serviceUri, servicePartition);
        }

        #region INotifySubscriberContract
        /// <summary>
        /// OBAZRIVO, treba srediti cancellation tokene, sa tajmerima...
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<Uri> GetSubscriberUri()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetSubscriberUri());
        }

        /// <summary>
        /// OBAZRIVO, treba srediti cancellation tokene, sa tajmerima...
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<bool> Notify(IPublishableMessage message)
        {
            return InvokeWithRetryAsync(client => client.Channel.Notify(message));
        }
        #endregion INotifySubscriberContract
    }
}
