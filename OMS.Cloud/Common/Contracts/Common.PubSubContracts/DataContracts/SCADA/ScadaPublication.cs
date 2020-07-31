using OMS.Common.Cloud;
using System.Runtime.Serialization;

namespace OMS.Common.PubSubContracts.DataContracts.SCADA
{
    [DataContract]
    public class ScadaPublication : Publication
    {
        public ScadaPublication(Topic topic, ScadaMessage message)
                : base(topic, message)
        {
        }
    }

}
