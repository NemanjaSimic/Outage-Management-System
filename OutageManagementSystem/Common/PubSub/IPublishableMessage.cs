using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.PubSub
{
    public interface IPublishableMessage
    {
    }

    public interface ISCADAMessage : IPublishableMessage
    {
    }

    public interface ICalculationEngineMessage : IPublishableMessage
    {
    }
}
