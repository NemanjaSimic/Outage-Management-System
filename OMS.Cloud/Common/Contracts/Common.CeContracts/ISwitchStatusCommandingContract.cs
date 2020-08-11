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
		Task SendOpenCommand(long gid);

		[OperationContract]
		Task SendCloseCommand(long gid);
	}
}
