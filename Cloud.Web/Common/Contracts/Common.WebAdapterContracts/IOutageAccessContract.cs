using Common.PubSubContracts.DataContracts.OMS;
using Outage.Common.ServiceContracts.OMS;
using System.Collections.Generic;
using System.ServiceModel;

namespace Common.Contracts.WebAdapterContracts
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