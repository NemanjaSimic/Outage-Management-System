using OMS.Common.Cloud;
using OMS.Common.PubSubContracts.DataContracts;
using System.Runtime.Serialization;

namespace Common.CeContracts
{
    [DataContract]
    public class CalculationEnginePublication : Publication
    {
        public CalculationEnginePublication(Topic topic, CalculationEngineMessage message) : base(topic, message)
        {

        }
    }
}
