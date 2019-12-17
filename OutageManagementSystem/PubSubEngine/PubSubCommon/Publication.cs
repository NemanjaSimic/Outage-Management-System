using System.Runtime.Serialization;
using static PubSubCommon.Enums;

namespace PubSubCommon
{
	[DataContract]
	public class Publication
	{
		//testni primer salje string, treba promeniti da message bude ono sta nam treba u projektu(npr ResourseDescription?)
		public Publication(){}

		public Publication(Topic topic, string message)
		{
			Topic = topic;
			Message = message;
		}

		[DataMember]
		public Topic Topic { get; set; }

		[DataMember]
		public string Message { get; set; }
	}
}
