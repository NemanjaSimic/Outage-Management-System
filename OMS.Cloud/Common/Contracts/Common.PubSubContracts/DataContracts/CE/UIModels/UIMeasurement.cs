using Common.PubSubContracts.DataContracts.CE.Interfaces;
using System.Runtime.Serialization;

namespace Common.PubSubContracts.DataContracts.CE.UIModels
{
	[DataContract(IsReference = true)]
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
