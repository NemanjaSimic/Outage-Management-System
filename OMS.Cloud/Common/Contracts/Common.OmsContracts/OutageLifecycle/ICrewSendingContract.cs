using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.OmsContracts.OutageLifecycle
{
    [ServiceContract]
	public interface ICrewSendingContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<bool> SendLocationIsolationCrew(long outageId);

		[OperationContract]
		Task<bool> SendRepairCrew(long outageId);
	}
}
