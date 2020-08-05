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
    public class OutageLifecycleCommandHandler
        : IRequestHandler<IsolateOutageCommand>,
          IRequestHandler<SendOutageLocationIsolationCrewCommand>,
          IRequestHandler<SendOutageRepairCrewCommand>,
          IRequestHandler<ValidateResolveConditionsCommand>,
          IRequestHandler<ResolveOutageCommand>
    {

        private readonly ILogger _logger;

        public OutageLifecycleCommandHandler(ILogger logger)
        {
            _logger = logger;
        }

        public Task<Unit> Handle(IsolateOutageCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Unit>(() =>
            {
                _logger.LogInformation($"[OutageLifecycleCommandHandler::IsolateOutageCommand] Sending isolate outage command for outage: {request.OutageId}");

                //using (OutageLifecycleUICommandingProxy commandingProxy = _proxyFactory.CreateProxy<OutageLifecycleUICommandingProxy, IOutageLifecycleUICommandingContract>(EndpointNames.OutageLifecycleUICommandingEndpoint))
                {
                    try
                    {
                        //commandingProxy.IsolateOutage(request.OutageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageLifecycleCommandHandler::IsolateOutageCommand] OutageLifecycleCommandHandler failed on IsolateOutage handler.", ex);
                        throw;
                    }
                }

                return Task.FromResult(new Unit());
            }, cancellationToken);
        }

        public Task<Unit> Handle(SendOutageLocationIsolationCrewCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Unit>(() =>
            {
                _logger.LogInformation($"[OutageLifecycleCommandHandler::SendOutageLocationIsolationCrewCommand] Sending outage location isolation crew command for outage: {request.OutageId}");


                //using (OutageLifecycleUICommandingProxy commandingProxy = _proxyFactory.CreateProxy<OutageLifecycleUICommandingProxy, IOutageLifecycleUICommandingContract>(EndpointNames.OutageLifecycleUICommandingEndpoint))
                {
                    try
                    {
                        //commandingProxy.SendLocationIsolationCrew(request.OutageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageLifecycleCommandHandler::SendOutageLocationIsolationCrewCommand] OutageLifecycleCommandHandler failed on SendOutageLocationIsolationCrew handler.", ex);
                        throw;
                    }
                }

                return Task.FromResult(new Unit());
            }, cancellationToken);
        }

        public Task<Unit> Handle(SendOutageRepairCrewCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Unit>(() =>
            {
                _logger.LogInformation($"[OutageLifecycleCommandHandler::SendOutageRepairCrewCommand] Sending outage repair crew command for outage: {request.OutageId}");


                //using (OutageLifecycleUICommandingProxy commandingProxy = _proxyFactory.CreateProxy<OutageLifecycleUICommandingProxy, IOutageLifecycleUICommandingContract>(EndpointNames.OutageLifecycleUICommandingEndpoint))
                {
                    try
                    {
                        //commandingProxy.SendRepairCrew(request.OutageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageLifecycleCommandHandler::SendOutageRepairCrewCommand] OutageLifecycleCommandHandler failed on SendOutageRepairCrew handler.", ex);
                        throw;
                    }
                }

                return Task.FromResult(new Unit());
            }, cancellationToken);
        }

        public Task<Unit> Handle(ValidateResolveConditionsCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Unit>(() =>
            {
                _logger.LogInformation($"[OutageLifecycleCommandHandler::ValidateResolveConditionsCommand] Sending validate resolve conditions command for outage: {request.OutageId}");


                //using (OutageLifecycleUICommandingProxy commandingProxy = _proxyFactory.CreateProxy<OutageLifecycleUICommandingProxy, IOutageLifecycleUICommandingContract>(EndpointNames.OutageLifecycleUICommandingEndpoint))
                {
                    try
                    {
                        //commandingProxy.ValidateResolveConditions(request.OutageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageLifecycleCommandHandler::ValidateResolveConditionsCommand] OutageLifecycleCommandHandler failed on ValidateResolveConditions handler.", ex);
                        throw;
                    }
                }

                return Task.FromResult(new Unit());
            }, cancellationToken);
        }

        public Task<Unit> Handle(ResolveOutageCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Unit>(() =>
            {
                _logger.LogInformation($"[OutageLifecycleCommandHandler::IsolateOutageCommand] Sending isolate outage command for outage: {request.OutageId}");


                //using (OutageLifecycleUICommandingProxy commandingProxy = _proxyFactory.CreateProxy<OutageLifecycleUICommandingProxy, IOutageLifecycleUICommandingContract>(EndpointNames.OutageLifecycleUICommandingEndpoint))
                {
                    try
                    {
                        //commandingProxy.ResolveOutage(request.OutageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageLifecycleCommandHandler::IsolateOutageCommand] OutageLifecycleCommandHandler failed on IsolateOutage handler.", ex);
                        throw;
                    }
                }

                return Task.FromResult(new Unit());
            }, cancellationToken);
        }
    }
}
