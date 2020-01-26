using OMS.Email.Dispatchers;
using OMS.Email.EmailParsers;
using OMS.Email.Imap;

namespace OMS.Email.Factories
{
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
