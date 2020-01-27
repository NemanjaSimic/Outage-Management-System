namespace OMS.Email.Interfaces
{
    using ImapX;
    using OMS.Email.Models;
    
    public interface IImapEmailMapper
    {
        OutageMailMessage MapMail(Message message);
    }
}
