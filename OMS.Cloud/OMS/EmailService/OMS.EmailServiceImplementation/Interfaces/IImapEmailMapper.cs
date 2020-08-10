using ImapX;
using OMS.CallTrackingServiceImplementation.Models;

namespace OMS.CallTrackingServiceImplementation.Interfaces
{
	public interface IImapEmailMapper
	{
		OutageMailMessage MapMail(Message message);
	}
}
