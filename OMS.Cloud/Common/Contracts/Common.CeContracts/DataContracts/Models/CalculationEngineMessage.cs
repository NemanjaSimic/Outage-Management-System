using System.Runtime.Serialization;
using Common.PubSub;
using OMS.Common.PubSub;

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
