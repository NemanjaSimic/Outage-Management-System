using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;

namespace SCADA.AcquisitionService
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            string baseLoggString = $"{typeof(Program)} [static] =>";
            ICloudLogger logger = CloudLoggerFactory.GetLogger();
            
            try
            {

                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                logger.LogDebug($"{baseLoggString} Main => Calling RegisterServiceAsync for type 'SCADA.AcquisitionServiceType'.");

                ServiceRuntime.RegisterServiceAsync("SCADA.AcquisitionServiceType",
                    context => new AcquisitionService(context)).GetAwaiter().GetResult();

                logger.LogInformation($"{baseLoggString} Main => 'SCADA.AcquisitionServiceType' type registered.");
                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(AcquisitionService).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                logger.LogError($"{baseLoggString} Main => Exception: {e.Message}.", e);
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
