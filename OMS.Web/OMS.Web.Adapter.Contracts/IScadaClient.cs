namespace OMS.Web.Adapter.Contracts
{
    public interface IScadaClient
    {
        void SendCommand(long guid, object value);
    }
}
