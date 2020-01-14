using Outage.Common.PubSub;
using System.Runtime.Serialization;

namespace Outage.SCADA.SCADA_Common.PubSub
{
    //TODO: scada message types

    [DataContract]
    public class SCADAMessage : ISCADAMessage
    {
        [DataMember]
        public long Gid { get; set; }
        [DataMember]
        public object Value { get; set; }
    }
}
