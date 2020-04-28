using Outage.Common;
using System.Runtime.Serialization;

namespace OMS.Common.ScadaContracts.DataContracts
{
    [DataContract]
    public class CommandDescription
    {
        [DataMember]
        public ushort Address { get; set; }
        [DataMember]
        public int Value { get; set; }
        [DataMember]
        public CommandOriginType CommandOrigin { get; set; }
    }
}
