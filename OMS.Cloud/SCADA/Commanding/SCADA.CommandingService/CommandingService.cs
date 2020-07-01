using System.Collections.Generic;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.ScadaContracts.Commanding;
using SCADA.CommandingImplementation;

namespace SCADA.CommandingService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CommandingService : StatelessService
    {
        private readonly string baseLogString;
        private readonly CommandingProvider commandingProvider;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public CommandingService(StatelessServiceContext context)
            : base(context)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            this.commandingProvider = new CommandingProvider();

            string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
            Logger.LogInformation(infoMessage);
            ServiceEventSource.Current.ServiceMessage(this.Context, $"[CommandingService | Information] {infoMessage}");
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            //return new ServiceInstanceListener[0];
            return new List<ServiceInstanceListener>()
            {
                //ScadaCommandService
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IScadaCommandingContract>(context,
                                                                                  this.commandingProvider,
                                                                                  WcfUtility.CreateTcpListenerBinding(),
                                                                                  EndpointNames.ScadaCommandService);
                }, EndpointNames.ScadaCommandService),
            };
        }
    }
}
