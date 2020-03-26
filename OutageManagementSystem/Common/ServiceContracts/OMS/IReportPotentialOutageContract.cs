using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.OMS
{
    [ServiceContract]
    public interface IReportPotentialOutageContract : IService
    {
        [OperationContract]
        bool ReportPotentialOutage(long elementGid);
    }
}
