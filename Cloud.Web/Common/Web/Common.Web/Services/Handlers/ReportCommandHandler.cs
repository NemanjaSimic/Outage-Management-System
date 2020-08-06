using Common.Web.Services.Commands;
using Common.Web.Models;
using Common.Web.Models.BindingModels;
using Common.Web.Models.ViewModels;
using MediatR;
using OMS.Common.Cloud.Names;
using System;
using System.Threading;
using System.Threading.Tasks;
using ILogger = OMS.Common.Cloud.Logger.ICloudLogger;
using Common.Contracts.WebAdapterContracts;

namespace Common.Web.Services.Handlers
{
    public class ReportCommandHandler : IRequestHandler<GenerateReportCommand, ReportViewModel>
    {
        private readonly ILogger _logger;

        public ReportCommandHandler(ILogger logger)
        {
            _logger = logger;
        }

        public Task<ReportViewModel> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                
                //using (OutageAccessProxy outageProxy = _proxyFactory.CreateProxy<OutageAccessProxy, IOutageAccessContract>(EndpointNames.OutageAccessEndpoint))
                //{
                    try
                    {
                        _logger.LogInformation("[ReportCommandHandler::GenerateReport] Sending a Generate command to Outage service.");

                        var options = new ReportOptions
                        {
                            Type = (ReportType)request.Options.Type,
                            ElementId = request.Options.ElementId,
                            StartDate = request.Options.StartDate,
                            EndDate = request.Options.EndDate
                        };

                        //var report = outageProxy.GenerateReport(options);

                        return new ReportViewModel
                        {
                            //Data = report.Data,
                            //Type = report.Type
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[ReportCommandHandler::GenerateReport] Failed to generate active outages from Outage service.", ex);
                        throw ex;
                    }
                //}


            }, cancellationToken);
        }
    }
}
