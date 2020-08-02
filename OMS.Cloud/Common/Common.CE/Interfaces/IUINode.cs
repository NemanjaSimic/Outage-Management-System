using System.Collections.Generic;

namespace Common.CE.Interfaces
{
	public interface IUINode
	{
		string Description { get; set; }
		string DMSType { get; set; }
		long Id { get; set; }
		bool IsActive { get; set; }
		bool IsRemote { get; set; }
		List<IUIMeasurement> Measurements { get; set; }
		string Mrid { get; set; }
		string Name { get; set; }
		float NominalVoltage { get; set; }
		bool NoReclosing { get; set; }
	}
}