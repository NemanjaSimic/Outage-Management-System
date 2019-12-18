using OMS.Web.Common.Constants;

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
