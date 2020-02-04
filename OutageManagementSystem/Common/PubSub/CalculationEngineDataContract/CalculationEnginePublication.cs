using System.Runtime.Serialization;

namespace Outage.Common.PubSub.CalculationEngineDataContract
{

    //[Serializable]
    [DataContract]
    public class CalculationEnginePublication : Publication
    {
        public CalculationEnginePublication(Topic topic, CalculationEngineMessage message) : base(topic, message)
        {

        }
    }
}
