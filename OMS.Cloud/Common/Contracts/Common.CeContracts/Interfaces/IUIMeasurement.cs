namespace Common.CeContracts
{
	public interface IUIMeasurement
	{
		long Gid { get; set; }
		string Type { get; set; }
		float Value { get; set; }
	}
}