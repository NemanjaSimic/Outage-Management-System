﻿using OMS.Web.Common.Constants;

namespace OMS.Web.Services.Commands
{
    public class TurnOnSwitchCommand : SwitchCommandBase
    {
        public TurnOnSwitchCommand(long gid) : base(gid)
        {
            Command = SwitchCommandType.TURN_ON;
        }
    }
}
