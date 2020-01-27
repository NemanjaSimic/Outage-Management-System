namespace OMS.Email.Factories
{
    using OMS.Email.Imap;
    using OMS.Email.Dispatchers;
    using OMS.Email.EmailParsers;
    using Outage.Common.ServiceProxies.PubSub;
    using Outage.Common;

    public class ImapIdleClientFactory
    {
        public ImapIdleEmailClient CreateClient() =>
            new ImapIdleEmailClient(
                new ImapEmailMapper(),
                new OutageEmailParser(),
                new PublisherProxy(EndpointNames.PublisherEndpoint),
                new GraphHubDispatcher());
    }
}
