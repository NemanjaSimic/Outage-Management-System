using Common.Web.Mappers;
using Common.Web.Services.Queries;
using Common.Web.UI.Models.ViewModels;
using MediatR;
using OMS.Common.Cloud.Names;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Web.Services.Handlers
{
    public class TopologyQueryHandler : IRequestHandler<GetTopologyQuery, OmsGraphViewModel>
    {
        private readonly IGraphMapper _mapper;
        private readonly ILogger _logger;
        private IProxyFactory _proxyFactory;

        public TopologyQueryHandler(IGraphMapper mapper, ILogger logger, IProxyFactory factory)
        {
            _mapper = mapper;
            _logger = logger;
            _proxyFactory = factory;
        }

        public Task<OmsGraphViewModel> Handle(GetTopologyQuery request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                using (UITopologyServiceProxy topologyProxy = _proxyFactory.CreateProxy<UITopologyServiceProxy, ITopologyServiceContract>(EndpointNames.TopologyServiceEndpoint))
                {
                    try
                    {
                        _logger.LogInfo("[TopologyQueryHandler::GetTopologyQuery] Sending GET query to topology client.");
                        UIModel topologyModel = topologyProxy.GetTopology();
                        OmsGraphViewModel graph = _mapper.Map(topologyModel);
                        return graph;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[TopologyQueryHandler::GetTopologyQuery] Sending GET query to topology client failed.", ex);
                        return null;
                    }
                }

            });
        }
    }
}
