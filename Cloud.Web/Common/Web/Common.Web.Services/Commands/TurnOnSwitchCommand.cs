using Common.Web.UI.Models.BindingModels;

namespace Common.Web.Services.Commands
{
    public class CloseSwitchCommand : SwitchCommandBase
    {
        public CloseSwitchCommand(long gid) : base(gid)
        {
            Command = SwitchCommandType.CLOSE;
        }
    }
}
