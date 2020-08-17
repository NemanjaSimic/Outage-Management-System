using Common.Web.Mappers;
using Common.Web.Services.Queries;
using Common.Web.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.PubSubContracts.DataContracts.OMS;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.OMS.ModelAccess;

namespace Common.Web.Services.Handlers
{
    public class OutageQueryHandler :
        IRequestHandler<GetActiveOutagesQuery, IEnumerable<ActiveOutageViewModel>>,
        IRequestHandler<GetArchivedOutagesQuery, IEnumerable<ArchivedOutageViewModel>>
    //outageaccesssclient
    {
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private readonly IOutageMapper _mapper;

        public OutageQueryHandler(IOutageMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<ActiveOutageViewModel>> Handle(GetActiveOutagesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("[OutageQueryHandler::GetActiveOutages] Sending a GET query to Outage service for active outages.");
                
                var outageAccessClient = OutageModelAccessClient.CreateClient();
                var activeOutages = await outageAccessClient.GetAllActiveOutages();
                
                var activeOutageViewModels = _mapper.MapActiveOutages(activeOutages);
                return activeOutageViewModels;
            }
            catch (Exception ex)
            {
                Logger.LogError("[OutageQueryHandler::GetActiveOutages] Failed to GET active outages from Outage service.", ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<ArchivedOutageViewModel>> Handle(GetArchivedOutagesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("[OutageQueryHandler::GetArchivedOutages] Sending a GET query to Outage service for archived outages.");

                var outageAccessClient = OutageModelAccessClient.CreateClient();
                var archivedOutages = await outageAccessClient.GetAllArchivedOutages();

                var archivedOutageViewModels = _mapper.MapArchivedOutages(archivedOutages);
                return archivedOutageViewModels;
            }
            catch (Exception ex)
            {
                Logger.LogError("[OutageQueryHandler::GetArchivedOutages] Failed to GET archived outages from Outage service.", ex);
                throw ex;
            }
        }
    }
}
