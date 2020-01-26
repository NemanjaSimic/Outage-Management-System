namespace OMS.Email.Factories
{
    using OMS.Email.Dispatchers;
    using OMS.Email.EmailParsers;
    using OMS.Email.Imap;
    
    public class ImapIdleClientFactory
    {
        public ImapIdleEmailClient CreateClient() =>
            new ImapIdleEmailClient(
                new ImapEmailMapper(),
                new OutageEmailParser(),
                new GraphHubDispatcher()
                );
    }
}
