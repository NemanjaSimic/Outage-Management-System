using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.OmsContracts.OutageLifecycle
{
    [ServiceContract]
	public interface IPotentialOutageReportingContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<bool> ReportPotentialOutage(long gid, CommandOriginType commandOriginType);
	}
}
