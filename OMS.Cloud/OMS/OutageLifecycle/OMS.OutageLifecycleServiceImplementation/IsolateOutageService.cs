using Common.OmsContracts.OutageLifecycle;
using System;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleServiceImplementation
{
    public class IsolateOutageService : IIsolateOutageContract
	{
		public Task IsolateOutage(long outageId)
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsAlive()
		{
			return Task.Run(() => { return true; });
		}
	}
}
