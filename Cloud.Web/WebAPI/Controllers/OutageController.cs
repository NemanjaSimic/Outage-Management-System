using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OMS.Web.Services.Commands;
using OMS.Web.Services.Queries;
using OMS.Web.UI.Models.ViewModels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OutageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> GetActive()
        {
            IEnumerable<ActiveOutageViewModel> activeOutages = await _mediator.Send(new GetActiveOutagesQuery());
            return Ok(activeOutages);
        }

        [HttpGet]
        [Route("archived")]
        public async Task<IActionResult> GetArchived()
        {
            IEnumerable<ArchivedOutageViewModel> archivedOutages = await _mediator.Send(new GetArchivedOutagesQuery());
            return Ok(archivedOutages);
        }

        [HttpPost]
        [Route("isolate/{id}")]
        public async Task<IActionResult> IsolateOutage([System.Web.Http.FromUri] long id)
        {
            try
            {
                _ = await _mediator.Send(new IsolateOutageCommand(id));
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

            return Ok("Isolated.");
        }

        [HttpPost]
        [Route("sendlocationisolationcrew/{id}")]
        public async Task<IActionResult> SendOutageLocationIsolationCrew([System.Web.Http.FromUri] long id)
        {
            try
            {
                await _mediator.Send(new SendOutageLocationIsolationCrewCommand(id));
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

            return Ok("Isolated.");
        }

        [HttpPost]
        [Route("sendrepaircrew/{id}")]
        public async Task<IActionResult> SendOutageRepairCrew([System.Web.Http.FromUri] long id)
        {
            try
            {
                await _mediator.Send(new SendOutageRepairCrewCommand(id));
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

            return Ok("Repair crew sent.");
        }

        [HttpPost]
        [Route("validateresolve/{id}")]
        public async Task<IActionResult> ValidateOutage([System.Web.Http.FromUri] long id)
        {
            try
            {
                await _mediator.Send(new ValidateResolveConditionsCommand(id));
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

            return Ok("Resolve conditions validated.");
        }

        [HttpPost]
        [Route("resolve/{id}")]
        public async Task<IActionResult> ResolveOutage([System.Web.Http.FromUri] long id)
        {
            try
            {
                await _mediator.Send(new ResolveOutageCommand(id));
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

            return Ok("Resolved.");
        }
    }
}
