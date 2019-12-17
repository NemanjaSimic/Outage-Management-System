using System.ServiceModel;

namespace PubSubCommon
{
	[ServiceContract]
	public interface IPublisher
	{
		[OperationContract]
		void Publish(Publication publication);
	}
}
