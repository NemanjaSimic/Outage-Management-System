using ImapX;
using OMS.CallTrackingServiceImplementation.Interfaces;
using OMS.CallTrackingServiceImplementation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.CallTrackingServiceImplementation.Imap
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
