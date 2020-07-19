using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.OutageLifecycle
{
	[ServiceContract]
	public interface IReportOutageContract : IService
	{
		[OperationContract]
		bool ReportPotentialOutage(long gid, CommandOriginType commandOriginType);
	}
}
