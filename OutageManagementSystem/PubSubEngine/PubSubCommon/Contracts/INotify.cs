using System.ServiceModel;

namespace PubSubCommon
{
	[ServiceContract]
	public interface INotify
	{
		[OperationContract(IsOneWay = true)]
		void Notify(string msg);
	}
}
