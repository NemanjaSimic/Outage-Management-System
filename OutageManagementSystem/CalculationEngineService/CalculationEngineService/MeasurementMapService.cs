using CECommon.Providers;
using Outage.Common.ServiceContracts.CalculationEngine;
using System;
using System.Collections.Generic;
using Logger = Outage.Common.LoggerWrapper;

namespace CalculationEngineService
{
    public class MeasurementMapService : IMeasurementMapContract
    {
        public List<Tuple<string, long>> GetMeasurementsForElement(long elementId)
        {
            try
            {
                return Provider.Instance.CacheProvider.GetMeasurementsForElement(elementId);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"Failed to return measurements for element with GID 0x{elementId.ToString("X16")}. Exception message: {ex.Message}. \n Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
