using Outage.Common.ServiceContracts;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies.CalcualtionEngine
{
    public class UITopologyServiceProxy : BaseProxy<ITopologyServiceContract>, ITopologyServiceContract
    {
        public UITopologyServiceProxy(string endpoitntName)
            : base(endpoitntName)
        {
        }

        public UIModel GetTopology()
        {
            UIModel result;

            try
            {
                result = Channel.GetTopology();
            }
            catch (Exception e)
            {
                string message = "Exception in GetTopology() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
        }
    }
}
