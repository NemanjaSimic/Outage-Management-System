using CECommon;
using CECommon.TopologyConfiguration;
using Outage.Common;
using System.Collections.Generic;

namespace NetworkModelServiceFunctions
{
	public class TopologyHelper
	{
		private ILogger logger = LoggerWrapper.Instance;
		private readonly Dictionary<TopologyStatus, List<DMSType>> elementsStatus;
		private readonly Dictionary<TopologyType, List<DMSType>> topologyTypes;

		#region Singleton
		private static object syncObj = new object();
		private static TopologyHelper instance;
		public static TopologyHelper Instance
		{
			get
			{
				lock (syncObj)
				{
					if (instance == null)
					{
						instance = new TopologyHelper();
					}
				}
				return instance;
			}

		}
		#endregion
		private TopologyHelper()
		{
			ConfigurationParse cp = new ConfigurationParse();
			elementsStatus = cp.GetAllElementStatus();
			topologyTypes = cp.GetAllTopologyTypes();
		}
		public TopologyStatus GetElementTopologyStatus(long gid)
		{
			TopologyStatus retVal;
			DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gid);
			foreach (var item in elementsStatus)
			{
				if (item.Value.Contains(type))
				{
					retVal = item.Key;
					return retVal;
				}
			}
			return TopologyStatus.Regular;
		}
		public TopologyType GetElementTopologyType(long gid)
		{
			TopologyType retVal;
			DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gid);
			foreach (var item in topologyTypes)
			{
				if (item.Value.Contains(type))
				{
					retVal = item.Key;
					return retVal;
				}
			}
			return TopologyType.None;
		}
	}
}
