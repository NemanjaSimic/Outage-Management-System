using Outage.Common.ServiceContracts.CalculationEngine;
using System;

namespace Outage.Common.ServiceProxies.Commanding
{
    public class SwitchStatusCommandingProxy : BaseProxy<ISwitchStatusCommandingContract>, ISwitchStatusCommandingContract
    {
        public SwitchStatusCommandingProxy(string endpointName)
            : base(endpointName)
        {
        }

        public void SendCloseCommand(long guid)
        {
            try
            {
                Channel.SendCloseCommand(guid);
            }
            catch (Exception e)
            {
                string message = "Exception in SendCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }
        }

        public void SendOpenCommand(long guid)
        {
            try
            {
                Channel.SendOpenCommand(guid);
            }
            catch (Exception e)
            {
                string message = "Exception in SendCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }
        }

        [Obsolete("Use SendOpenCommand and SendCloseCommand methods instead")]
        public void SendSwitchCommand(long guid, int value)
        {
            try
            {
                Channel.SendSwitchCommand(guid, value);
            }
            catch (Exception e)
            {
                string message = "Exception in SendCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }
        }
    }
}
