using System;
using System.Runtime.Serialization;
using Outage.Common.PubSub;

namespace Outage.Common.PubSub.SCADADataContract
{
    [Serializable]
    [DataContract]
    //[KnownType(typeof(Publication))]
    //[KnownType(typeof(IPublication))]
    public class SCADAPublication : Publication
    {
        public SCADAPublication(Topic topic, SCADAMessage message)
            : base(topic, message)
        {
        }
    }
}