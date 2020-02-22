using CECommon.Providers;
using Outage.Common.ServiceContracts.CalculationEngine;
using System;
using System.Collections.Generic;
using Logger = Outage.Common.LoggerWrapper;

namespace CalculationEngineService
{
    public class MeasurementMapService : IMeasurementMapContract
    {
        public Dictionary<long, List<long>> GetElementToMeasurementMap()
        {
            try
            {
                return Provider.Instance.CacheProvider.GetElementToMeasurementMap();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"Failed to get element to measurement map.", ex);
                throw;
            }
        }

        public List<long> GetMeasurementsForElement(long elementId)
        {
            try
            {
                return Provider.Instance.MeasurementProvider.GetMeasurementsForElement(elementId);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"Failed to return measurements for element with GID 0x{elementId.ToString("X16")}. Exception message: {ex.Message}. \n Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public Dictionary<long, long> GetMeasurementToElementMap()
        {
            try
            {
                return Provider.Instance.CacheProvider.GetMeasurementToElementMap();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"Failed to get measurement to element map.", ex);
                throw;
            }
        }
    }
}
