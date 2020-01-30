using Outage.Common.PubSub;
using Outage.Common.PubSub.SCADADataContract;
using System.Collections.Generic;

namespace CECommon.Interfaces
{
    public interface ISCADAResultHandler
    {
        void HandleResult(IPublishableMessage message);
    }
}
