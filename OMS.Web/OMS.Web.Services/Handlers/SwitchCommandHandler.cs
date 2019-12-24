using MediatR;
using OMS.Web.Services.Commands;
using Outage.Common;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Web.Services.Handlers
{
    public class SwitchCommandHandler : IRequestHandler<TurnOffSwitchCommand>, IRequestHandler<TurnOnSwitchCommand>
    {
        private readonly ILogger _logger;

        public SwitchCommandHandler(ILogger logger)
        {
            _logger = logger;
        }

        public Task<Unit> Handle(TurnOffSwitchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Sending {request.Command.ToString()} command to {request.Gid}");
            // TODO: Implement logic for sending command

            return null;
        }

        public Task<Unit> Handle(TurnOnSwitchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Sending {request.Command.ToString()} command to {request.Gid}");
            // TODO: Implement logic for sending command

            return null;
        }
    }
}
