using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.CalculationEngine
{
    [ServiceContract]
    public interface IMeasurementMapContract : IService
    {
        [OperationContract]
        List<long> GetMeasurementsOfElement(long elementId);

        [OperationContract]
        Dictionary<long, long> GetMeasurementToElementMap();

        [OperationContract]
        Dictionary<long, List<long>> GetElementToMeasurementMap();
    }
}
