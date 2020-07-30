using Common.CE;
using Common.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
