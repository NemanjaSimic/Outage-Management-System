using Outage.Common.ServiceContracts.CalculationEngine;
using System;
using System.Collections.Generic;

namespace Outage.Common.ServiceProxies.CalcualtionEngine
{
    public class MeasurementMapProxy : BaseProxy<IMeasurementMapContract>, IMeasurementMapContract
    {
        public MeasurementMapProxy(string endpoint) 
            : base(endpoint)
        {
        }

        public Dictionary<long, List<long>> GetElementToMeasurementMap()
        {
            try
            {
                return Channel.GetElementToMeasurementMap();
            }
            catch (Exception ex)
            {
                LoggerWrapper.Instance.LogError($"Failed to get element to measurement map.", ex);
                throw;
            }
        }

        public List<long> GetMeasurementsOfElement(long elementId)
        {
            try
            {
                return Channel.GetMeasurementsOfElement(elementId);
            }
            catch (Exception ex)
            {
                LoggerWrapper.Instance.LogError($"Failed to get measurements fot element with GID 0x{elementId.ToString("X16")}. Exception message: {ex.Message}");
                throw;
            }
        }

        public Dictionary<long, long> GetMeasurementToElementMap()
        {
            try
            {
                return Channel.GetMeasurementToElementMap();
            }
            catch (Exception ex)
            {
                LoggerWrapper.Instance.LogError($"Failed to get measurement to element map.", ex);
                throw;
            }
        }
    }
}
