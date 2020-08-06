namespace Common.CE.Interfaces
{
	public interface IUIMeasurement
	{
		long Gid { get; set; }
		string Type { get; set; }
		float Value { get; set; }
	}
}