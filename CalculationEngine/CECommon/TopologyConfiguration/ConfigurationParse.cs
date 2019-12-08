using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.TopologyConfiguration
{
	public class ConfigurationParse
	{
		private readonly string ignorableFilePath ="ignorables.txt";
		private readonly string fieldFilePath ="fields.txt";
		private readonly string nodeFilePath = "nodes.txt";
		private readonly string edgeFilePath = "edges.txt";

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
				catch (Exception)
				{
					Console.WriteLine("Invalid structured configuration file " + path + ".");
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
				{TopologyType.Edge, new List<DMSType>(ParseConfigFile(edgeFilePath))}
			};

			return elements;
		}

	}
}
