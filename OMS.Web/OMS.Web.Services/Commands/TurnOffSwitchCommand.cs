namespace OMS.Web.Services.Commands
{
    using OMS.Web.UI.Models.BindingModels;  
    
    public class TurnOffSwitchCommand : SwitchCommandBase
    {
        public TurnOffSwitchCommand(long gid) : base(gid) 
        {
            Command = SwitchCommandType.TURN_OFF;
        }
    }
}
