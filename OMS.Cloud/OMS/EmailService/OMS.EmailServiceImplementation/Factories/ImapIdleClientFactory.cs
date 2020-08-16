using OMS.EmailImplementation.Dispatchers;
using OMS.EmailImplementation.EmailParsers;
using OMS.EmailImplementation.Imap;
using OMS.Common.WcfClient.PubSub;

namespace OMS.EmailImplementation.Factories
{
    public class ImapIdleClientFactory
	{
        public ImapIdleEmailClient CreateClient() =>
            new ImapIdleEmailClient(
                new ImapEmailMapper(),
                new OutageEmailParser(),
                PublisherClient.CreateClient(),
                new GraphHubDispatcher());
    }
}
