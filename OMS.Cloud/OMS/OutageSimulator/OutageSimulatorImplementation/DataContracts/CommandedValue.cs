using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OMS.OutageSimulatorImplementation.DataContracts
{
    [DataContract]
    public class CommandedValue
    {
        [DataMember]
        public long MeasurementGid { get; set; }
        [DataMember]
        public ushort Value { get; set; }
        [DataMember]
        public DateTime TimeOfCreation { get; set; }
    }
}
