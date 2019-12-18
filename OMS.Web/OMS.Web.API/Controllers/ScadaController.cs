using MediatR;
using System.Web.Http;
using OMS.Web.Common.Constants;
using OMS.Web.Services.Commands;
using OMS.Web.UI.Models.BindingModels;

namespace OMS.Web.API.Controllers
{
    public class ScadaController : ApiController
    {
        private readonly IMediator _mediator;

        public ScadaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public IHttpActionResult Post(SwitchCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SwitchCommandBase switchCommand;

            if (command.Command == SwitchCommandType.TURN_ON)
            {
                switchCommand = new TurnOnSwitchCommand(command.Guid);
            }
            else if (command.Command == SwitchCommandType.TURN_OFF)
            {
                switchCommand = new TurnOffSwitchCommand(command.Guid);
            }
            else
            {
                return BadRequest("Invalid CommandType.");
            }

            _mediator.Send(switchCommand);

            return Ok($"{switchCommand.Command.ToString()} command for {command.Guid} sent");
        }

    }
}
