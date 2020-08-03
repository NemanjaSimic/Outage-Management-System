using Common.Web.UI.Models.BindingModels;

namespace Common.Web.Services.Commands
{
    public class OpenSwitchCommand : SwitchCommandBase
    {
        public OpenSwitchCommand(long gid) : base(gid)
        {
            Command = SwitchCommandType.OPEN;
        }
    }
}
