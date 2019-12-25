using Outage.Common;
using Outage.Common.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADA_Common.PubSub
{
    public class SCADAPublication : IPublication
    {
        public Topic Topic { get; set; }

        public IPublishableMessage Message { get; set; }
    }
}
