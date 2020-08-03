using Common.Web.UI.Models.BindingModels;
using Common.Web.UI.Models.ViewModels;
using MediatR;

namespace Common.Web.Services.Commands
{
    public class GenerateReportCommand : IRequest<ReportViewModel>
    {
        protected ReportOptions _options;

        public ReportOptions Options
        {
            get => _options;
            set => _options = value;
        }

        public GenerateReportCommand(ReportOptions options)
            => _options = options;
    }
}
