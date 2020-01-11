using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Outage.Common.PubSub.CalculationEngineDataContract
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(IPublishableMessage))]
    public abstract class CalculationEngineMessage : IPublishableMessage
    {
    }
}
