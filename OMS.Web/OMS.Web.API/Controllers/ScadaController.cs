using MediatR;
using OMS.Web.Common.Constants;
using OMS.Web.Services.Commands;
using OMS.Web.UI.Models.BindingModels;
using System.Threading.Tasks;
using System.Web.Http;

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

            if(command.Command == SwitchCommandType.TURN_ON)
            {
                switchCommand = new TurnOnSwitchCommand(command.Guid);
            }
            else
            {
                switchCommand = new TurnOffSwitchCommand(command.Guid);
            }
              
            _mediator.Send(switchCommand);

            return Ok($"{switchCommand.Command.ToString()} command for {command.Guid} sent");
        }

    }
}
