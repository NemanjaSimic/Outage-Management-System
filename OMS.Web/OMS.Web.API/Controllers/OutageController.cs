namespace OMS.Web.API.Controllers
{
    using MediatR;
    using OMS.Web.Services.Commands;
    using OMS.Web.Services.Queries;
    using OMS.Web.UI.Models.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class OutageController : ApiController
    {
        private readonly IMediator _mediator;

        public OutageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("api/outage/active")]
        public async Task<IHttpActionResult> GetActive()
        {
            IEnumerable<ActiveOutageViewModel> activeOutages = await _mediator.Send(new GetActiveOutagesQuery());
            return Ok(activeOutages);
        }

        [HttpGet]
        [Route("api/outage/archived")]
        public async Task<IHttpActionResult> GetArchived()
        {
            IEnumerable<ArchivedOutageViewModel> archivedOutages = await _mediator.Send(new GetArchivedOutagesQuery());
            return Ok(archivedOutages);
        }

        [HttpPost]
        [Route("api/outage/isolate/{id}")]
        public async Task<IHttpActionResult> IsolateOutage([FromUri]long id)
        {
            try
            {
                _ = await _mediator.Send(new IsolateOutageCommand(id));
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return Ok("Isolated.");
        }

        [HttpPost]
        [Route("api/outage/sendcrew/{id}")]
        public async Task<IHttpActionResult> SendOutageCrew([FromUri]long id)
        {
            try
            {
                _ = await _mediator.Send(new SendOutageCrewCommand(id));
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return Ok("Crew sent.");
        }

        [HttpPost]
        [Route("api/outage/resolve/{id}")]
        public async Task<IHttpActionResult> ResolveOutage([FromUri]long id)
        {
            try
            {
                _ = await _mediator.Send(new ResolveOutageCommand(id));
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return Ok("Resolved.");
        }

        [HttpPost]
        [Route("api/outage/validate/{id}")]
        public async Task<IHttpActionResult> ValidateOutage([FromUri]long id)
        {
            try
            {
                _ = await _mediator.Send(new ValidateOutageCommand(id));
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return Ok("Validated.");
        }

    }
}