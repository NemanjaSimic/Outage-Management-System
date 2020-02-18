using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.CalculationEngine
{
    [ServiceContract]
    public interface IMeasurementMapContract
    {
        [OperationContract]
        List<Tuple<string, long>> GetMeasurementsForElement(long elementId);
    }
}
