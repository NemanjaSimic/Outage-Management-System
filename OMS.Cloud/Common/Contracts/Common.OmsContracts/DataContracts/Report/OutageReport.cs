using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.OmsContracts.DataContracts.Report
{
    [DataContract]
    public class OutageReport
    {
        [DataMember]
        public IDictionary<string, float> Data { get; set; }
        
        [DataMember]
        public string Type { get; set; }
    }
}
