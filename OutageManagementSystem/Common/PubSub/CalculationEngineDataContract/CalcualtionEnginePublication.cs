using System.Runtime.Serialization;

namespace Outage.Common.PubSub.CalculationEngineDataContract
{

    //[Serializable]
    [DataContract]
    public class CalcualtionEnginePublication : Publication
    {
        public CalcualtionEnginePublication(Topic topic, CalculationEngineMessage message) : base(topic, message)
        {

        }
    }
}
