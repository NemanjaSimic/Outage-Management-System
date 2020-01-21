using OMS.Email.Models;
using System.Collections.Generic;

namespace OMS.Email.Interfaces
{
    public interface IEmailClient
    {
        bool Connect();
        IEnumerable<OutageMailMessage> GetUnreadMessages();
    }
}
