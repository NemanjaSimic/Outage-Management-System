namespace Outage.Common.PubSub.EmailDataContract
{
    public class EmailServicePublication : Publication
    {
        public EmailServicePublication(Topic topic, IPublishableMessage message) 
            : base(topic, message) { }
    }
}
