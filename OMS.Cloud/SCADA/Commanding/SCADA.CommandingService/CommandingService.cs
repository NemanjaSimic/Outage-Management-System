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
        private readonly ICloudLogger logger;

        public CommandingService(StatelessServiceContext context)
            : base(context)
        {
            logger = CloudLoggerFactory.GetLogger();
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
                                                                                  new CommandingProvider(),
                                                                                  WcfUtility.CreateTcpListenerBinding(),
                                                                                  EndpointNames.ScadaCommandService);
                }, EndpointNames.ScadaCommandService),
            };
        }
    }
}
