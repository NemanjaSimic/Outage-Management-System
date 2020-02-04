using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.PubSub.OutageDataContract
{
    [DataContract]
    public class OutagePublication : Publication
    { 
        public OutagePublication(Topic topic, OutageMessage message)
            : base (topic, message)
        {

        }
    }
}
