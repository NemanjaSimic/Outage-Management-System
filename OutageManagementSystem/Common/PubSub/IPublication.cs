using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Outage.Common.PubSub
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
