using Common.Web.Services.Queries;
using Common.Web.UI.Models.ViewModels;
using MediatR;
using OMS.Common.Cloud.Names;
using Outage.Common.PubSub.OutageDataContract;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ILogger = OMS.Common.Cloud.Logger.ICloudLogger;

namespace Common.Web.Services.Handlers
{
    public class OutageQueryHandler :
        IRequestHandler<GetActiveOutagesQuery, IEnumerable<ActiveOutageViewModel>>,
        IRequestHandler<GetArchivedOutagesQuery, IEnumerable<ArchivedOutageViewModel>>
    {
        private readonly ILogger _logger;
        private readonly IOutageMapper _mapper;
        IProxyFactory _proxyFactory;

        public OutageQueryHandler(ILogger logger, IOutageMapper mapper, IProxyFactory factory)
        {
            _logger = logger;
            _mapper = mapper;
            _proxyFactory = factory;
        }

        public Task<IEnumerable<ActiveOutageViewModel>> Handle(GetActiveOutagesQuery request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                using (OutageAccessProxy outageProxy = _proxyFactory.CreateProxy<OutageAccessProxy, IOutageAccessContract>(EndpointNames.OutageAccessEndpoint))
                {
                    try
                    {
                        _logger.LogInformation("[OutageQueryHandler::GetActiveOutages] Sending a GET query to Outage service for active outages.");
                        IEnumerable<ActiveOutageMessage> activeOutages = outageProxy.GetActiveOutages();

                        IEnumerable<ActiveOutageViewModel> activeOutageViewModels = _mapper.MapActiveOutages(activeOutages);
                        return activeOutageViewModels;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageQueryHandler::GetActiveOutages] Failed to GET active outages from Outage service.", ex);
                        throw ex;
                    }
                }
            });
        }

        public Task<IEnumerable<ArchivedOutageViewModel>> Handle(GetArchivedOutagesQuery request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                using (OutageAccessProxy outageProxy = _proxyFactory.CreateProxy<OutageAccessProxy, IOutageAccessContract>(EndpointNames.OutageAccessEndpoint))
                {
                    try
                    {
                        _logger.LogInformation("[OutageQueryHandler::GetArchivedOutages] Sending a GET query to Outage service for archived outages.");
                        IEnumerable<ArchivedOutageMessage> archivedOutages = outageProxy.GetArchivedOutages();

                        IEnumerable<ArchivedOutageViewModel> archivedOutageViewModels = _mapper.MapArchivedOutages(archivedOutages);
                        return archivedOutageViewModels;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[OutageQueryHandler::GetArchivedOutages] Failed to GET archived outages from Outage service.", ex);
                        throw ex;
                    }
                }
            });
        }
    }
}
