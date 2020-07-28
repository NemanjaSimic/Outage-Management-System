using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.ModelProvider;
using OMS.Common.WcfClient.SCADA;
using SCADA.AcquisitionImplementation;

namespace SCADA.AcquisitionService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class AcquisitionService : StatelessService
    {
        private readonly string baseLogString;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public AcquisitionService(StatelessServiceContext context)
            : base(context)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");
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
                acquisitionCycle = new AcquisitionCycle();
                IScadaModelReadAccessContract readAccessClient = ScadaModelReadAccessClient.CreateClient();
                configData = await readAccessClient.GetScadaConfigData();

                string message = $"{baseLogString} RunAsync => AcquisitionCycle initialized.";
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[AcquisitionService | Information] {message}");
                Logger.LogInformation(message);
            }
            catch (Exception e)
            {
                string errMessage = $"{baseLogString} RunAsync => Exception caught: {e.Message}.";
                Logger.LogError(errMessage, e);
                ServiceEventSource.Current.ServiceMessage(this.Context, $"[AcquisitionService | Error] {errMessage}");
                throw e;
            }

            int acquisitionCycleCount = 1;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string message = $"{baseLogString} RunAsync => AcquisitionCycleCount: {acquisitionCycleCount}";

                if(acquisitionCycleCount % 100 == 0)
                {
                    Logger.LogInformation(message);
                }
                else if(acquisitionCycleCount % 10 == 0)
                {
                    Logger.LogDebug(message);
                }
                else
                {
                    Logger.LogVerbose(message);
                }

                try
                {
                    await acquisitionCycle.Start();

                    message = $"{baseLogString} RunAsync => AcquisitionCycle executed.";
                    Logger.LogVerbose(message);
                    //ServiceEventSource.Current.ServiceMessage(this.Context, $"[AcquisitionService | Verbose] {message}");
                }
                catch (Exception e)
                {
                    string errMessage = $"{baseLogString} RunAsync => Exception caught: {e.Message}";
                    Logger.LogError(errMessage, e);
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"[AcquisitionService | Error] {errMessage}");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(configData.AcquisitionInterval), cancellationToken);
                acquisitionCycleCount++;
            }
        }
    }
}
