using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.UI
{
    [DataContract]
    public class UIMeasurement
    {
        [DataMember]
        public long Gid { get; set; }
        [DataMember]
        public float Value { get; set; }
        [DataMember]
        public string Type { get; set; }
    }
}
