using CECommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CECommon.TopologyConfiguration
{
	public class DailyCurveReader
	{
		private static readonly string path = "DailyLoadCurves.xml";

		public static Dictionary<DailyCurveType, DailyCurve> ReadDailyCurves()
		{
			Dictionary<DailyCurveType, DailyCurve> dailyCurves = new Dictionary<DailyCurveType, DailyCurve>();
			DailyCurve currentCurve = null;
			short currentTime = -1;
			float currentValue = -1;
			DailyCurveConfigProgress dailyCurveConfigProgress = DailyCurveConfigProgress.NewDailyCurve;

			XmlReader xmlreader = XmlReader.Create(path);

			while (xmlreader.Read())
			{
				if (xmlreader.NodeType == XmlNodeType.Element)
				{
					switch (xmlreader.Name)
					{
						case "DailyLoadCurve":
							{
								dailyCurveConfigProgress = DailyCurveConfigProgress.NewDailyCurve;

								if (currentTime != -1 && currentValue != -1)
								{
									currentCurve?.TryAddPair(currentTime, currentValue);
								}

								if (currentCurve != null)
								{
									dailyCurves[currentCurve.DailyCurveType] = currentCurve;
								}

								currentTime = -1;
								currentValue = -1;

								if (Enum.TryParse(xmlreader.GetAttribute("name"), true, out DailyCurveType dailyCurveType))
								{
									currentCurve = new DailyCurve(dailyCurveType);
								}
								else
								{
									currentCurve = null;
								}

								break;
							}
						case "Data":
							{
								if (currentTime != -1 && currentValue != -1)
								{
									currentCurve?.TryAddPair(currentTime, currentValue);
								}
								break;
							}
						case "Value":
							dailyCurveConfigProgress = DailyCurveConfigProgress.Value;
							break;
						case "Time":
							dailyCurveConfigProgress = DailyCurveConfigProgress.Time;
							break;
						default:
							break;
					}
				}
				else if (xmlreader.NodeType == XmlNodeType.Text)
				{
					switch (dailyCurveConfigProgress)
					{
						case DailyCurveConfigProgress.NewDailyCurve:
							break;
						case DailyCurveConfigProgress.Value:
							{
								if (float.TryParse(xmlreader.Value, out float value))
								{
									currentValue = value;
								}
								else
								{
									currentValue = -1;
								}
								break;
							}
						case DailyCurveConfigProgress.Time:
							{
								if (TimeSpan.TryParse(xmlreader.Value, out TimeSpan time))
								{
									currentTime = (short)time.Hours;
								}
								else
								{
									currentTime = -1;
								}
								break;
							}
						default:
							break;
					}
				}
			}

			if (currentTime != -1 && currentValue != -1)
			{
				currentCurve?.TryAddPair(currentTime, currentValue);
			}

			if (currentCurve != null)
			{
				dailyCurves[currentCurve.DailyCurveType] = currentCurve;
			}

			return dailyCurves;
		}
	}
}
