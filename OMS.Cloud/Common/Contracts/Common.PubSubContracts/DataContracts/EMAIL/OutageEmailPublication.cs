using OMS.Common.Cloud;
using OMS.Common.PubSubContracts.DataContracts;
using System.Runtime.Serialization;

namespace Common.PubSubContracts.DataContracts.EMAIL
{
	[DataContract]
    public class OutageEmailPublication : Publication
	{
		public OutageEmailPublication(Topic topic, EmailToOutageMessage message) : base(topic, message) { }
	}
}
