using Outage.Common.ServiceContracts.CalculationEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies.CalcualtionEngine
{
    public class MeasurementMapProxy : BaseProxy<IMeasurementMapContract>, IMeasurementMapContract
    {
        public MeasurementMapProxy(string endpoint) 
            : base(endpoint)
        {

        }
        public List<Tuple<string, long>> GetMeasurementsForElement(long elementId)
        {
            try
            {
                return Channel.GetMeasurementsForElement(elementId);
            }
            catch (Exception ex)
            {
                LoggerWrapper.Instance.LogError($"Failed to get measurements fot element with GID 0x{elementId.ToString("X16")}. Exception message: {ex.Message}");
                throw;
            }
        }
    }
}
