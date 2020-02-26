using Outage.Common;
using System;
using System.Collections.Generic;

namespace CECommon.TopologyConfiguration
{
	public class ConfigurationParse
	{
        #region ConfigurationFiles
        private readonly string ignorableFilePath ="ignorable.txt";
		private readonly string fieldFilePath ="field.txt";
		private readonly string nodeFilePath = "node.txt";
		private readonly string edgeFilePath = "edge.txt";
		private readonly string measurementFilePath = "measurement.txt";
		#endregion

		private ILogger logger = LoggerWrapper.Instance;
        private List<DMSType> ParseConfigFile(string path)
		{
			string[] elements = Config.Instance.ReadConfiguration(path).Split(';');
			List<DMSType> retValue = new List<DMSType>();
			foreach (var item in elements)
			{
				DMSType type;
				try
				{
					type = (DMSType)Enum.Parse(typeof(DMSType), item);
					retValue.Add(type);
				}
				catch (Exception ex)
				{
					logger.LogError($"Failed to parse configuration file on path {path}. Exception message: {ex.Message}");
					throw ex;
				}
			}
			return retValue;
		}
		public Dictionary<TopologyStatus, List<DMSType>> GetAllElementStatus()
		{
			Dictionary<TopologyStatus, List<DMSType>> elements = new Dictionary<TopologyStatus, List<DMSType>>
			{
				{ TopologyStatus.Ignorable, new List<DMSType>(ParseConfigFile(ignorableFilePath)) },
				{ TopologyStatus.Field, new List<DMSType>(ParseConfigFile(fieldFilePath)) }
			};
			return elements;
		}
		public Dictionary<TopologyType, List<DMSType>> GetAllTopologyTypes()
		{
			Dictionary<TopologyType, List<DMSType>> elements = new Dictionary<TopologyType, List<DMSType>>
			{
				{TopologyType.Node, new List<DMSType>(ParseConfigFile(nodeFilePath))},
				{TopologyType.Edge, new List<DMSType>(ParseConfigFile(edgeFilePath))},
				{TopologyType.Measurement, new List<DMSType>(ParseConfigFile(measurementFilePath))}
			};
			return elements;
		}
	}
}
