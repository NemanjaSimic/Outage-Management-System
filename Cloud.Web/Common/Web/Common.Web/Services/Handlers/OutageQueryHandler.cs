﻿using Common.Web.Mappers;
using Common.Web.Services.Queries;
using Common.Web.Models.ViewModels;
using MediatR;
using OMS.Common.Cloud.Names;
using Outage.Common.PubSub.OutageDataContract;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ILogger = OMS.Common.Cloud.Logger.ICloudLogger;
using Common.Contracts.WebAdapterContracts;

namespace Common.Web.Services.Handlers
{
    public class OutageQueryHandler :
        IRequestHandler<GetActiveOutagesQuery, IEnumerable<ActiveOutageViewModel>>,
        IRequestHandler<GetArchivedOutagesQuery, IEnumerable<ArchivedOutageViewModel>>
        //outageaccesssclient
    {
        private readonly ILogger _logger;
        private readonly IOutageMapper _mapper;

        public OutageQueryHandler(ILogger logger, IOutageMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        public Task<IEnumerable<ActiveOutageViewModel>> Handle(GetActiveOutagesQuery request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                //using (OutageAccessProxy outageProxy = _proxyFactory.CreateProxy<OutageAccessProxy, IOutageAccessContract>(EndpointNames.OutageAccessEndpoint))
                {
                    try
                    {
                        _logger.LogInformation("[OutageQueryHandler::GetActiveOutages] Sending a GET query to Outage service for active outages.");
                        //IEnumerable<ActiveOutageMessage> activeOutages = outageProxy.GetActiveOutages();

                        //TODO: FIX
                        //IEnumerable<ActiveOutageViewModel> activeOutageViewModels = _mapper.MapActiveOutages(activeOutages);
                        IEnumerable<ActiveOutageViewModel> activeOutageViewModels = _mapper.MapActiveOutages(null);
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
                //using (OutageAccessProxy outageProxy = _proxyFactory.CreateProxy<OutageAccessProxy, IOutageAccessContract>(EndpointNames.OutageAccessEndpoint))
                {
                    try
                    {
                        _logger.LogInformation("[OutageQueryHandler::GetArchivedOutages] Sending a GET query to Outage service for archived outages.");
                        //IEnumerable<ArchivedOutageMessage> archivedOutages = outageProxy.GetArchivedOutages();

                        //TODO: FIX
                        //IEnumerable<ArchivedOutageViewModel> archivedOutageViewModels = _mapper.MapArchivedOutages(archivedOutages);
                        IEnumerable<ArchivedOutageViewModel> archivedOutageViewModels = _mapper.MapArchivedOutages(null);
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
