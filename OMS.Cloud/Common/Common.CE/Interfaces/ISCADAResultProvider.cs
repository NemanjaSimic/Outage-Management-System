using Common.PubSub;

namespace Common.CE.Interfaces
{
    public interface ISCADAResultHandler
    {
        void HandleResult(IPublishableMessage message);
    }
}
