namespace OMS.EmailImplementation.Interfaces
{
    public interface IIdleEmailClient : IEmailClient
	{
		bool StartIdling();
		void RegisterIdleHandler();
	}
}
