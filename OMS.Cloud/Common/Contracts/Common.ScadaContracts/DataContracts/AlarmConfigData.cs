using OMS.Common.SCADA;
using System.Runtime.Serialization;

namespace OMS.Common.ScadaContracts.DataContracts
{
	[DataContract]
	public class AlarmConfigData : IAlarmConfigData
	{
		[DataMember]
		public float LowPowerLimit { get; set; }
		[DataMember]
		public float HighPowerLimit { get; set; }

		[DataMember]
		public float LowVoltageLimit { get; set; }
		[DataMember]
		public float HighVolageLimit { get; set; }

		[DataMember]
		public float LowFeederCurrentLimit { get; set; }
		[DataMember]
		public float HighFeederCurrentLimit { get; set; }

		[DataMember]
		public float LowCurrentLimit { get; set; }
		[DataMember]
		public float HighCurrentLimit { get; set; }		
    }
}
