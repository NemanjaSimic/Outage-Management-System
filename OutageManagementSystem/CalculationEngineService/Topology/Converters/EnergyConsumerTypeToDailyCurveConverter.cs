﻿using CECommon;
using Outage.Common;
using System;

namespace Topology.Converters
{
	public class EnergyConsumerTypeToDailyCurveConverter
	{
		public static DailyCurveType GetDailyCurveType(EnergyConsumerType energyConsumerType)
		{
			switch (energyConsumerType)
			{
				case EnergyConsumerType.HOUSEHOLD:
					return DailyCurveType.Household;
				case EnergyConsumerType.SMALL_INDUSTRY:
					return DailyCurveType.SmallIndustry;
				default:
					throw new Exception($"Unknown energy cosnumer type {energyConsumerType}.");
			}
		}
	}
}
