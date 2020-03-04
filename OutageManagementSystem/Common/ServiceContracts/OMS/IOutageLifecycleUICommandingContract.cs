using System.ServiceModel;

namespace Outage.Common.ServiceContracts.OMS
{
    [ServiceContract]
    public interface IOutageLifecycleUICommandingContract
    {
        [OperationContract]
        //SCADA
        bool IsolateOutage(long outageId);

        [OperationContract]
        //NoSCADA
        bool SendLocationIsolationCrew(long outageId);

        [OperationContract]
        bool SendRepairCrew(long outageId);

        [OperationContract]
        bool ValidateResolveConditions(long outageId);

        [OperationContract]
        bool ResolveOutage(long outageId);
    }
}
