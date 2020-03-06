namespace OMS.Web.API.Controllers
{
    using MediatR;
    using OMS.Web.UI.Models.BindingModels;
    using System.Web.Http;

    public class ReportController : ApiController
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public IHttpActionResult Get([FromUri]ReportOptions options)
        {
            return Ok(options);
        }
    }
}
