using Outage.Common;
using Outage.Common.PubSub;

namespace Outage.SCADA.SCADA_Common.PubSub
{
    public class SCADAPublication : IPublication
    {
        public Topic Topic { get; set; }

        public IPublishableMessage Message { get; set; }
    }
}
