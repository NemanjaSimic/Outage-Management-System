<<<<<<< HEAD
﻿using ImapX;
using OMS.Email.Interfaces;
using OMS.Email.Models;

namespace OMS.Email.Imap
{
=======
﻿namespace OMS.Email.Imap
{
    using ImapX;
    using OMS.Email.Interfaces;
    using OMS.Email.Models;

>>>>>>> 2800298cec0dac58b6c9a650c22ac579428c4bc6
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
