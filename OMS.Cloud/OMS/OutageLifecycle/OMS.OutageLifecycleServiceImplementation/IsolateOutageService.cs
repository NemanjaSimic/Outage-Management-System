using Common.OmsContracts.OutageLifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleServiceImplementation
{
	public class IsolateOutageService : IIsolateOutageContract
	{
		public Task IsolateOutage(long outageId)
		{
			throw new NotImplementedException();
		}
	}
}
