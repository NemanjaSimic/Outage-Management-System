using System.Collections.Generic;

namespace CECommon.Models
{
	public class DailyCurve
	{
		public Dictionary<short, float> TimeValuePairs { get; private set; }
		public DailyCurveType DailyCurveType { get; private set; }
		public DailyCurve(DailyCurveType dailyCurveType)
		{
			TimeValuePairs = new Dictionary<short, float>();
			DailyCurveType = dailyCurveType;
		}

		public bool TryAddPair(short time, float value)
		{
			if (!TimeValuePairs.ContainsKey(time))
			{
				TimeValuePairs.Add(time, value);
				return true;
			}
			else
			{
				return false;
			}
		}

		public float GetValue(short time)
		{
			if (!TimeValuePairs.TryGetValue(time, out float value))
			{
				value = -1;
			}
			return value;
		}
	}
}
