using Common.PubSubContracts.DataContracts.CE;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.Interfaces;
using OMS.Common.WcfClient.PubSub;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SubscriberTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Hello from {MicroserviceNames.SubscriberTester}");

            Initialize().Wait();
            Console.ReadLine();
            UnsubscribeAll().Wait();

            Console.WriteLine($"Goodbye from {MicroserviceNames.SubscriberTester}");
            Console.ReadLine();
        }

        static async Task Initialize()
        {
            var host = new ServiceHost(typeof(Subscriber));
            host.AddServiceEndpoint(typeof(INotifySubscriberContract),
                                    new NetTcpBinding(),
                                    ServiceDefines.Instance.ServiceNameToServiceUri[MicroserviceNames.SubscriberTester]);
            host.Open();

            var topics = new List<Topic>
            {
                Topic.ACTIVE_OUTAGE,
                Topic.ARCHIVED_OUTAGE,
                Topic.MEASUREMENT,
                Topic.SWITCH_STATUS,
                Topic.OUTAGE_EMAIL,
                Topic.OMS_MODEL,
                Topic.TOPOLOGY,
            };

            var regSub = RegisterSubscriberClient.CreateClient();
            await regSub.SubscribeToTopics(topics, MicroserviceNames.SubscriberTester);
            var subTopics = await regSub.GetAllSubscribedTopics(MicroserviceNames.SubscriberTester);

            Console.WriteLine($"Number of subscribed Topics: {subTopics.Count}");
        }

        static async Task UnsubscribeAll()
        {
            var regSub = RegisterSubscriberClient.CreateClient();
            await regSub.UnsubscribeFromAllTopics(MicroserviceNames.SubscriberTester);
        }
    }

    public class Subscriber : INotifySubscriberContract
    {
        public async Task<string> GetSubscriberName()
        {
            return MicroserviceNames.SubscriberTester;
        }

        public async Task<bool> IsAlive()
        {
            return true;
        }

        public async Task Notify(IPublishableMessage message, string publisherName)
        {
            Console.WriteLine($"Message received. Message Type: {message.GetType()}, PublisherName: {publisherName}");

            if(message is TopologyForUIMessage uiTopologyMessage)
            {

                string uiTopologyString = $"FirstNode: 0x{uiTopologyMessage.UIModel.FirstNode:X16}{Environment.NewLine}" +
                                   $"Count of Nodes: {uiTopologyMessage.UIModel.Nodes.Count}{Environment.NewLine}" +
                                   $"Count of Relations: {uiTopologyMessage.UIModel.Relations.Count}{Environment.NewLine}";
                Console.WriteLine(uiTopologyString);
            }
            else if(message is OMSModelMessage omsModelMessage)
            {
                string omsModelString = $"FirstNode: 0x{omsModelMessage.OutageTopologyModel.FirstNode:X16}{Environment.NewLine}" +
                                         $"Count of Elements: {omsModelMessage.OutageTopologyModel.OutageTopology.Count}{Environment.NewLine}";
                Console.WriteLine(omsModelString);
            }
        }
    }
}
