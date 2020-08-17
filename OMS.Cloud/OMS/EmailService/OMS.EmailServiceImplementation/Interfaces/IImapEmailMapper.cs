using ImapX;
using OMS.EmailImplementation.Models;

namespace OMS.EmailImplementation.Interfaces
{
	public interface IImapEmailMapper
	{
		OutageMailMessage MapMail(Message message);
	}
}
