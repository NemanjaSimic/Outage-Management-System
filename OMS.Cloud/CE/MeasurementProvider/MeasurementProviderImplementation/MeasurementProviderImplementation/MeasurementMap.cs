using Common.CeContracts;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.CE;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CE.MeasurementProviderImplementation
{
    public class MeasurementMap : IMeasurementMapContract
	{
        private readonly string baseLogString;

        private readonly IMeasurementProviderContract measurementProviderClient;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public MeasurementMap()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            measurementProviderClient = MeasurementProviderClient.CreateClient();

            string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
            Logger.LogDebug(debugMessage);
        }

        public async Task<Dictionary<long, List<long>>> GetElementToMeasurementMap()
        {
            string verboseMessage = $"{baseLogString} entering GetElementToMeasurementMap method.";
            Logger.LogVerbose(verboseMessage);

            try
            {
                return await measurementProviderClient.GetElementToMeasurementMap();
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} GetElementToMeasurementMap => " +
                    $"Failed to get element to measurement map from measurement provider client." +
                    $"{Environment.NewLine} Exception message: {e.Message}";
                Logger.LogError(message);
                throw;
            }
        }

        public async Task<List<long>> GetMeasurementsOfElement(long elementId)
        {
            string verboseMessage = $"{baseLogString} entering GetMeasurementsOfElement method.";
            Logger.LogVerbose(verboseMessage);

            try
            {
                return await measurementProviderClient.GetMeasurementsOfElement(elementId);
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} GetMeasurementsOfElement => " +
                   $"Failed to get measurements of element with GID {elementId:X16} from measurement provider client." +
                   $"{Environment.NewLine} Exception message: {e.Message}";
                Logger.LogError(message);
                throw;
            }
        }

        public async Task<Dictionary<long, long>> GetMeasurementToElementMap()
        {
            string verboseMessage = $"{baseLogString} entering GetMeasurementToElementMap method.";
            Logger.LogVerbose(verboseMessage);

            try
            {
                return await measurementProviderClient.GetMeasurementToElementMap();
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} GetMeasurementToElementMap => " +
                  $"Failed to get measurement to element map from measurement provider client." +
                  $"{Environment.NewLine} Exception message: {e.Message}";
                Logger.LogError(message);
                throw;
            }
        }
    }
}
