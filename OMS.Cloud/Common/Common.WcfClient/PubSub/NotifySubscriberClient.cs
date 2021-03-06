﻿using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.Interfaces;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.PubSub
{
    public class NotifySubscriberClient : WcfSeviceFabricClientBase<INotifySubscriberContract>, INotifySubscriberContract
    {
        private static readonly string listenerName = EndpointNames.PubSubNotifySubscriberEndpoint;

        public NotifySubscriberClient(WcfCommunicationClientFactory<INotifySubscriberContract> clientFactory, Uri serviceName, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceName, servicePartition, listenerName)
        {
        }

        public static INotifySubscriberContract CreateClient(string serviceName)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<NotifySubscriberClient, INotifySubscriberContract>(serviceName);
        }

        public static INotifySubscriberContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<NotifySubscriberClient, INotifySubscriberContract>(serviceUri, servicePartitionKey);
        }

        #region INotifySubscriberContract
        /// <summary>
        /// OBAZRIVO, treba srediti cancellation tokene, sa tajmerima...
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<string> GetSubscriberName()
        {
            //return MethodWrapperAsync<string>("GetSubscriberName", new object[0]);
            return InvokeWithRetryAsync(client => client.Channel.GetSubscriberName());
        }

        /// <summary>
        /// OBAZRIVO, treba srediti cancellation tokene, sa tajmerima...
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task Notify(IPublishableMessage message, string publisherName)
        {
            //return MethodWrapperAsync("Notify", new object[2] { message, publisherName });
            return InvokeWithRetryAsync(client => client.Channel.Notify(message, publisherName));
        }
        #endregion INotifySubscriberContract

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
    }
}
