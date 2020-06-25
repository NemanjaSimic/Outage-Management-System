using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.SCADA;
using OMS.Common.WcfClient.SCADA;
using SCADA.AcquisitionImplementation;

namespace SCADA.AcquisitionService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class AcquisitionService : StatelessService
    {
        private readonly string baseLoggString;
        private readonly ICloudLogger logger;

        public AcquisitionService(StatelessServiceContext context)
            : base(context)
        {
            this.baseLoggString = $"{typeof(AcquisitionService)} [{this.GetHashCode()}] =>";

            logger = CloudLoggerFactory.GetLogger();
            logger.LogDebug($"{baseLoggString} Ctor => Logger initialized");
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            AcquisitionCycle acquisitionCycle;
            IScadaConfigData configData;

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                acquisitionCycle = new AcquisitionCycle(this.Context);
                ScadaModelReadAccessClient readAccessClient = ScadaModelReadAccessClient.CreateClient();
                configData = await readAccessClient.GetScadaConfigData();

                string message = $"{baseLoggString} RunAsync => AcquisitionCycle initialized.";
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[AcquisitionService | Information] {message}");
                logger.LogInformation(message);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[AcquisitionService | Error] {e.Message}");
                throw e;
            }

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await acquisitionCycle.Start();

                    string message = $"{baseLoggString} RunAsync => AcquisitionCycle executed.";
                    logger.LogVerbose(message);
                    //ServiceEventSource.Current.ServiceMessage(this.Context, $"[AcquisitionService | Verbose] {message}");
                }
                catch (Exception e)
                {
                    logger.LogError($"{baseLoggString} RunAsync => {e.Message}", e);
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"[AcquisitionService | Error] {e.Message}");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(configData.AcquisitionInterval), cancellationToken);
            }
        }
    }
}
