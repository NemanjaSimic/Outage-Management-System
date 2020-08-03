using Common.Contracts.WebAdapterContracts;
using Common.Web.Services.Commands;
using MediatR;
using OMS.Common.Cloud.Names;
using System;
using System.Threading;
using System.Threading.Tasks;
using ILogger = OMS.Common.Cloud.Logger.ICloudLogger;

namespace Common.Web.Services.Handlers
{
    public class SwitchCommandHandler : IRequestHandler<OpenSwitchCommand>, IRequestHandler<CloseSwitchCommand>
    {
        private readonly ILogger _logger;
        private IProxyFactory _proxyFactory;

        public SwitchCommandHandler(ILogger logger, IProxyFactory factory)
        {
            _logger = logger;
            _proxyFactory = factory;
        }

        public Task<Unit> Handle(OpenSwitchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[SwitchCommandHandler::TurnOffSwitchCommand] Sending {request.Command.ToString()} command to {request.Gid}");

            using (SwitchStatusCommandingProxy commandingProxy = _proxyFactory.CreateProxy<SwitchStatusCommandingProxy, ISwitchStatusCommandingContract>(EndpointNames.SwitchStatusCommandingEndpoint))
            {
                try
                {
                    commandingProxy.SendOpenCommand(request.Gid);
                }
                catch (Exception ex)
                {
                    _logger.LogError("[SwitchCommandHandler::TurnOffSwitchCommand] SwitchCommandHandler failed on TurnOffSwitch handler.", ex);
                    throw;
                }
            }

            return null;
        }

        public Task<Unit> Handle(CloseSwitchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"[SwitchCommandHandler::TurnOnSwitchCommand] Sending {request.Command.ToString()} command to {request.Gid}");


            using (SwitchStatusCommandingProxy commandingProxy = _proxyFactory.CreateProxy<SwitchStatusCommandingProxy, ISwitchStatusCommandingContract>(EndpointNames.SwitchStatusCommandingEndpoint))
            {
                try
                {
                    //commandingProxy.SendSwitchCommand(request.Gid, (int)request.Command);
                    commandingProxy.SendCloseCommand(request.Gid);
                }
                catch (Exception ex)
                {
                    _logger.LogError("[SwitchCommandHandler::TurnOnSwitchCommand] Failed on TurnOnSwitch handler.", ex);
                    throw;
                }
            }

            return null;
        }
    }
}
