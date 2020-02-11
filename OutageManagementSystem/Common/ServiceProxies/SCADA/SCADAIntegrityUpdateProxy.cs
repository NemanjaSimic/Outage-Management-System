using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies.SCADA
{
    public class SCADAIntegrityUpdateProxy : BaseProxy<ISCADAIntegrityUpdateContract>, ISCADAIntegrityUpdateContract
    {
        public SCADAIntegrityUpdateProxy(string endpoint)
            : base(endpoint)
        {
        }

        public Dictionary<Topic, SCADAPublication> GetIntegrityUpdate()
        {
            Dictionary<Topic, SCADAPublication> result;

            try
            {
                result = Channel.GetIntegrityUpdate();
            }
            catch (Exception e)
            {
                string message = "Exception in SendAnalogCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
        }

        public SCADAPublication GetIntegrityUpdateForSpecificTopic(Topic topic)
        {
            SCADAPublication result;

            try
            {
                result = Channel.GetIntegrityUpdateForSpecificTopic(topic);
            }
            catch (Exception e)
            {
                string message = "Exception in SendAnalogCommand() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
        }
    }
}
