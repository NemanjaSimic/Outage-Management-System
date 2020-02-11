using System.Runtime.Serialization;
using Outage.Common.OutageService.Interface;
using Outage.Common.UI;

namespace Outage.Common.PubSub.CalculationEngineDataContract
{
    [DataContract]
    public abstract class CalculationEngineMessage : IPublishableMessage
    {

    }

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

    [DataContract]
    public class OMSModelMessage : CalculationEngineMessage
    {
        [DataMember]
        public IOutageTopologyModel OutageTopologyModel { get; set; }

        public OMSModelMessage(IOutageTopologyModel outageTopologyModel)
        {
            OutageTopologyModel = outageTopologyModel;
        }
    }
}
