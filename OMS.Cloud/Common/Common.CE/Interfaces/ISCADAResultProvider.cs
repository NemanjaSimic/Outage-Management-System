using Common.PubSub;

namespace CECommon.Interfaces
{
    public interface ISCADAResultHandler
    {
        void HandleResult(IPublishableMessage message);
    }
}
