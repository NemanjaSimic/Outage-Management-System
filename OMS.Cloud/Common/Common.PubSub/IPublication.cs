using OMS.Common.Cloud;

namespace Common.PubSub
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
