using System.Collections.Generic;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.WcfServiceFabricClients;
using OMS.Common.ScadaContracts;
using Outage.Common;

namespace OMS.Cloud.SCADA.CommandingService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CommandingService : StatelessService
    {
        public CommandingService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>()
            {
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IScadaCommandingContract>(context,
                                                                              new CommandingProvider(),
                                                                              TcpBindingHelper.CreateListenerBinding(),
                                                                              EndpointNames.SCADACommandService);
                }, EndpointNames.SCADACommandService)
            };
        }
    }
}
