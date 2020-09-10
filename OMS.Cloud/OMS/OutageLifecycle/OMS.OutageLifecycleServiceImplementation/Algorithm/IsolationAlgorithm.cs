using System.Runtime.Serialization;

namespace OMS.OutageLifecycleImplementation.Algorithm
{
    [DataContract]
    public class IsolationAlgorithm
    {
        [DataMember]
        public long HeadBreakerGid { get; set; }
        [DataMember]
        public long CurrentBreakerGid { get; set; }
        [DataMember]
        public long RecloserGid { get; set; }

        [DataMember]
        public long HeadBreakerMeasurementGid { get; set; }
        [DataMember]
        public long CurrentBreakerMeasurementGid { get; set; }
        [DataMember]
        public long RecloserMeasurementGid { get; set; }

        [DataMember]
        public long OutageId { get; set; }

        [DataMember]
        public int CycleCounter { get; set; }
    }
}
