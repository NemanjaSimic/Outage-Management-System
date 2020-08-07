using Common.Web.Services.Commands;
using Common.Web.Models.ViewModels;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using OMS.Common.WcfClient.OMS;
using Common.OmsContracts.Report;
using ReportType = OMS.Common.Cloud.ReportType;
using OMS.Common.Cloud.Logger;

namespace Common.Web.Services.Handlers
{
    public class ReportCommandHandler : IRequestHandler<GenerateReportCommand, ReportViewModel>
    {
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public async Task<ReportViewModel> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
        {
            IReportingContract reportingClient = ReportingClient.CreateClient();
            try
            {
                Logger.LogInformation("[ReportCommandHandler::GenerateReport] Sending a Generate command to Outage service.");

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
                Logger.LogError("[ReportCommandHandler::GenerateReport] Failed to generate active outages from Outage service.", ex);
                throw ex;
            }

        }
    }
}
