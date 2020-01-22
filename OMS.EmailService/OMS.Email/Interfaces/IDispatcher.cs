namespace OMS.Email.Interfaces
{
    public interface IDispatcher
    {
        bool IsConnected { get; }
        void Dispatch(long gid);
    }
}
