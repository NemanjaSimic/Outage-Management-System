using OMS.Common.Cloud;

namespace OMS.Common.PubSubContracts.Interfaces
{
    public interface IPublication
    {
        Topic Topic { get; }
        IPublishableMessage Message { get; }
    }


    public interface IPublishableMessage
    {
    }
}
