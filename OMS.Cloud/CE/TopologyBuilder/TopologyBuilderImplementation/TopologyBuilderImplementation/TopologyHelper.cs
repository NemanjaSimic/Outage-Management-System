using CE.TopologyBuilderImplementation.Configuration;
using CECommon;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using System;
using System.Collections.Generic;

namespace CE.TopologyBuilderImplementation
{
    class TopologyHelper
	{
		private readonly Dictionary<TopologyStatus, List<DMSType>> elementsStatus;
		private readonly Dictionary<TopologyType, List<DMSType>> topologyTypes;
		
		private readonly string baseLogString;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

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
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);

			ConfigurationParse cp = new ConfigurationParse();
			elementsStatus = cp.GetAllElementStatus();
			topologyTypes = cp.GetAllTopologyTypes();
		}
		public TopologyStatus GetElementTopologyStatus(long gid)
		{
			string verboseMessage = $"{baseLogString} GetElementTopologyStatus method called for GID {gid:X16}.";
			Logger.LogVerbose(verboseMessage);

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
			string verboseMessage = $"{baseLogString} GetElementTopologyType method called for GID {gid:X16}.";
			Logger.LogVerbose(verboseMessage);

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
