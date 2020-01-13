using Outage.Common.ServiceContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies
{
    public class SCADACommandProxy : ClientBase<ISCADACommand>, ISCADACommand
    {
        public bool SendAnalogCommand(long gid, float commandingValue)
        {
            bool success;

            try
            {
                success = Channel.SendAnalogCommand(gid, commandingValue);
            }
            catch (Exception e)
            {
                string message = "Exception in SendAnalogCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }

        public bool SendDiscreteCommand(long gid, ushort commandingValue)
        {
            bool success;

            try
            {
                success = Channel.SendDiscreteCommand(gid, commandingValue);
            }
            catch (Exception e)
            {
                string message = "Exception in SendDiscreteCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }
    }
}
