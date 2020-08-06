using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.CallTrackingServiceImplementation.Interfaces
{
	public interface IDispatcher
	{
		bool IsConnected { get; }
		void Dispatch(long gid);
	}
}
