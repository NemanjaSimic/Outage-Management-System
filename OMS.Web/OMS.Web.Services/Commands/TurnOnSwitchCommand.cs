namespace OMS.Web.Services.Commands
{
    using OMS.Web.UI.Models.BindingModels;
    
    public class TurnOnSwitchCommand : SwitchCommandBase
    {
        public TurnOnSwitchCommand(long gid) : base(gid)
        {
            Command = SwitchCommandType.TURN_ON;
        }
    }
}
