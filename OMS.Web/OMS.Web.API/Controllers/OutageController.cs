﻿﻿﻿namespace OMS.Web.API.Controllers
{
    using MediatR;
    using OMS.Web.Services.Queries;
    using OMS.Web.UI.Models.ViewModels;
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
            IEnumerable<ArchivedOutageViewModel> activeOutages = await _mediator.Send(new GetArchivedOutagesQuery());
            return Ok(activeOutages);
        }
    }
}