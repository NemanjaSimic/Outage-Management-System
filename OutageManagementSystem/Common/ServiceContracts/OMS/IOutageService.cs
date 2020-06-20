namespace Outage.Common.ServiceContracts.OMS
{
    // service naming convention
    //TODO: prosiriti i sa ostalim outage contract-ima? npr. IOutageLifecycleUICommandingContract
    public interface IOutageService : IOutageAccessContract, IReportingContract
    { 
    }
}
