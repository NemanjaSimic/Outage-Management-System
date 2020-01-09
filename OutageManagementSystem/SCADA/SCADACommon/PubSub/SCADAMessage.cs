using Outage.Common.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADACommon.PubSub
{
    //TODO: scada message types

    [DataContract]
    public class SCADAMessage : ISCADAMessage
    {
        [DataMember]
        public long Gid { get; set; }
        [DataMember]
        public object Value { get; set; }
    }
}
