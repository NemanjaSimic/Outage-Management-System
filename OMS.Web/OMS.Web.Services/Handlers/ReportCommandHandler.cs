namespace OMS.Web.Services.Handlers
{
    using MediatR;
    using OMS.Web.Services.Commands;
    using OMS.Web.UI.Models.ViewModels;
    using System.Threading;
    using System.Threading.Tasks;

    public class ReportCommandHandler : IRequestHandler<GenerateReportCommand, ReportViewModel>
    {
        public Task<ReportViewModel> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                // logic
                return new ReportViewModel { };
            }, cancellationToken);
        }
    }
}
