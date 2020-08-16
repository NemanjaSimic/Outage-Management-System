using OMS.EmailImplementation.Dispatchers;
using OMS.EmailImplementation.EmailParsers;
using OMS.EmailImplementation.Imap;
using OMS.Common.WcfClient.PubSub;

namespace OMS.EmailImplementation.Factories
{
    public class ImapClientFactory
    {
        public ImapEmailClient CreateClient() =>
            new ImapEmailClient(
                new ImapEmailMapper(),
                new OutageEmailParser(),
                PublisherClient.CreateClient(), 
                new GraphHubDispatcher());
    }
}
