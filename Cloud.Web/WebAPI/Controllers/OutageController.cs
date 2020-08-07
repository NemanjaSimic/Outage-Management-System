using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Web.Services.Commands;
using Common.Web.Services.Queries;
using Common.Web.Models.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OMS.Common.Cloud.Logger;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutageController : ControllerBase
    {
        private readonly string baseLogString;

        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private readonly IMediator _mediator;

        public OutageController(IMediator mediator)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

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
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} IsolateOutage => exception {e.Message}";
                Logger.LogError(errorMessage, e);
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
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} SendOutageLocationIsolationCrew => exception {e.Message}";
                Logger.LogError(errorMessage, e);
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
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} SendOutageRepairCrew => exception {e.Message}";
                Logger.LogError(errorMessage, e);
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
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} ValidateOutage => exception {e.Message}";
                Logger.LogError(errorMessage, e);
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
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} ResolveOutage => exception {e.Message}";
                Logger.LogError(errorMessage, e);
                return StatusCode(500);
            }

            return Ok("Resolved.");
        }
    }
}
