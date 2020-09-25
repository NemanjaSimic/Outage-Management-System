using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts
{
	[ServiceContract]
	public interface ISwitchStatusCommandingContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<bool> SendOpenCommand(long gid);

		[OperationContract]
		Task<bool> SendCloseCommand(long gid);
	}
}
