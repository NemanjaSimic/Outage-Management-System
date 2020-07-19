using Common.OmsContracts.OutageLifecycle;
using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleServiceImplementation
{
	public class ReportOutageService : IReportOutageContract
	{
		public bool ReportPotentialOutage(long gid, CommandOriginType commandOriginType)
		{
			throw new NotImplementedException();
		}
	}
}
