namespace OMS.Web.API.Controllers
{
    using MediatR;
    using OMS.Web.Services.Commands;
    using OMS.Web.UI.Models.BindingModels;
    using OMS.Web.UI.Models.ViewModels;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class ReportController : ApiController
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get([FromUri]ReportOptions options)
        {
            var report = await _mediator.Send<ReportViewModel>(new GenerateReportCommand(options));
            return Ok(report);
        }
    }
}
