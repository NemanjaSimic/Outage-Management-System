using System.Runtime.Serialization;

namespace Common.CeContracts
{
	[DataContract]
	public class UIMeasurement : IUIMeasurement
	{
		[DataMember]
		public long Gid { get; set; }
		[DataMember]
		public float Value { get; set; }
		[DataMember]
		public string Type { get; set; }
	}
}
