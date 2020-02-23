using Outage.Common.PubSub.OutageDataContract;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.OMS
{
    [ServiceContract]
    public interface IOutageAccessContract
    {
        [OperationContract]
        List<ActiveOutage> GetActiveOutages();

        [OperationContract]
        List<ArchivedOutage> GetArchivedOutages();
    }
}