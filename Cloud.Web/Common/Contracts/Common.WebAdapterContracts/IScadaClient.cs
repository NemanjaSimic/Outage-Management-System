namespace Common.Contracts.WebAdapterContracts
{
    public interface IScadaClient
    {
        void SendCommand(long guid, int value);
    }
}
