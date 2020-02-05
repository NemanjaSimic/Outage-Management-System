<<<<<<< 4922d294ad4802527c71ac4f94f01f3f712a64bb
﻿namespace OMS.Web.API.Controllers
=======
﻿using MediatR;
using OMS.Web.Services.Queries;
using OMS.Web.UI.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace OMS.Web.API.Controllers
>>>>>>> WEB: backend ViewModel ArchivedOutage
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
<<<<<<< 4922d294ad4802527c71ac4f94f01f3f712a64bb
        [Route("api/outage/active")]
        public async Task<IHttpActionResult> GetActive()
        {
            IEnumerable<ActiveOutageViewModel> activeOutages = await _mediator.Send(new GetActiveOutagesQuery());
=======
        [ResponseType(typeof(IEnumerable<Outage.Common.ServiceContracts.OMS.ActiveOutage>))]
        [Route("api/outage/getActive")]
        public async Task<IHttpActionResult> GetActive()
        {
            IEnumerable<Outage.Common.ServiceContracts.OMS.ActiveOutage> activeOutages = await _mediator.Send<IEnumerable<Outage.Common.ServiceContracts.OMS.ActiveOutage>>(new GetActiveOutagesQuery());
>>>>>>> WEB: backend ViewModel ArchivedOutage
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