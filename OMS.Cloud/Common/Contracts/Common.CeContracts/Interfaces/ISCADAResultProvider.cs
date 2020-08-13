using OMS.Common.PubSubContracts.Interfaces;

namespace Common.CeContracts
{
    public interface ISCADAResultHandler
    {
        void HandleResult(IPublishableMessage message);
    }
}
