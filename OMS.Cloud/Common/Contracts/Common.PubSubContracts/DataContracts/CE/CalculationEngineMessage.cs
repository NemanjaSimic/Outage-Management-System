using Common.PubSubContracts.DataContracts.CE.UIModels;
using OMS.Common.PubSubContracts.Interfaces;
using System.Runtime.Serialization;


namespace Common.PubSubContracts.DataContracts.CE
{
    [DataContract]
    [KnownType(typeof(TopologyForUIMessage))]
    [KnownType(typeof(OMSModelMessage))]
    public abstract class CalculationEngineMessage : IPublishableMessage
    {
    }

    [DataContract]
    public class TopologyForUIMessage : CalculationEngineMessage
    {
        [DataMember]
        public UIModel UIModel { get; set; }

        public TopologyForUIMessage(UIModel uiModel)
        {
            UIModel = uiModel;
        }

    }

    [DataContract]
    public class OMSModelMessage : CalculationEngineMessage
    {
        [DataMember]
        public OutageTopologyModel OutageTopologyModel { get; set; }

        public OMSModelMessage(OutageTopologyModel outageTopologyModel)
        {
            OutageTopologyModel = outageTopologyModel;
        }
    }
}
