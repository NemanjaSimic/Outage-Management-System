using Common.CE;
using Common.CeContracts;
using OMS.Common.Cloud.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;

namespace CE.LoadFlowImplementation
{
	public class DailyCurveReader
	{
		private const string curvesFileNameSettingKey = "curvesFileNameSettingKey";
		private const string curvesFileFolderPathSettingKey = "curvesFileFolderPathSettingKey";

		private readonly string baseLogString;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public DailyCurveReader()
		{
			baseLogString = $"[DailyCurveReader] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering static Ctor.";
			Logger.LogVerbose(verboseMessage);
		}

		public Dictionary<DailyCurveType, DailyCurve> ReadDailyCurves()
		{
			string verboseMessage = $"{baseLogString} ReadDailyCurves method called.";
			Logger.LogVerbose(verboseMessage);

			Dictionary<DailyCurveType, DailyCurve> dailyCurves = new Dictionary<DailyCurveType, DailyCurve>();
			DailyCurve currentCurve = null;
			short currentTime = -1;
			float currentValue = -1;
			DailyCurveConfigProgress dailyCurveConfigProgress = DailyCurveConfigProgress.NewDailyCurve;

			string fullPath = GetCurvesFilePath();
			Logger.LogDebug($"{baseLogString} ReadDailyCurves => Creating Xml reader for file {fullPath}.");

			using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
				try
				{
					var xmlreader = XmlReader.Create(stream);

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
						$"{baseLogString} ReadDailyCurves => Failed while reading file {fullPath}." +
						$"{Environment.NewLine} Exception message: {e.Message} " +
						$"{Environment.NewLine} Stack race: {e.StackTrace}";
					Logger.LogError(message);
					throw new Exception(message);
				}
			}

			return dailyCurves;
		}

		private string GetCurvesFilePath()
		{
			if (!(ConfigurationManager.AppSettings[curvesFileNameSettingKey] is string curvesFileName))
            {
				throw new Exception($"{baseLogString} GetCurvesFilePath => Key '{curvesFileNameSettingKey}' not found in app settings.");
            }

			var curvesFilePath = curvesFileName;

			if (!File.Exists(curvesFilePath))
			{
				if (!(ConfigurationManager.AppSettings[curvesFileFolderPathSettingKey] is string curvesFileFolderPath))
				{
					throw new Exception($"{baseLogString} GetCurvesFilePath => Key '{curvesFileFolderPathSettingKey}' not found in app settings.");
				}

				curvesFilePath = $@"{curvesFileFolderPath}\{curvesFileName}";

				if(!File.Exists(curvesFilePath))
                {
					throw new Exception($"{baseLogString} GetCurvesFilePath => Dalie Curves file was not found.");
                }
			}

			return curvesFilePath;
		}
	}
}
