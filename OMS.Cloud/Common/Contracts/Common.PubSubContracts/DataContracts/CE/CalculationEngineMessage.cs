using OMS.Common.PubSubContracts.Interfaces;
using System.Runtime.Serialization;


namespace Common.PubSubContracts.DataContracts.CE
{
    [DataContract]
	public class CalculationEngineMessage : IPublishableMessage
	{
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
