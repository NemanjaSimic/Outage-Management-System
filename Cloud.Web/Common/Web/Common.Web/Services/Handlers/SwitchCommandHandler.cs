using Common.Web.Services.Commands;
using MediatR;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.CE;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Web.Services.Handlers
{
    public class SwitchCommandHandler : IRequestHandler<OpenSwitchCommand>, IRequestHandler<CloseSwitchCommand>
    {
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public async Task<Unit> Handle(OpenSwitchCommand request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"[SwitchCommandHandler::TurnOffSwitchCommand] Sending {request.Command.ToString()} command to {request.Gid}");

            CeContracts.ISwitchStatusCommandingContract switchStatusCommandingClient = SwitchStatusCommandingClient.CreateClient();
            try
            {
                await switchStatusCommandingClient.SendOpenCommand(request.Gid);
            }
            catch (Exception ex)
            {
                Logger.LogError("[SwitchCommandHandler::TurnOffSwitchCommand] SwitchCommandHandler failed on TurnOffSwitch handler.", ex);
                throw;
            }

            //return null;
            return new Unit();
        }

        public async Task<Unit> Handle(CloseSwitchCommand request, CancellationToken cancellationToken)
        {
            Logger.LogDebug($"[SwitchCommandHandler::TurnOnSwitchCommand] Sending {request.Command.ToString()} command to {request.Gid}");

            CeContracts.ISwitchStatusCommandingContract switchStatusCommandingClient = SwitchStatusCommandingClient.CreateClient();

            try
            {
                //commandingProxy.SendSwitchCommand(request.Gid, (int)request.Command);
                await switchStatusCommandingClient.SendCloseCommand(request.Gid);
            }
            catch (Exception ex)
            {
                Logger.LogError("[SwitchCommandHandler::TurnOnSwitchCommand] Failed on TurnOnSwitch handler.", ex);
                throw;
            }

            //return null;
            return new Unit();
        }
    }
}
