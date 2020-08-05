using OMS.Common.Cloud.Logger;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using System;
using System.Configuration;

namespace SCADA.ModelProviderImplementation.Helpers
{
    internal class AlarmConfigDataHelper
	{
		private static readonly object lockSync = new object();
		private static IAlarmConfigData alarmConfigData;
		
		public static IAlarmConfigData GetAlarmConfigData()
		{
			if(alarmConfigData == null)
			{
				lock(lockSync)
				{
					if(alarmConfigData == null)
					{
						alarmConfigData = ImportAppSettings();
					}
				}
			}

			return alarmConfigData;
		}

		private static ICloudLogger logger;
		private static ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		private static IAlarmConfigData ImportAppSettings()
		{
			string baseLogString = $"{typeof(AlarmConfigDataHelper)} [static] =>";

			var data = new AlarmConfigData();

			if (ConfigurationManager.AppSettings["LowPowerLimit"] is string lowPowerLimitSetting)
			{
				if (float.TryParse(lowPowerLimitSetting, out float lowPowerLimit))
				{
					data.LowPowerLimit = lowPowerLimit;
				}
				else
				{
					string errorMessage = $"{baseLogString} ImportAppSettings => LowPowerLimit in Alarm configuration is either not defined or not valid.";
					Logger.LogError(errorMessage);
					throw new Exception(errorMessage);
				}
			}

			if (ConfigurationManager.AppSettings["HighPowerLimit"] is string highPowerLimitSetting)
			{
				if (float.TryParse(highPowerLimitSetting, out float highPowerLimit))
				{
					data.HighPowerLimit = highPowerLimit;
				}
				else
				{
					string errorMessage = $"{baseLogString} ImportAppSettings => HighPowerLimit in Alarm configuration is either not defined or not valid.";
					Logger.LogError(errorMessage);
					throw new Exception(errorMessage);
				}
			}

			if (ConfigurationManager.AppSettings["LowVoltageLimit"] is string lowVoltageLimitSetting)
			{
				if (float.TryParse(lowVoltageLimitSetting, out float lowVolageLimit))
				{
					data.LowVoltageLimit = lowVolageLimit;
				}
				else
				{
					string errorMessage = $"{baseLogString} ImportAppSettings => LowVoltageLimit in Alarm configuration is either not defined or not valid.";
					Logger.LogError(errorMessage);
					throw new Exception(errorMessage);
				}
			}

			if (ConfigurationManager.AppSettings["HighVoltageLimit"] is string highVoltageLimitSetting)
			{
				if (float.TryParse(highVoltageLimitSetting, out float highVoltageLimit))
				{
					data.HighVolageLimit = highVoltageLimit;
				}
				else
				{
					string errorMessage = $"{baseLogString} ImportAppSettings => HighVoltageLimit in Alarm configuration is either not defined or not valid.";
					Logger.LogError(errorMessage);
					throw new Exception(errorMessage);
				}
			}

			if (ConfigurationManager.AppSettings["LowFeederCurrentLimit"] is string LowFeederCurrentLimitSetting)
			{
				if (float.TryParse(LowFeederCurrentLimitSetting, out float lowFeederCurrentLimit))
				{
					data.LowFeederCurrentLimit = lowFeederCurrentLimit;
				}
				else
				{
					string errorMessage = $"{baseLogString} ImportAppSettings => LowFeederCurrentLimit in Alarm configuration is either not defined or not valid.";
					Logger.LogError(errorMessage);
					throw new Exception(errorMessage);
				}
			}

			if (ConfigurationManager.AppSettings["HighFeederCurrentLimit"] is string highFeederCurrentLimitSetting)
			{
				if (float.TryParse(highFeederCurrentLimitSetting, out float highFeederCurrentLimit))
				{
					data.HighFeederCurrentLimit = highFeederCurrentLimit;
				}
				else
				{
					string errorMessage = $"{baseLogString} ImportAppSettings => HighFeederCurrentLimit in Alarm configuration is either not defined or not valid.";
					Logger.LogError(errorMessage);
					throw new Exception(errorMessage);
				}
			}

			if (ConfigurationManager.AppSettings["LowCurrentLimit"] is string lowCurrentLimitSetting)
			{
				if (float.TryParse(lowCurrentLimitSetting, out float lowCurrentLimit))
				{
					data.LowCurrentLimit = lowCurrentLimit;
				}
				else
				{
					string errorMessage = $"{baseLogString} ImportAppSettings => LowCurrentLimit in Alarm configuration is either not defined or not valid.";
					Logger.LogError(errorMessage);
					throw new Exception(errorMessage);
				}
			}

			if (ConfigurationManager.AppSettings["HighCurrentLimit"] is string highCurrentLimitSetting)
			{
				if (float.TryParse(highCurrentLimitSetting, out float highCurrentLimit))
				{
					data.HighCurrentLimit = highCurrentLimit;
				}
				else
				{
					string errorMessage = $"{baseLogString} ImportAppSettings => HighCurrentLimit in Alarm configuration is either not defined or not valid.";
					Logger.LogError(errorMessage);
					throw new Exception(errorMessage);
				}
			}

			string infoMessage = $"{baseLogString} ImportAppSettings => Alarm config data Imported.";
			Logger.LogInformation(infoMessage);

			string debugMessage = $"{baseLogString} ImportAppSettings => CurrentLimits: [{data.LowCurrentLimit}, {data.HighCurrentLimit}], PowerLimits: [{data.LowPowerLimit}, {data.HighPowerLimit}], VoltageLimits: [{data.LowVoltageLimit}, {data.HighVolageLimit}].";
			Logger.LogDebug(debugMessage);

			return data;
		}
	}
}
