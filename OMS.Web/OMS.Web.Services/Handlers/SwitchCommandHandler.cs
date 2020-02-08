namespace OMS.Web.Services.Handlers
{
    using MediatR;
    using OMS.Web.Adapter.Contracts;
    using OMS.Web.Adapter.Topology;
    using OMS.Web.Common;
    using OMS.Web.Services.Commands;
    using Outage.Common;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class SwitchCommandHandler : IRequestHandler<TurnOffSwitchCommand>, IRequestHandler<TurnOnSwitchCommand>
    {
        private readonly ILogger _logger;
        private readonly IScadaClient _scadaClient;

        public SwitchCommandHandler(ILogger logger)
        {
            _logger = logger;
            string scadaCommandServiceAddress = AppSettings.Get<string>(ServiceAddress.ScadaCommandServiceAddress);
            _scadaClient = new TopologySCADACommandProxy(scadaCommandServiceAddress);
        }

        public Task<Unit> Handle(TurnOffSwitchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInfo($"[SwitchCommandHandler::TurnOffSwitchCommand] Sending {request.Command.ToString()} command to {request.Gid}");

            try
            {
                _scadaClient.SendCommand(request.Gid, (int)request.Command);
            }
            catch (Exception ex)
            {
                _logger.LogError("[SwitchCommandHandler::TurnOffSwitchCommand] SwitchCommandHandler failed on TurnOffSwitch handler.", ex);
                throw;
            }

            return null;
        }

        public Task<Unit> Handle(TurnOnSwitchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"[SwitchCommandHandler::TurnOnSwitchCommand] Sending {request.Command.ToString()} command to {request.Gid}");

            try
            {
                _scadaClient.SendCommand(request.Gid, (int)request.Command);
            }
            catch (Exception ex)
            {
                _logger.LogError("[SwitchCommandHandler::TurnOnSwitchCommand] Failed on TurnOnSwitch handler.", ex);
                throw;
            }

            return null;
        }
    }
}
