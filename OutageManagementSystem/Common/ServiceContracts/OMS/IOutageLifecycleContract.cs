using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts.OMS
{
    [ServiceContract]
    public interface IOutageLifecycleContract
    {
        [OperationContract]
        bool ReportOutage(long elementGid);

        [OperationContract]
        bool IsolateOutage(long outageId);

        [OperationContract]
        bool SendRepairCrew(long outageId);

        //TODO: mozda posebni contract-i za SCADA i NoSCADA deo...
        [OperationContract]
        bool SendLocationIsolationCrew(long outageId);

        [OperationContract]
        bool ValidateResolveConditions(long outageId);

        [OperationContract]
        bool ResolveOutage(long outageId);
    }
}
