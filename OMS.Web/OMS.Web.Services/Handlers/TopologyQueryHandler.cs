namespace OMS.Web.Services.Handlers
{
    using MediatR;
    using OMS.Web.Adapter.Contracts;
    using OMS.Web.Adapter.Topology;
    using OMS.Web.Common;
    using OMS.Web.Common.Exceptions;
    using OMS.Web.Common.Mappers;
    using OMS.Web.Services.Queries;
    using OMS.Web.UI.Models.ViewModels;
    using Outage.Common;
    using Outage.Common.UI;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class TopologyQueryHandler : IRequestHandler<GetTopologyQuery, OmsGraphViewModel>
    {
        private readonly ITopologyClient _topologyClient;
        private readonly IGraphMapper _mapper;
        private readonly ILogger _logger;

        public TopologyQueryHandler(IGraphMapper mapper, ILogger logger)
        {
            _mapper = mapper;
            _logger = logger;

            // nece dependency injection zbog nekog unity updatea
            string topologyServiceAddress = AppSettings.Get<string>("topologyServiceAddress");
            _topologyClient = new TopologyClientProxy(topologyServiceAddress);
        }

        public Task<OmsGraphViewModel> Handle(GetTopologyQuery request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    _logger.LogInfo("[TopologyQueryHandler::GetTopologyQuery] Sending GET query to topology client.");
                    UIModel topologyModel = _topologyClient.GetTopology();
                    OmsGraphViewModel graph = _mapper.Map(topologyModel);
                    return graph;
                }
                catch (Exception ex)
                {
                    _logger.LogInfo("[TopologyQueryHandler::GetTopologyQuery] Sending GET query to topology client.");
                    throw new TopologyException("GET query on topology failed.", ex);
                }
            });
        }
    }
}
