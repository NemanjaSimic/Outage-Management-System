using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts
{
    [ServiceContract]

    public interface IMeasurementMapContract : IService, IHealthChecker
    {
        [OperationContract]
        Task<List<long>> GetMeasurementsOfElement(long elementId);

        [OperationContract]
        Task<Dictionary<long, long>> GetMeasurementToElementMap();

        [OperationContract]
        Task<Dictionary<long, List<long>>> GetElementToMeasurementMap();
    }
}
