namespace OMS.Web.Services.Commands
{
    using MediatR;
    using OMS.Web.UI.Models.BindingModels;
    using OMS.Web.UI.Models.ViewModels;

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
