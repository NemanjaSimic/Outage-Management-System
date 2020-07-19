using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OMS.Web.Services.Commands;
using OMS.Web.UI.Models.BindingModels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScadaController : ControllerBase
    {
        private readonly IMediator _mediator;

        private readonly Dictionary<SwitchCommandType, Func<long, SwitchCommandBase>> switchCommandMap =
            new Dictionary<SwitchCommandType, Func<long, SwitchCommandBase>>
            {
                { SwitchCommandType.OPEN, (long gid) => new OpenSwitchCommand(gid) },
                { SwitchCommandType.CLOSE, (long gid) => new CloseSwitchCommand(gid) }
            };

        public ScadaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public IActionResult Post(SwitchCommandBindingModel command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SwitchCommandBase switchCommand = switchCommandMap[command.Command](command.Guid);

            try
            {
                _mediator.Send(switchCommand);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

            return Ok($"{switchCommand.Command.ToString()} command for {command.Guid} sent");
        }
    }
}
