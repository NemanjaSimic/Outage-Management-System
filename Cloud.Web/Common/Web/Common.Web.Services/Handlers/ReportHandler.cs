namespace OMS.Web.Services.Handlers
{
    using MediatR;
    using OMS.Web.Services.Commands;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class ReportHandler : IRequestHandler<GenerateReport>
    {
        public Task<Unit> Handle(GenerateReport request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
