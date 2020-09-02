using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.OMS.OutageSimulator
{
    [DataContract]
    public enum IsolationPointType : int
    {
        [EnumMember]
        OPTIMUM = 1,

        [EnumMember]
        DEFAULT = 2,
    }

    [DataContract]
    public class MonitoredIsolationPoint
    {
        [DataMember]
        public long IsolationElementGid { get; set; }

        public long DiscreteMeasurementGid { get; set; }

        [DataMember]
        public DiscreteModbusData DiscreteModbusData { get; set; }
        
        [DataMember]
        public long SimulatedOutageElementGid { get; set; }

        [DataMember]
        public IsolationPointType IsolationPointType { get; set; }
    }
}
