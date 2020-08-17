using Common.PubSubContracts.DataContracts.CE.UIModels;
using OMS.Common.PubSubContracts.Interfaces;
using System.Runtime.Serialization;


namespace Common.PubSubContracts.DataContracts.CE
{
    [DataContract]
    [KnownType(typeof(TopologyForUIMessage))]
    [KnownType(typeof(OMSModelMessage))]
    //[KnownType(typeof(OutageTopologyModel))]
    //[KnownType(typeof(OutageTopologyElement))]
    //TODO: clean up
    public abstract class CalculationEngineMessage : IPublishableMessage
    {
    }

    [DataContract]
    //[KnownType(typeof(UIModel))]
    //TODO: clean up
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
    //TODO: clean up
    //[KnownType(typeof(OutageTopologyModel))]
    //[KnownType(typeof(OutageTopologyElement))]
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
