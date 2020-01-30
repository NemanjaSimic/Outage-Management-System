namespace OMS.Email.Interfaces
{
    public interface IIdleEmailClient : IEmailClient
    {
        bool StartIdling();
        void RegisterIdleHandler();
    }
}
