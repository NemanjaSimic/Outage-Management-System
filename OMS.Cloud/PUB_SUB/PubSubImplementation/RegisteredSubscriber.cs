using OMS.Common.Cloud;
using System;
using System.Runtime.Serialization;

namespace PubSubImplementation
{
    [DataContract]
    public class RegisteredSubscriber
    {
        [DataMember]
        public ServiceType ServiceType { get; private set; }
        [DataMember]
        public Uri SubcriberUri { get; private set; }

        public RegisteredSubscriber(Uri subcriberUri, ServiceType serviceType)
        {
            ServiceType = serviceType;
            SubcriberUri = subcriberUri;
        }
    }
}
