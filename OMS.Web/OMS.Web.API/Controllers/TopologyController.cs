namespace OMS.Web.API.Controllers
{
    using MediatR;
    using OMS.Web.Services.Queries;
    using OMS.Web.UI.Models.ViewModels;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class TopologyController : ApiController
    {
        private readonly IMediator _mediator;

        public TopologyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            OmsGraph graph = await _mediator.Send<OmsGraph>(new GetTopologyQuery());
            return Ok(graph);
        }
    }
}
