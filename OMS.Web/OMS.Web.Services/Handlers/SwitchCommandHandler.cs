namespace OMS.Web.Services.Handlers
{
    using MediatR;
    using OMS.Web.Services.Commands;
    using Outage.Common;
    using Outage.Common.ServiceContracts.CalculationEngine;
    using Outage.Common.ServiceProxies;
    using Outage.Common.ServiceProxies.Commanding;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class SwitchCommandHandler : IRequestHandler<OpenSwitchCommand>, IRequestHandler<CloseSwitchCommand>
    {
        private readonly ILogger _logger;
        private ProxyFactory _proxyFactory;

        public SwitchCommandHandler(ILogger logger)
        {
            _logger = logger;
            _proxyFactory = new ProxyFactory();
        }

        public Task<Unit> Handle(OpenSwitchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInfo($"[SwitchCommandHandler::TurnOffSwitchCommand] Sending {request.Command.ToString()} command to {request.Gid}");


            using (SwitchStatusCommandingProxy commandingProxy = _proxyFactory.CreateProxy<SwitchStatusCommandingProxy, ISwitchStatusCommandingContract>(EndpointNames.SwitchStatusCommandingEndpoint))
            {
                try
                {
                    //commandingProxy.SendSwitchCommand(request.Gid, (int)request.Command);
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
