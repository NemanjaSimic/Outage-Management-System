using Common.Web.Services.Commands;
using Common.Web.Models.ViewModels;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ILogger = OMS.Common.Cloud.Logger.ICloudLogger;
using OMS.Common.WcfClient.OMS;
using Common.OmsContracts.Report;
using ReportType = OMS.Common.Cloud.ReportType;

namespace Common.Web.Services.Handlers
{
    public class ReportCommandHandler : IRequestHandler<GenerateReportCommand, ReportViewModel>
    {
        private readonly ILogger _logger;

        public ReportCommandHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<ReportViewModel> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
        {
            IReportingContract reportingClient = ReportingClient.CreateClient();
            try
            {
                _logger.LogInformation("[ReportCommandHandler::GenerateReport] Sending a Generate command to Outage service.");

                var options = new OMS.Report.ReportOptions
                {
                    Type = (ReportType)request.Options.Type,
                    ElementId = request.Options.ElementId,
                    StartDate = request.Options.StartDate,
                    EndDate = request.Options.EndDate
                };

                var report = await reportingClient.GenerateReport(options);

                return new ReportViewModel
                {
                    Data = report.Data,
                    Type = report.Type
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("[ReportCommandHandler::GenerateReport] Failed to generate active outages from Outage service.", ex);
                throw ex;
            }

        }
    }
}
