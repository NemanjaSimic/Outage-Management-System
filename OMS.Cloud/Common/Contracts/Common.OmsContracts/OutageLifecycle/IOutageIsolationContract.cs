using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.OmsContracts.OutageLifecycle
{
    [ServiceContract]
	public interface IOutageIsolationContract : IService, IHealthChecker
	{
		[OperationContract]
		Task IsolateOutage(long outageId);
	}
}
