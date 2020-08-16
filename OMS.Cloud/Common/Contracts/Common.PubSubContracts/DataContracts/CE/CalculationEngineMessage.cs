using Common.PubSubContracts.DataContracts.CE.Interfaces;
using Common.PubSubContracts.DataContracts.CE.UIModels;
using OMS.Common.PubSubContracts.Interfaces;
using System.Runtime.Serialization;


namespace Common.PubSubContracts.DataContracts.CE
{
    [DataContract]
    [KnownType(typeof(TopologyForUIMessage))]
    [KnownType(typeof(OMSModelMessage))]
    [KnownType(typeof(OutageTopologyModel))]
    [KnownType(typeof(OutageTopologyElement))]
    public abstract class CalculationEngineMessage : IPublishableMessage
    {
    }

    [DataContract]
    [KnownType(typeof(UIModel))]
    public class TopologyForUIMessage : CalculationEngineMessage
    {
        [DataMember]
        public IUIModel UIModel { get; set; }

        public TopologyForUIMessage(IUIModel uiModel)
        {
            UIModel = uiModel;
        }

    }

    [DataContract]
    [KnownType(typeof(OutageTopologyModel))]
    [KnownType(typeof(OutageTopologyElement))]
    public class OMSModelMessage : CalculationEngineMessage
    {
        [DataMember]
        //todo: try this instead =>        public IOutageTopologyModel OutageTopologyModel { get; set; }
        public IOutageTopologyModel OutageTopologyModel { get; set; }

        public OMSModelMessage(IOutageTopologyModel outageTopologyModel)
        {
            OutageTopologyModel = outageTopologyModel;
        }
    }
}
