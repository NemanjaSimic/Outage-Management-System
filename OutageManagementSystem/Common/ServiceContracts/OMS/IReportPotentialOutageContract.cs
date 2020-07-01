using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.OMS
{
    [ServiceContract]
    public interface IReportPotentialOutageContract
    {
        [OperationContract]
        bool ReportPotentialOutage(long elementGid, CommandOriginType commandOriginType);

        [OperationContract]
        void OnSwitchClose(long elementGid);
    }
}
