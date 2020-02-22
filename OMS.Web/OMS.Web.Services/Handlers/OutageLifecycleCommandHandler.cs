namespace OMS.Web.Services.Handlers
{
    using MediatR;
    using OMS.Web.Services.Commands;
    using Outage.Common;
    using Outage.Common.ServiceProxies;
    using System.Threading;
    using System.Threading.Tasks;

    public class OutageLifecycleCommandHandler
        : IRequestHandler<IsolateOutageCommand>,
          IRequestHandler<SendOutageCrewCommand>,
          IRequestHandler<ResolveOutageCommand>,
          IRequestHandler<ValidateOutageCommand>
    {

        private readonly ILogger _logger;
        private readonly IProxyFactory _proxyFactory;

        public OutageLifecycleCommandHandler(ILogger logger, IProxyFactory proxyFactory)
        {
            _logger = logger;
            _proxyFactory = proxyFactory;
        }

        // @TODO: 
        // Implementirati ove metode na osnovu OMS servisa koji ce se koristiti
        // Moramo se dogovoriti tacno oko contract-a

        public Task<Unit> Handle(IsolateOutageCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<Unit> Handle(SendOutageCrewCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<Unit> Handle(ResolveOutageCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<Unit> Handle(ValidateOutageCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
