using OMS.Email.Imap;

namespace OMS.Email.Factories
{
    public class ImapClientFactory
    {
        public ImapEmailClient CreateClient() => 
            new ImapEmailClient(new ImapEmailMapper());
    }
}
