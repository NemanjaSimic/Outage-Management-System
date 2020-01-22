using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Outage.Common.UI;

namespace Outage.Common.PubSub.CalculationEngineDataContract
{
    [DataContract]
    public abstract class CalculationEngineMessage : IPublishableMessage
    {

    }

    //[Serializable]
    [DataContract]
    public class TopologyForUIMessage : CalculationEngineMessage
    {
        [DataMember]
        public UIModel UIModel { get; set; }

        public TopologyForUIMessage(UIModel uIModel)
        {
            UIModel = uIModel;
        }
        
    }
}
