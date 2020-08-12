using Common.PubSub;

namespace Common.CeContracts
{ 
    public interface ISCADAResultHandler
    {
        void HandleResult(IPublishableMessage message);
    }
}
