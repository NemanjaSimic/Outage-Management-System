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
        public void SendAnalogCommand(long gid, float commandingValue)
        {
            try
            {
                Channel.SendAnalogCommand(gid, commandingValue);
            }
            catch (Exception e)
            {
                string message = "Exception in SendAnalogCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }
        }

        public void SendAnalogCommand(ushort address, float commandingValue)
        {
            try
            {
                Channel.SendAnalogCommand(address, commandingValue);
            }
            catch (Exception e)
            {
                string message = "Exception in SendAnalogCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }
        }

        public void SendDiscreteCommand(long gid, ushort commandingValue)
        {
            try
            {
                Channel.SendDiscreteCommand(gid, commandingValue);
            }
            catch (Exception e)
            {
                string message = "Exception in SendDiscreteCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }
        }

        public void SendDiscreteCommand(ushort address, ushort commandingValue)
        {
            try
            {
                Channel.SendDiscreteCommand(address, commandingValue);
            }
            catch (Exception e)
            {
                string message = "Exception in SendDiscreteCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }
        }
    }
}
