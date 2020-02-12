namespace OMS.Web.Adapter.Contracts
{
    public interface IScadaClient
    {
        void SendCommand(long guid, int value);
    }
}
