using MediatR;
using OMS.Web.Adapter.Contracts;
using OMS.Web.Adapter.Topology;
using OMS.Web.Common;
using OMS.Web.Common.Exceptions;
using OMS.Web.Common.Mappers;
using OMS.Web.Services.Queries;
using OMS.Web.UI.Models.ViewModels;
using Outage.Common.UI;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Web.Services.Handlers
{
    public class TopologyQueryHandler : IRequestHandler<GetTopologyQuery, OmsGraph>
    {
        private readonly ITopologyClient _topologyClient;
        private readonly IGraphMapper _mapper;

        public TopologyQueryHandler(IGraphMapper mapper)
        {
            _mapper = mapper;

            // nece dependency injection zbog nekog unity updatea
            // al radi ovako
            string topologyServiceAddress = AppSettings.Get<string>("topologyServiceAddress");
            _topologyClient = new TopologyClientProxy(topologyServiceAddress);
        }

        public Task<OmsGraph> Handle(GetTopologyQuery request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    UIModel topologyModel =  _topologyClient.GetTopology();
                    OmsGraph graph = _mapper.MapTopology(topologyModel);
                    return graph;
                }
                catch (Exception)
                {
                    throw new TopologyException();
                }
            });
        }
    }
}
