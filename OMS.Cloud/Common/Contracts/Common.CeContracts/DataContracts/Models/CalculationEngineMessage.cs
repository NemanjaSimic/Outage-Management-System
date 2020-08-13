using System.Runtime.Serialization;
using OMS.Common.PubSubContracts.Interfaces;

namespace Common.CeContracts
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
