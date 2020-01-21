using ImapX;
using OMS.Email.Models;

namespace OMS.Email.Interfaces
{
    public interface IImapEmailMapper
    {
        OutageMailMessage MapMail(Message message);
    }
}
