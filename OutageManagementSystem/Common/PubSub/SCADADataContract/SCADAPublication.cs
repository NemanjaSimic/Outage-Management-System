using System;
using System.Runtime.Serialization;
using Outage.Common.PubSub;

namespace Outage.Common.PubSub.SCADADataContract
{
    [DataContract]
    public class SCADAPublication : Publication
    {
        public SCADAPublication(Topic topic, SCADAMessage message)
            : base(topic, message)
        {
        }
    }
}