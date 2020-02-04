using MediatR;
using OMS.Web.Services.Queries;
using OMS.Web.UI.Models.ViewModels;
using Outage.Common.ServiceContracts.OMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace OMS.Web.API.Controllers
{
    public class OutageController : ApiController
    {
        private readonly IMediator _mediator;

        public OutageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ResponseType(typeof(IEnumerable<ActiveOutage>))]
        [Route("api/outage/getActive")]
        public async Task<IHttpActionResult> GetActive()
        {
            IEnumerable<ActiveOutage> activeOutages = await _mediator.Send<IEnumerable<ActiveOutage>>(new GetActiveOutagesQuery());
            return Ok(activeOutages);
        }

        [HttpGet]
        [ResponseType(typeof(IEnumerable<ArchivedOutage>))]
        [Route("api/outage/getArchived")]
        public async Task<IHttpActionResult> GetArchived()
        {
            IEnumerable<ArchivedOutage> activeOutages = await _mediator.Send<IEnumerable<ArchivedOutage>>(new GetArchivedOutagesQuery());
            return Ok(activeOutages);
        }
    }
}