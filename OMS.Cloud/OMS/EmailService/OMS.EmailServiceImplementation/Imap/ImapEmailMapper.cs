using ImapX;
using OMS.EmailImplementation.Interfaces;
using OMS.EmailImplementation.Models;

namespace OMS.EmailImplementation.Imap
{
    public class ImapEmailMapper : IImapEmailMapper
    {
        public OutageMailMessage MapMail(Message message)
        {
            return new OutageMailMessage
            {
                SenderEmail = message.From.Address,
                SenderDisplayName = message.From.DisplayName,
                Body = message.Body.Text
            };
        }
    }
}
