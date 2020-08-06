using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.CallTrackingServiceImplementation.Interfaces
{
	public interface IIdleEmailClient : IEmailClient
	{
		bool StartIdling();
		void RegisterIdleHandler();
	}
}
