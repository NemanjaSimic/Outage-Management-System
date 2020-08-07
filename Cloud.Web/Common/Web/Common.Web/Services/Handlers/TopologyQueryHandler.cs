using Common.Web.Mappers;
using Common.Web.Services.Queries;
using Common.Web.Models.ViewModels;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using OMS.Common.WcfClient.CE;
using Common.CeContracts.TopologyProvider;
using Common.CeContracts;
using OMS.Common.Cloud.Logger;

namespace Common.Web.Services.Handlers
{
    public class TopologyQueryHandler : IRequestHandler<GetTopologyQuery, OmsGraphViewModel>
    {
        private readonly IGraphMapper _mapper;
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public TopologyQueryHandler(IGraphMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<OmsGraphViewModel> Handle(GetTopologyQuery request, CancellationToken cancellationToken)
        {
            ITopologyProviderContract topologyServiceClient = TopologyProviderClient.CreateClient();

            try
            {
                Logger.LogInformation("[TopologyQueryHandler::GetTopologyQuery] Sending GET query to topology client.");
                UIModel topologyModel = await topologyServiceClient.GetUIModel();
                OmsGraphViewModel graph = _mapper.Map(topologyModel);
                return graph;
            }
            catch (Exception ex)
            {
                Logger.LogError("[TopologyQueryHandler::GetTopologyQuery] Sending GET query to topology client failed.", ex);
                return null;
            }
        }
    }
}
