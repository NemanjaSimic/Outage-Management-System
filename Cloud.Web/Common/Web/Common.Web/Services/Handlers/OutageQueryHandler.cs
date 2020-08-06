using Common.Web.Mappers;
using Common.Web.Services.Queries;
using Common.Web.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ILogger = OMS.Common.Cloud.Logger.ICloudLogger;
using Common.Contracts.WebAdapterContracts;
using Common.OmsContracts;
using OMS.Common.WcfClient.OMS;
using Common.PubSubContracts.DataContracts.OMS;

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

        public async Task<IEnumerable<ActiveOutageViewModel>> Handle(GetActiveOutagesQuery request, CancellationToken cancellationToken)
        {
            IOutageAccessContract outageAccessClient = OutageAccessClient.CreateClient();
            try
            {
                _logger.LogInformation("[OutageQueryHandler::GetActiveOutages] Sending a GET query to Outage service for active outages.");
                // TODO: FIX
                //IEnumerable<ActiveOutageMessage> activeOutages = await outageAccessClient.GetActiveOutages();
                IEnumerable<ActiveOutageMessage> activeOutages = new List<ActiveOutageMessage>();

                IEnumerable<ActiveOutageViewModel> activeOutageViewModels = _mapper.MapActiveOutages(activeOutages);
                return activeOutageViewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError("[OutageQueryHandler::GetActiveOutages] Failed to GET active outages from Outage service.", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<ArchivedOutageViewModel>> Handle(GetArchivedOutagesQuery request, CancellationToken cancellationToken)
        {
            IOutageAccessContract outageAccessClient = OutageAccessClient.CreateClient();
            try
            {
                _logger.LogInformation("[OutageQueryHandler::GetArchivedOutages] Sending a GET query to Outage service for archived outages.");
                // TODO: FIX
                //IEnumerable<ArchivedOutageMessage> archivedOutages = await outageAccessClient.GetArchivedOutages();
                IEnumerable<ArchivedOutageMessage> archivedOutages = new List<ArchivedOutageMessage>();

                IEnumerable<ArchivedOutageViewModel> archivedOutageViewModels = _mapper.MapArchivedOutages(archivedOutages);
                return archivedOutageViewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError("[OutageQueryHandler::GetArchivedOutages] Failed to GET archived outages from Outage service.", ex);
                throw ex;
            }
        }
    }
}
