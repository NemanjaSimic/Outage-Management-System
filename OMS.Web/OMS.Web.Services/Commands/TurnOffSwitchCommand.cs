using OMS.Web.UI.Models.BindingModels;

namespace OMS.Web.Services.Commands
{
    public class TurnOffSwitchCommand : SwitchCommandBase
    {
        public TurnOffSwitchCommand(long gid) : base(gid) 
        {
            Command = SwitchCommandType.TURN_OFF;
        }
    }
}
