using Outage.Common.PubSub.OutageDataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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

    }
}