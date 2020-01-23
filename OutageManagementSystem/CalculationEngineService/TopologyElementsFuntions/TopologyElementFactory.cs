using CECommon;
using CECommon.Model;
using NetworkModelServiceFunctions;
using Outage.Common;
using System;
using System.Collections.Generic;

namespace TopologyElementsFuntions
{
	public class TopologyElementFactory
	{
		private ILogger logger = LoggerWrapper.Instance;
		public TopologyElement CreateTopologyElement(long gid)
		{
			TopologyElement retVal;
			TopologyType dmsTopologyType = TopologyHelper.Instance.GetElementTopologyType(gid);
	
			if (dmsTopologyType == TopologyType.Edge)
				retVal = new Edge(gid);
			else if (dmsTopologyType == TopologyType.Node)
				retVal = new Node(gid);
			else
			{
				string message = $"Element with GID: {gid.ToString("X")} is neither Edge nor Node. Please check configuration files.";
				logger.LogError(message);
				throw new Exception(message);
			}
			return retVal;
		}

		public Measurement CreateMeasurement(long gid)
		{
			Measurement measurement;
			DMSType dmsTopologyType = TopologyHelper.Instance.GetDMSTypeOfTopologyElement(gid);
			if (dmsTopologyType == DMSType.ANALOG)
			{
				measurement = NMSManager.Instance.GetPopulatedAnalogMeasurement(gid);
			}
			else if (dmsTopologyType == DMSType.DISCRETE)
			{
				measurement = NMSManager.Instance.GetPopulatedDiscreteMeasurement(gid);
			}
			else
			{
				string message = $"Element with GID: {gid.ToString("X")} is neither analog nor discrete measurement. Please check configuration files.";
				logger.LogError(message);
				throw new Exception(message);
			}
			return measurement;
		}
	}
}
