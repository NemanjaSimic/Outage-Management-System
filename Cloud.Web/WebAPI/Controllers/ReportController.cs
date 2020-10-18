using System.Threading.Tasks;
using Common.Web.Services.Commands;
using Common.Web.Models.BindingModels;
using Common.Web.Models.ViewModels;
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
        public async Task<IActionResult> Get([FromQuery] ReportOptions options)
        {
            options.StartDate = options.StartDate.Value.Date;
            options.EndDate = options.EndDate.Value.Date.AddDays(1).AddTicks(-1);

            var report = await _mediator.Send<ReportViewModel>(new GenerateReportCommand(options));
            return Ok(report);
        }
    }
}
