using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OMS.Web.Services.Queries;
using OMS.Web.UI.Models.ViewModels;

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
