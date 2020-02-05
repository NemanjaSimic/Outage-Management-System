﻿﻿namespace OMS.Web.API.Controllers
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
<<<<<<< 304ba2c4bcafbfd5a60bca220862cfad38698c1a
        [Route("api/outage/active")]
        public async Task<IHttpActionResult> GetActive()
        {
            IEnumerable<ActiveOutageViewModel> activeOutages = await _mediator.Send(new GetActiveOutagesQuery());
=======
        [ResponseType(typeof(IEnumerable<Outage.Common.PubSub.OutageDataContract.ActiveOutage>))]
        [Route("api/outage/getActive")]
        public async Task<IHttpActionResult> GetActive()
        {
            IEnumerable<Outage.Common.PubSub.OutageDataContract.ActiveOutage> activeOutages = await _mediator.Send<IEnumerable<Outage.Common.PubSub.OutageDataContract.ActiveOutage>>(new GetActiveOutagesQuery());
>>>>>>> Post Merge modifications
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