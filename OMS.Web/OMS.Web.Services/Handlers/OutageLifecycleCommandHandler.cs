namespace OMS.Web.Services.Handlers
{
    using MediatR;
    using OMS.Web.Services.Commands;
    using Outage.Common;
    using Outage.Common.ServiceContracts.OMS;
    using Outage.Common.ServiceProxies;
    using Outage.Common.ServiceProxies.Outage;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class OutageLifecycleCommandHandler
        : IRequestHandler<IsolateOutageCommand>,
          IRequestHandler<SendOutageLocationIsolationCrewCommand>,
          IRequestHandler<SendOutageRepairCrewCommand>,
          IRequestHandler<ValidateResolveConditionsCommand>,
          IRequestHandler<ResolveOutageCommand>
    {

        private readonly ILogger _logger;
        private readonly IProxyFactory _proxyFactory;

        public OutageLifecycleCommandHandler(ILogger logger, IProxyFactory proxyFactory)
        {
            _logger = logger;
            _proxyFactory = proxyFactory;
        }

        public Task<Unit> Handle(IsolateOutageCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Unit>(() =>
            {
                _logger.LogInfo($"[OutageLifecycleCommandHandler::IsolateOutageCommand] Sending isolate outage command for outage: {request.OutageId}");

                using (OutageLifecycleUICommandingProxy commandingProxy =
                    _proxyFactory.CreateProxy<OutageLifecycleUICommandingProxy, IOutageLifecycleUICommandingContract>(
                        EndpointNames.OutageLifecycleUICommandingEndpoint
                        )
                    )
                {
                    try
                    {
                        commandingProxy.IsolateOutage(request.OutageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageLifecycleCommandHandler::IsolateOutageCommand] OutageLifecycleCommandHandler failed on IsolateOutage handler.", ex);
                        throw;
                    }
                }

                return null;
            });
        }

        public Task<Unit> Handle(SendOutageLocationIsolationCrewCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Unit>(() =>
            {
                _logger.LogInfo($"[OutageLifecycleCommandHandler::SendOutageLocationIsolationCrewCommand] Sending outage location isolation crew command for outage: {request.OutageId}");


                using (OutageLifecycleUICommandingProxy commandingProxy =
                    _proxyFactory.CreateProxy<OutageLifecycleUICommandingProxy, IOutageLifecycleUICommandingContract>(
                        EndpointNames.OutageLifecycleUICommandingEndpoint)
                    )
                {
                    try
                    {
                        commandingProxy.SendLocationIsolationCrew(request.OutageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageLifecycleCommandHandler::SendOutageLocationIsolationCrewCommand] OutageLifecycleCommandHandler failed on SendOutageLocationIsolationCrew handler.", ex);
                        throw;
                    }
                }

                return null;
            });
        }

        public Task<Unit> Handle(SendOutageRepairCrewCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Unit>(() =>
            {
                _logger.LogInfo($"[OutageLifecycleCommandHandler::SendOutageRepairCrewCommand] Sending outage repair crew command for outage: {request.OutageId}");


                using (OutageLifecycleUICommandingProxy commandingProxy = _proxyFactory.CreateProxy<OutageLifecycleUICommandingProxy, IOutageLifecycleUICommandingContract>(EndpointNames.OutageLifecycleUICommandingEndpoint))
                {
                    try
                    {
                        commandingProxy.SendRepairCrew(request.OutageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageLifecycleCommandHandler::SendOutageRepairCrewCommand] OutageLifecycleCommandHandler failed on SendOutageRepairCrew handler.", ex);
                        throw;
                    }
                }

                return null;
            });
        }

        public Task<Unit> Handle(ValidateResolveConditionsCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Unit>(() => 
            { 
                _logger.LogInfo($"[OutageLifecycleCommandHandler::ValidateResolveConditionsCommand] Sending validate resolve conditions command for outage: {request.OutageId}");


                using (OutageLifecycleUICommandingProxy commandingProxy =
                    _proxyFactory.CreateProxy<OutageLifecycleUICommandingProxy, IOutageLifecycleUICommandingContract>(
                        EndpointNames.OutageLifecycleUICommandingEndpoint)
                    )
                {
                    try
                    {
                        commandingProxy.ValidateResolveConditions(request.OutageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageLifecycleCommandHandler::ValidateResolveConditionsCommand] OutageLifecycleCommandHandler failed on ValidateResolveConditions handler.", ex);
                        throw;
                    }
                }

                return null;
            });
        }

        public Task<Unit> Handle(ResolveOutageCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Unit>(() => 
            { 
                _logger.LogInfo($"[OutageLifecycleCommandHandler::IsolateOutageCommand] Sending isolate outage command for outage: {request.OutageId}");


                using (OutageLifecycleUICommandingProxy commandingProxy =
                    _proxyFactory.CreateProxy<OutageLifecycleUICommandingProxy, IOutageLifecycleUICommandingContract>(
                        EndpointNames.OutageLifecycleUICommandingEndpoint)
                    )
                {
                    try
                    {
                        commandingProxy.IsolateOutage(request.OutageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageLifecycleCommandHandler::IsolateOutageCommand] OutageLifecycleCommandHandler failed on IsolateOutage handler.", ex);
                        throw;
                    }
                }

                return null;
            });
        }
    }
}
