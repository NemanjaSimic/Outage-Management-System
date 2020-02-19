using Outage.Common.PubSub.OutageDataContract;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.OMS
{
    [ServiceContract]
    public interface IOutageContract
    {
        [OperationContract]
        bool ReportOutage(long elementGid);

        [OperationContract]
        List<ActiveOutage> GetActiveOutages();

        [OperationContract]
        List<ArchivedOutage> GetArchivedOutages();

        [OperationContract]
        bool UpdateActiveOutageIsolated(long elementGid, List<long> locatedElements);

        [OperationContract]
        bool UpdateActiveOutageResolved(long elementGid);

    }
}