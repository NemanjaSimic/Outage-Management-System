using Common.CE;
using Common.CeContracts;
using OMS.Common.Cloud.Logger;
using System;
using System.Collections.Generic;
using System.Xml;

namespace CE.LoadFlowImplementation
{
	public class DailyCurveReader
	{
		private static readonly string path = "DailyLoadCurves.xml";

		private static readonly string baseLogString;

		private static ICloudLogger logger;
		private static ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		static DailyCurveReader()
		{
			baseLogString = $"[DailyCurveReader] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering static Ctor.";
			Logger.LogVerbose(verboseMessage);
		}
		public static Dictionary<DailyCurveType, DailyCurve> ReadDailyCurves()
		{
			string verboseMessage = $"{baseLogString} ReadDailyCurves method called.";
			Logger.LogVerbose(verboseMessage);

			Dictionary<DailyCurveType, DailyCurve> dailyCurves = new Dictionary<DailyCurveType, DailyCurve>();
			DailyCurve currentCurve = null;
			short currentTime = -1;
			float currentValue = -1;
			DailyCurveConfigProgress dailyCurveConfigProgress = DailyCurveConfigProgress.NewDailyCurve;

			Logger.LogDebug($"{baseLogString} ReadDailyCurves => Creating Xml reader for file {path}.");
			XmlReader xmlreader = XmlReader.Create(path);

			try
			{
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
			}
			catch (Exception e)
			{
				string message = 
					$"{baseLogString} ReadDailyCurves => Failed while reading file {path}." +
					$"{Environment.NewLine} Exception message: {e.Message} " +
					$"{Environment.NewLine} Stack race: {e.StackTrace}";
				Logger.LogError(message);
				throw new Exception(message);
			}
			return dailyCurves;
		}
	}
}
