using System.Runtime.Serialization;

namespace Outage.Common.PubSub.OutageDataContract
{
    [DataContract]
    public class OutagePublication : Publication
    { 
        public OutagePublication(Topic topic, OutageMessage message)
            : base (topic, message)
        {
        }
    }
}
