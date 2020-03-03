namespace OMS.Web.Services.Commands
{
    using OMS.Web.UI.Models.BindingModels;
    
    public class CloseSwitchCommand : SwitchCommandBase
    {
        public CloseSwitchCommand(long gid) : base(gid)
        {
            Command = SwitchCommandType.CLOSE;
        }
    }
}
