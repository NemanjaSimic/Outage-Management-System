namespace OMS.Web.Services.Commands
{
    using MediatR;
    using OMS.Web.UI.Models.BindingModels;

    public class GenerateReport : IRequest
    {
        private readonly ReportOptions _options;

        public GenerateReport(ReportOptions options)
            => _options = options;
    }
}
