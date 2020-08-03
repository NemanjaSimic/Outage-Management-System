using Common.CeContracts;
using Common.Contracts.WebAdapterContracts;
using Common.Web.Services.Commands;
using MediatR;
using OMS.Common.Cloud.Names;
using OMS.Common.WcfClient.CE;
using System;
using System.Threading;
using System.Threading.Tasks;
using ILogger = OMS.Common.Cloud.Logger.ICloudLogger;

namespace Common.Web.Services.Handlers
{
    public class SwitchCommandHandler : IRequestHandler<OpenSwitchCommand>, IRequestHandler<CloseSwitchCommand>
    {
        private readonly ILogger _logger;

        public SwitchCommandHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<Unit> Handle(OpenSwitchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[SwitchCommandHandler::TurnOffSwitchCommand] Sending {request.Command.ToString()} command to {request.Gid}");

            CeContracts.ISwitchStatusCommandingContract switchStatusCommandingClient = SwitchStatusCommandingClient.CreateClient();
            try
            {
                await switchStatusCommandingClient.SendOpenCommand(request.Gid);
            }
            catch (Exception ex)
            {
                _logger.LogError("[SwitchCommandHandler::TurnOffSwitchCommand] SwitchCommandHandler failed on TurnOffSwitch handler.", ex);
                throw;
            }

            //return null;
            return new Unit();
        }

        public async Task<Unit> Handle(CloseSwitchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"[SwitchCommandHandler::TurnOnSwitchCommand] Sending {request.Command.ToString()} command to {request.Gid}");

            CeContracts.ISwitchStatusCommandingContract switchStatusCommandingClient = SwitchStatusCommandingClient.CreateClient();

            try
            {
                //commandingProxy.SendSwitchCommand(request.Gid, (int)request.Command);
                await switchStatusCommandingClient.SendCloseCommand(request.Gid);
            }
            catch (Exception ex)
            {
                _logger.LogError("[SwitchCommandHandler::TurnOnSwitchCommand] Failed on TurnOnSwitch handler.", ex);
                throw;
            }

            //return null;
            return new Unit();
        }
    }
}
