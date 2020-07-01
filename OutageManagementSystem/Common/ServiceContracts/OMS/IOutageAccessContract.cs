using Microsoft.ServiceFabric.Services.Remoting;
using Outage.Common.PubSub.OutageDataContract;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.OMS
{
    [ServiceContract]
    public interface IOutageAccessContract : IReportingContract
    {
        [OperationContract]
        IEnumerable<ActiveOutageMessage> GetActiveOutages();

        [OperationContract]
        IEnumerable<ArchivedOutageMessage> GetArchivedOutages();
    }
}