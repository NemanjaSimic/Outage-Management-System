using CECommon;
using CECommon.TopologyConfiguration;
using Outage.Common;
using System.Collections.Generic;

namespace TopologyElementsFuntions
{
	public class TopologyHelper
	{
		private readonly Dictionary<TopologyStatus, List<DMSType>> elementsStatus;
		private readonly Dictionary<TopologyType, List<DMSType>> topologyTypes;

		private GDAModelHelper gDAModelHelper = new GDAModelHelper();
		private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();

		public TopologyHelper()
		{
			ConfigurationParse cp = new ConfigurationParse();
			elementsStatus = cp.GetAllElementStatus();
			topologyTypes = cp.GetAllTopologyTypes();
		}

		public TopologyStatus GetElementTopologyStatus(long gid)
		{
			TopologyStatus retVal;
			DMSType type = ModelCodeHelper.GetTypeFromModelCode(modelResourcesDesc.GetModelCodeFromId(gid));
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
			DMSType type = ModelCodeHelper.GetTypeFromModelCode(modelResourcesDesc.GetModelCodeFromId(gid));
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

		public List<long> GetAllReferencedElements(long gid)
		{
			List<long> elements = new List<long>();

			foreach (var resourceDescription in gDAModelHelper.GetAllReferencedElements(gid))
			{
				elements.Add(resourceDescription.Id);
			}

			return elements;
		}

		public List<long> GetNextNonIgnorableElements(long gid)
		{
			List<long> nonIgnorableElements = new List<long>();

			if (GetElementTopologyStatus(gid) != TopologyStatus.Ignorable)
			{
				nonIgnorableElements.Add(gid);
			}
			else
			{
				List<long> nextElements = GetAllReferencedElements(gid);
				if (nextElements.Count != 0)
				{
					foreach (var element in nextElements)
					{

					}
				}
			}

			return nonIgnorableElements;
		}
	}
}
