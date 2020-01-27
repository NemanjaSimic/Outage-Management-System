namespace Outage.Common.PubSub.EmailDataContract
{
    using System.Runtime.Serialization;

    [DataContract]
    public class OutageEmailPublication : Publication
    {
        public OutageEmailPublication(Topic topic, EmailToOutageMessage message) : base(topic, message) { }
    }
}
