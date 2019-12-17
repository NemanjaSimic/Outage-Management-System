using System.ServiceModel;
using static PubSubCommon.Enums;

namespace PubSubCommon
{
	[ServiceContract(CallbackContract = typeof(INotify))]
	public interface ISubscriber
	{
		[OperationContract]
		void Subscribe(Topic topic);
	}
}
