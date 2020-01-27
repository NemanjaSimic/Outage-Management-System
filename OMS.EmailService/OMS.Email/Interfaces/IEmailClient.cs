namespace OMS.Email.Interfaces
{
    using OMS.Email.Models;
    using System.Collections.Generic;

    public interface IEmailClient
    {
        bool Connect();
        IEnumerable<OutageMailMessage> GetUnreadMessages();
    }
}
