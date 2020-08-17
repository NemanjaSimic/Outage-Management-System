using OMS.EmailImplementation.Models;
using System.Collections.Generic;

namespace OMS.EmailImplementation.Interfaces
{
    public interface IEmailClient
	{
		bool Connect();
		IEnumerable<OutageMailMessage> GetUnreadMessages();
	}
}
