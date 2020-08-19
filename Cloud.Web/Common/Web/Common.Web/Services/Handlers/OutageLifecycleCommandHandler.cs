using Common.OmsContracts.OutageLifecycle;
using Common.Web.Services.Commands;
using MediatR;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.OMS;
using OMS.Common.WcfClient.OMS.Lifecycle;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Web.Services.Handlers
{
    public class OutageLifecycleCommandHandler
        : IRequestHandler<IsolateOutageCommand>,
          IRequestHandler<SendOutageLocationIsolationCrewCommand>,
          IRequestHandler<SendOutageRepairCrewCommand>,
          IRequestHandler<ValidateResolveConditionsCommand>,
          IRequestHandler<ResolveOutageCommand>
    {
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public async Task<Unit> Handle(IsolateOutageCommand request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"[OutageLifecycleCommandHandler::IsolateOutageCommand] Sending isolate outage command for outage: {request.OutageId}");

            try
            {
                var isolateOutageClient = IsolateOutageClient.CreateClient();
                await isolateOutageClient.IsolateOutage(request.OutageId);
            }
            catch (Exception ex)
            {
                Logger.LogError("[OutageLifecycleCommandHandler::IsolateOutageCommand] OutageLifecycleCommandHandler failed on IsolateOutage handler.", ex);
                throw;
            }

            return new Unit();
        }

        public async Task<Unit> Handle(SendOutageLocationIsolationCrewCommand request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"[OutageLifecycleCommandHandler::SendOutageLocationIsolationCrewCommand] Sending outage location isolation crew command for outage: {request.OutageId}");

            try
            {
                var sendLocationIsolationCrewClient = SendLocationIsolationCrewClient.CreateClient();
                await sendLocationIsolationCrewClient.SendLocationIsolationCrew(request.OutageId);
            }
            catch (Exception ex)
            {
                Logger.LogError("[OutageLifecycleCommandHandler::SendOutageLocationIsolationCrewCommand] OutageLifecycleCommandHandler failed on SendOutageLocationIsolationCrew handler.", ex);
                throw;
            }

            return new Unit();
        }

        public async Task<Unit> Handle(SendOutageRepairCrewCommand request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"[OutageLifecycleCommandHandler::SendOutageRepairCrewCommand] Sending outage repair crew command for outage: {request.OutageId}");

            try
            {
                var sendRepairCrewClient = SendRepairCrewClient.CreateClient();
                await sendRepairCrewClient.SendRepairCrew(request.OutageId);
            }
            catch (Exception ex)
            {
                Logger.LogError("[OutageLifecycleCommandHandler::SendOutageRepairCrewCommand] OutageLifecycleCommandHandler failed on SendOutageRepairCrew handler.", ex);
                throw;
            }

            return new Unit();
        }

        public async Task<Unit> Handle(ValidateResolveConditionsCommand request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"[OutageLifecycleCommandHandler::ValidateResolveConditionsCommand] Sending validate resolve conditions command for outage: {request.OutageId}");

            try
            {
                var validateResolveConditionsClient = ValidateResolveConditionsClient.CreateClient();
                await validateResolveConditionsClient.ValidateResolveConditions(request.OutageId);
            }
            catch (Exception ex)
            {
                Logger.LogError("[OutageLifecycleCommandHandler::ValidateResolveConditionsCommand] OutageLifecycleCommandHandler failed on ValidateResolveConditions handler.", ex);
                throw;
            }

            return new Unit();
        }

        public async Task<Unit> Handle(ResolveOutageCommand request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"[OutageLifecycleCommandHandler::IsolateOutageCommand] Sending isolate outage command for outage: {request.OutageId}");

            try
            {
                var resolveOutageClient = ResolveOutageClient.CreateClient();
                await resolveOutageClient.ResolveOutage(request.OutageId);
            }
            catch (Exception ex)
            {
                Logger.LogError("[OutageLifecycleCommandHandler::IsolateOutageCommand] OutageLifecycleCommandHandler failed on IsolateOutage handler.", ex);
                throw;
            }

            return new Unit();
        }
    }
}
