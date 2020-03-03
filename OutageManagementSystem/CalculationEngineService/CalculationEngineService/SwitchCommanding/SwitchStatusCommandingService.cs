using CalculationEngine.SCADAFunctions;
using Outage.Common;
using Outage.Common.ServiceContracts.CalculationEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService.SwitchCommanding
{
    public class SwitchStatusCommandingService : ISwitchStatusCommandingContract
    {
        private readonly SCADACommanding scadaCommanding;

        public SwitchStatusCommandingService()
        {
            scadaCommanding = new SCADACommanding();
        }

        public void SendCloseCommand(long gid)
        {
            //TODO: check if gid is of type DISCRETE or of SWITCH descendence

            scadaCommanding.SendDiscreteCommand(gid, (int)DiscreteCommandingType.CLOSE, CommandOriginType.USER_COMMAND);
        }

        public void SendOpenCommand(long gid)
        {
            //TODO: check if gid is of type DISCRETE or of SWITCH descendence

            scadaCommanding.SendDiscreteCommand(gid, (int)DiscreteCommandingType.OPEN, CommandOriginType.USER_COMMAND);
        }

        [Obsolete("Use SendOpenCommand and SendCloseCommand methods instead")]
        public void SendSwitchCommand(long gid, int value)
        {
            //TODO: check if gid is of type DISCRETE or of SWITCH descendence

            scadaCommanding.SendDiscreteCommand(gid, value, CommandOriginType.USER_COMMAND);
        }
    }
}
