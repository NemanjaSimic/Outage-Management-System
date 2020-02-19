using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.CalculationEngine
{
    [ServiceContract]
    public interface IMeasurementMapContract
    {
        [OperationContract]
        List<long> GetMeasurementsForElement(long elementId);

        [OperationContract]
        Dictionary<long, long> GetMeasurementToElementMap();

        [OperationContract]
        Dictionary<long, List<long>> GetElementToMeasurementMap();
    }
}
