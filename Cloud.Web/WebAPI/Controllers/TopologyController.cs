using System.Threading.Tasks;
using Common.Web.Services.Queries;
using Common.Web.UI.Models.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopologyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TopologyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            OmsGraphViewModel graph = await _mediator.Send<OmsGraphViewModel>(new GetTopologyQuery());
            return Ok(graph);
        }
    }
}
