using CECommon;
using Common.Web.Mappers;
using Common.Web.Services.Queries;
using Common.Web.Models.ViewModels;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ILogger = OMS.Common.Cloud.Logger.ICloudLogger;
using OMS.Common.WcfClient.CE;
using Common.CeContracts.TopologyProvider;

namespace Common.Web.Services.Handlers
{
    public class TopologyQueryHandler : IRequestHandler<GetTopologyQuery, OmsGraphViewModel>
    {
        private readonly IGraphMapper _mapper;
        private readonly ILogger _logger;

        public TopologyQueryHandler(IGraphMapper mapper, ILogger logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OmsGraphViewModel> Handle(GetTopologyQuery request, CancellationToken cancellationToken)
        {
            ITopologyProviderContract topologyServiceClient = TopologyProviderClient.CreateClient();

            try
            {
                _logger.LogInformation("[TopologyQueryHandler::GetTopologyQuery] Sending GET query to topology client.");
                UIModel topologyModel = await topologyServiceClient.GetUIModel();
                OmsGraphViewModel graph = _mapper.Map(topologyModel);
                return graph;
            }
            catch (Exception ex)
            {
                _logger.LogError("[TopologyQueryHandler::GetTopologyQuery] Sending GET query to topology client failed.", ex);
                return null;
            }
        }
    }
}
