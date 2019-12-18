using System;
using MediatR;
using Outage.Common;
using System.Threading;
using System.Threading.Tasks;
using OMS.Web.Adapter.Contracts;
using OMS.Web.Services.Commands;

namespace OMS.Web.Services.Handlers
{
    public class SwitchCommandHandler : IRequestHandler<TurnOffSwitchCommand>, IRequestHandler<TurnOnSwitchCommand>
    {
        private readonly ILogger _logger;
        private readonly IScadaClient _scadaClient;

        public SwitchCommandHandler(ILogger logger)
        {
            _logger = logger;
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
            
            return null;
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
