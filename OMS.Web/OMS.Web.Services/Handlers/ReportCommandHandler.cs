namespace OMS.Web.Services.Handlers
{
    using MediatR;
    using OMS.Web.Services.Commands;
    using OMS.Web.UI.Models.ViewModels;
    using Outage.Common;
    using Outage.Common.ServiceContracts.OMS;
    using Outage.Common.ServiceProxies;
    using Outage.Common.ServiceProxies.Outage;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class ReportCommandHandler : IRequestHandler<GenerateReportCommand, ReportViewModel>
    {
        private readonly ILogger _logger;
        private readonly IProxyFactory _proxyFactory;

        public ReportCommandHandler(ILogger logger, IProxyFactory factory)
        {
            _logger = logger;
            _proxyFactory = factory;
        }

        public Task<ReportViewModel> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                using (OutageAccessProxy outageProxy = _proxyFactory.CreateProxy<OutageAccessProxy, IOutageAccessContract>(EndpointNames.OutageAccessEndpoint))
                {
                    try
                    {
                        _logger.LogInfo("[OutageQueryHandler::GetActiveOutages] Sending a GET query to Outage service for active outages.");

                        // @TODO:
                        // - Call Outage Report Service

                        // Data examples
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
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageQueryHandler::GetActiveOutages] Failed to GET active outages from Outage service.", ex);
                        throw ex;
                    }
                }


            }, cancellationToken);
        }
    }
}
