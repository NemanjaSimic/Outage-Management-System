using System;
using System.Collections.Generic;
using Common.Web.Services.Commands;
using Common.Web.Models.BindingModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OMS.Common.Cloud.Logger;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScadaController : ControllerBase
    {
        private readonly string baseLogString;

        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private readonly IMediator _mediator;

        private readonly Dictionary<SwitchCommandType, Func<long, SwitchCommandBase>> switchCommandMap =
            new Dictionary<SwitchCommandType, Func<long, SwitchCommandBase>>
            {
                { SwitchCommandType.OPEN, (long gid) => new OpenSwitchCommand(gid) },
                { SwitchCommandType.CLOSE, (long gid) => new CloseSwitchCommand(gid) }
            };

        public ScadaController(IMediator mediator)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

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
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Post => exception {e.Message}";
                Logger.LogError(errorMessage, e);
                return StatusCode(500);
            }

            return Ok($"{switchCommand.Command.ToString()} command for {command.Guid} sent");
        }
    }
}
