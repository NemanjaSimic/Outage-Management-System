using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OMS.Web.Services.Commands;
using OMS.Web.UI.Models.BindingModels;
using OMS.Web.UI.Models.ViewModels;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get([System.Web.Http.FromUri] ReportOptions options)
        {
            var report = await _mediator.Send<ReportViewModel>(new GenerateReportCommand(options));
            return Ok(report);
        }
    }
}
