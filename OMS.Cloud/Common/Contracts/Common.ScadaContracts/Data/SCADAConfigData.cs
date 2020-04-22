using System.Net;
using System.Runtime.Serialization;
using OMS.Common.SCADA;

namespace OMS.Common.ScadaContracts.Data
{
    [DataContract]
    public class ScadaConfigData : IScadaConfigData
    {
        [DataMember]
        public ushort TcpPort { get; set; }
        [DataMember]
        public IPAddress IpAddress { get; set; }
        [DataMember]
        public byte UnitAddress { get; set; }
        [DataMember]
        public ushort AcquisitionInterval { get; set; }
        [DataMember]
        public ushort FunctionExecutionInterval { get; set; }
        [DataMember]
        public string ModbusSimulatorExeName { get; set; } = string.Empty;
        [DataMember]
        public string ModbusSimulatorExePath { get; set; } = string.Empty;
    }
}