using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using OMS.Web.Adapter.Contracts;
using OMS.Web.Services.Commands;
using Outage.Common;
using OMS.Web.Adapter.Topology;
using OMS.Web.Common;

namespace OMS.Web.Services.Handlers
{
    public class SwitchCommandHandler : IRequestHandler<TurnOffSwitchCommand>, IRequestHandler<TurnOnSwitchCommand>
    {
        private readonly ILogger _logger;
        private readonly IScadaClient _scadaClient;

        public SwitchCommandHandler(ILogger logger)
        {
            _logger = logger;
            string scadaCommandServiceAddress = AppSettings.Get<string>("scadaCommandServiceAddress");
            _scadaClient = new TopologySCADACommandProxy(scadaCommandServiceAddress);
        }

        public Task<Unit> Handle(TurnOffSwitchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Sending {request.Command.ToString()} command to {request.Gid}");
            
            try
            {
                // treba nam implementacija scada klijenta
                // jer sad treba da bude preko CE
                _scadaClient.SendCommand(request.Gid, (int)request.Command);
            }
            catch(Exception e)
            {
                _logger.LogError("SwitchCommandHandler failed on TurnOffSwitch handler.", e);
            }
            
            return null; // vracanje null vrednosti je anti-pattern ali ovde nemam drugog izbora
        }

        public Task<Unit> Handle(TurnOnSwitchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Sending {request.Command.ToString()} command to {request.Gid}");

            try
            {
                _scadaClient.SendCommand(request.Gid, (int)request.Command);
            }
            catch (Exception e)
            {
                _logger.LogDebug("SwitchCommandHandler failed on TurnOnSwitch handler.");
                _logger.LogError(null, e);
                throw;
            }

            return null;
        }
    }
}
