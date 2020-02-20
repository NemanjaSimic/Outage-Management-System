using Outage.Common.PubSub.OutageDataContract;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.OMS
{
    //TODO: IOutageContract => IOutageAccessContract
    [ServiceContract]
    public interface IOutageContract
    {
        [OperationContract]
        List<ActiveOutage> GetActiveOutages();

        [OperationContract]
        List<ArchivedOutage> GetArchivedOutages();
    }
}