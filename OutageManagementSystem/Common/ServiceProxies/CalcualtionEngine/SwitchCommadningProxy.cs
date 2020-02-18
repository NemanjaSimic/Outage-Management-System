using Outage.Common.ServiceContracts.CalculationEngine;
using System;

namespace Outage.Common.ServiceProxies.Commanding
{
    public class SwitchStatusCommadningProxy : BaseProxy<ISwitchStatusCommandingContract>, ISwitchStatusCommandingContract
    {
        public SwitchStatusCommadningProxy(string endpointName)
            : base(endpointName)
        {
        }

        public void SendCommand(long guid, int value)
        {
            try
            {
                Channel.SendCommand(guid, value);
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
