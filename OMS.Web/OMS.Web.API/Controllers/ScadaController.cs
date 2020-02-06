namespace OMS.Web.API.Controllers
{
    using System;
    using MediatR;
    using System.Web.Http;
    using OMS.Web.Services.Commands;
    using OMS.Web.UI.Models.BindingModels;
    using System.Collections.Generic;

    public class ScadaController : ApiController
    {
        private readonly IMediator _mediator;

        private readonly Dictionary<SwitchCommandType, Func<long, SwitchCommandBase>> switchCommandMap =
            new Dictionary<SwitchCommandType, Func<long, SwitchCommandBase>>
            {
                { SwitchCommandType.TURN_OFF, (long gid) => new TurnOffSwitchCommand(gid) },
                { SwitchCommandType.TURN_ON, (long gid) => new TurnOnSwitchCommand(gid) }
            };

        public ScadaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public IHttpActionResult Post(SwitchCommandBindingModel command)
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
                return InternalServerError();
            }

            return Ok($"{switchCommand.Command.ToString()} command for {command.Guid} sent");
        }

    }
}
