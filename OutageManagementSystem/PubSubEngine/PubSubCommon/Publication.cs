using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using System.Runtime.Serialization;

namespace PubSubCommon
{
	[DataContract]
	public class Publication : IPublication
	{
		public Publication(Topic topic, IPublishableMessage message)
		{
			Topic = topic;
			Message = message;
		}

		[DataMember]
		public Topic Topic { get; private set; }

		[DataMember]
		public IPublishableMessage Message { get; private set; }
	}
}
