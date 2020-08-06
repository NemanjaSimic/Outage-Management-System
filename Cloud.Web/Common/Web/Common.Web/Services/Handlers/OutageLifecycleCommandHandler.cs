using Common.OmsContracts.OutageLifecycle;
using Common.Web.Services.Commands;
using MediatR;
using OMS.Common.WcfClient.OMS;
using OMS.Common.WcfClient.OMS.Lifecycle;
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

        public async Task<Unit> Handle(IsolateOutageCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[OutageLifecycleCommandHandler::IsolateOutageCommand] Sending isolate outage command for outage: {request.OutageId}");

            IIsolateOutageContract isolateOutageClient = IsolateOutageClient.CreateClient();
            try
            {
                await isolateOutageClient.IsolateOutage(request.OutageId);
            }
            catch (Exception ex)
            {
                _logger.LogError("[OutageLifecycleCommandHandler::IsolateOutageCommand] OutageLifecycleCommandHandler failed on IsolateOutage handler.", ex);
                throw;
            }

            return new Unit();
        }

        public async Task<Unit> Handle(SendOutageLocationIsolationCrewCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[OutageLifecycleCommandHandler::SendOutageLocationIsolationCrewCommand] Sending outage location isolation crew command for outage: {request.OutageId}");

            ISendLocationIsolationCrewContract sendLocationIsolationCrewClient = SendLocationIsolationCrewClient.CreateClient();
            try
            {
                await sendLocationIsolationCrewClient.SendLocationIsolationCrew(request.OutageId);
            }
            catch (Exception ex)
            {
                _logger.LogError("[OutageLifecycleCommandHandler::SendOutageLocationIsolationCrewCommand] OutageLifecycleCommandHandler failed on SendOutageLocationIsolationCrew handler.", ex);
                throw;
            }

            return new Unit();
        }

        public async Task<Unit> Handle(SendOutageRepairCrewCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[OutageLifecycleCommandHandler::SendOutageRepairCrewCommand] Sending outage repair crew command for outage: {request.OutageId}");

            ISendRepairCrewContract sendRepairCrewClient = SendRepairCrewClient.CreateClient();
            try
            {
                await sendRepairCrewClient.SendRepairCrew(request.OutageId);
            }
            catch (Exception ex)
            {
                _logger.LogError("[OutageLifecycleCommandHandler::SendOutageRepairCrewCommand] OutageLifecycleCommandHandler failed on SendOutageRepairCrew handler.", ex);
                throw;
            }

            return new Unit();
        }

        public async Task<Unit> Handle(ValidateResolveConditionsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[OutageLifecycleCommandHandler::ValidateResolveConditionsCommand] Sending validate resolve conditions command for outage: {request.OutageId}");

            IValidateResolveConditionsContract validateResolveConditionsClient = ValidateResolveConditionsClient.CreateClient();
            try
            {
                await validateResolveConditionsClient.ValidateResolveConditions(request.OutageId);
            }
            catch (Exception ex)
            {
                _logger.LogError("[OutageLifecycleCommandHandler::ValidateResolveConditionsCommand] OutageLifecycleCommandHandler failed on ValidateResolveConditions handler.", ex);
                throw;
            }

            return new Unit();
        }

        public async Task<Unit> Handle(ResolveOutageCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[OutageLifecycleCommandHandler::IsolateOutageCommand] Sending isolate outage command for outage: {request.OutageId}");

            IResolveOutageContract resolveOutageClient = ResolveOutageClient.CreateClient();
            try
            {
                await resolveOutageClient.ResolveOutage(request.OutageId);
            }
            catch (Exception ex)
            {
                _logger.LogError("[OutageLifecycleCommandHandler::IsolateOutageCommand] OutageLifecycleCommandHandler failed on IsolateOutage handler.", ex);
                throw;
            }

            return new Unit();
        }
    }
}
