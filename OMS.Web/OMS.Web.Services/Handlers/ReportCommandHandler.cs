namespace OMS.Web.Services.Handlers
{
    using MediatR;
    using OMS.Web.Services.Commands;
    using OMS.Web.UI.Models.ViewModels;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class ReportCommandHandler : IRequestHandler<GenerateReportCommand, ReportViewModel>
    {
        public Task<ReportViewModel> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                // @TODO:
                // - Add report logic here

                return new ReportViewModel
                {
                    Data = new Dictionary<string, int>
                    {
                        { "January", 2154 },
                        { "February", 1538 },
                        { "March", 1234 },
                        { "April", 756 },
                        { "May", 2621 }
                    },
                    Type = "Monthly"
                };
            }, cancellationToken);
        }
    }
}
