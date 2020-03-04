using Outage.Common.ServiceContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
﻿using Outage.Common.ServiceContracts;
using System.ServiceModel;

namespace Outage.Common.ServiceProxies
{
    public class SCADACommandProxy : BaseProxy<ISCADACommand>, ISCADACommand
    {
        public SCADACommandProxy(string endpointName)
            : base(endpointName)
        {
        }

        public bool SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType)
        {
            bool success;

            try
            {
                success = Channel.SendAnalogCommand(gid, commandingValue, commandOriginType);
            }
            catch (Exception e)
            {
                string message = "Exception in SendAnalogCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }

        public bool SendDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType)
        {
            bool success;

            try
            {
                success = Channel.SendDiscreteCommand(gid, commandingValue, commandOriginType);
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
