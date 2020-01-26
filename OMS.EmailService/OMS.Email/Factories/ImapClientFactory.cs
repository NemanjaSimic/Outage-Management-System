namespace OMS.Email.Factories
{
    using OMS.Email.Imap;

    public class ImapClientFactory
    {
        public ImapEmailClient CreateClient() => 
            new ImapEmailClient(new ImapEmailMapper());
    }
}
