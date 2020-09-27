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
		Task<bool> EnqueuePotentialOutageCommand(long elementGid, CommandOriginType commandOriginType, NetworkType networkType);

		[OperationContract]
		Task<bool> ReportPotentialOutage(long gid, CommandOriginType commandOriginType, NetworkType networkType);
	}
}
