using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.OmsContracts.DataContracts.OutageSimulator
{
    [DataContract]
    public class SimulatedOutage
    {
        [DataMember]
        public long OutageElementGid { get; set; }

        [DataMember]
        public List<long> OptimumIsolationPointGids { get; set; }

        [DataMember]
        public List<long> DefaultIsolationPointGids { get; set; }

        [DataMember]
        public Dictionary<long, long> DefaultToOptimumIsolationPointMap { get; set; }

        [IgnoreDataMember]
        public HashSet<long> ElementsOfInteres
        {
            get
            {
                var hashSet = new HashSet<long>();
                hashSet.Add(OutageElementGid);
                hashSet.UnionWith(OptimumIsolationPointGids);
                hashSet.UnionWith(DefaultIsolationPointGids);

                return hashSet;
            }
        }

        public SimulatedOutage()
        {
            OptimumIsolationPointGids = new List<long>();
            DefaultIsolationPointGids = new List<long>();
            DefaultToOptimumIsolationPointMap = new Dictionary<long, long>();
        }
    }
}
