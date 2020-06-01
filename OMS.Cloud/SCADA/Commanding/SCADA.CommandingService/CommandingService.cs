using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.ScadaContracts.Commanding;
using Outage.Common;
using SCADA.CommandingImplementation;

namespace SCADA.CommandingService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CommandingService : StatelessService
    {
        public CommandingService(StatelessServiceContext context)
            : base(context)
        { }

        protected async override Task OnOpenAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ////FOR DEBUGING IN AZURE DEPLOYMENT (time to atach to process)
            //await Task.Delay(60000);

            await base.OnOpenAsync(cancellationToken);
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
                //ScadaReadCommandEnqueuerEndpoint
                new ServiceInstanceListener(context =>
                {
                    return new WcfCommunicationListener<IScadaCommandingContract>(context,
                                                                                  new CommandingProvider(),
                                                                                  WcfUtility.CreateTcpListenerBinding(),
                                                                                  EndpointNames.SCADACommandService);
                }, EndpointNames.SCADACommandService),
            };
        }
    }
}
