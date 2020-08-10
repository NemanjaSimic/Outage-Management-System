using Common.CE;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using System;
using System.Collections.Generic;

namespace CE.TopologyBuilderImplementation.Configuration
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

		private readonly string baseLogString;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public ConfigurationParse()
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);
		}

		private List<DMSType> ParseConfigFile(string path)
		{
			string verboseMessage = $"{baseLogString} ParseConfigFile method called for file {path}.";
			Logger.LogVerbose(verboseMessage);

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
				catch (Exception)
				{
					string message = $"{baseLogString} ParseConfigFile => Failed to parse [{item}] from configuration file {path}.";
					Logger.LogError(message);
					throw new Exception(message);
				}
			}
			return retValue;
		}
		public Dictionary<TopologyStatus, List<DMSType>> GetAllElementStatus()
		{
			string verboseMessage = $"{baseLogString} GetAllElementStatus method called.";
			Logger.LogVerbose(verboseMessage);

			Dictionary<TopologyStatus, List<DMSType>> elements = new Dictionary<TopologyStatus, List<DMSType>>
			{
				{ TopologyStatus.Ignorable, new List<DMSType>(ParseConfigFile(ignorableFilePath)) },
				{ TopologyStatus.Field, new List<DMSType>(ParseConfigFile(fieldFilePath)) }
			};
			return elements;
		}
		public Dictionary<TopologyType, List<DMSType>> GetAllTopologyTypes()
		{
			string verboseMessage = $"{baseLogString} GetAllTopologyTypes method called.";
			Logger.LogVerbose(verboseMessage);

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
