namespace OMS.EmailImplementation.Interfaces
{
    public interface IDispatcher
	{
		bool IsConnected { get; }
		void Dispatch(long gid);
	}
}
