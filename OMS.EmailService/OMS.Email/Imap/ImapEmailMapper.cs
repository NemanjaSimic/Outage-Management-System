using ImapX;
using OMS.Email.Interfaces;
using OMS.Email.Models;

namespace OMS.Email.Imap
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
