using System.Threading.Tasks;
using Common.Web.Services.Commands;
using Common.Web.UI.Models.BindingModels;
using Common.Web.UI.Models.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
