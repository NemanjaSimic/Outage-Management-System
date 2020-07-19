using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.OutageLifecycle
{
	[ServiceContract]
	public interface IValidateResolveConditionsContract : IService
	{
		[OperationContract]
		bool ValidateResolveConditions(long outageId);
	}
}
