using OMS.Common.PubSubContracts.Interfaces;
using System.Runtime.Serialization;

namespace Common.PubSubContracts.DataContracts.EMAIL
{
    [DataContract]
    public abstract class EmailServiceMessage : IPublishableMessage
    {
    }
    [DataContract]
    public class EmailToOutageMessage : EmailServiceMessage
    {

        public EmailToOutageMessage(long Gid)
        {
            this.Gid = Gid;
        }
        [DataMember]
        public long Gid { get; set; }
    }
}
