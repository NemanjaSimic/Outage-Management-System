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
    using Outage.Common.ServiceContracts;
    using Outage.Common.ServiceProxies;
    using Outage.Common.ServiceProxies.CalcualtionEngine;
    using Outage.Common.UI;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class TopologyQueryHandler : IRequestHandler<GetTopologyQuery, OmsGraphViewModel>
    {
        //private readonly ITopologyClient _topologyClient;
        private readonly IGraphMapper _mapper;
        private readonly ILogger _logger;
        private ProxyFactory _proxyFactory;

        public TopologyQueryHandler(IGraphMapper mapper, ILogger logger)
        {
            _mapper = mapper;
            _logger = logger;

            _proxyFactory = new ProxyFactory();

            // nece dependency injection zbog nekog unity updatea
            //string topologyServiceAddress = AppSettings.Get<string>(ServiceAddress.TopologyServiceAddress);
            //_topologyClient = new TopologyClientProxy(topologyServiceAddress);
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
