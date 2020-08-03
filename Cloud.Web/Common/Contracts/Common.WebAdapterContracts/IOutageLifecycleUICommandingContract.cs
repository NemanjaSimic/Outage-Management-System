using System.ServiceModel;

namespace Common.Contracts.WebAdapterContracts
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
