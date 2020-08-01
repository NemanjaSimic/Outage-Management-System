namespace CECommon
{
	public enum TopologyType
	{
		Node = 1,
		Edge,
		Measurement,
		None
	}

	public enum TopologyStatus
	{
		Ignorable = 1,
		Field,
		Regular
	}

	public enum TransactionFlag
	{
		InTransaction = 1,
		NoTransaction
	}

	public enum DailyCurveType
	{
		Household = 0,
		SmallIndustry
	}

	public enum DailyCurveConfigProgress
	{
		NewDailyCurve = 0,
		Value,
		Time
	}
}
