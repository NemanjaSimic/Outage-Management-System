using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.OMS
{
    [ServiceContract]
    public interface ICallingContract : IService
    {
        [OperationContract]
        void ReportMalfunction(long consumerGid);
    }
}
