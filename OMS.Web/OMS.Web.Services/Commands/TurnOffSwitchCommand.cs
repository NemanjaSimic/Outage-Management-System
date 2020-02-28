namespace OMS.Web.Services.Commands
{
    using OMS.Web.UI.Models.BindingModels;  
    
    public class OpenSwitchCommand : SwitchCommandBase
    {
        public OpenSwitchCommand(long gid) : base(gid) 
        {
            Command = SwitchCommandType.OPEN;
        }
    }
}
