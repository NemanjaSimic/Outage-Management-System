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

		private static IAlarmConfigData ImportAppSettings()
		{
			ICloudLogger logger = CloudLoggerFactory.GetLogger();
			AlarmConfigData data = new AlarmConfigData();

			if (ConfigurationManager.AppSettings["LowPowerLimit"] is string lowPowerLimitSetting)
			{
				if (float.TryParse(lowPowerLimitSetting, out float lowPowerLimit))
				{
					data.LowPowerLimit = lowPowerLimit;
				}
				else
				{
					string message = "LowPowerLimit in Alarm configuration is either not defined or not valid.";
					logger.LogError(message);
					throw new Exception(message);
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
					string message = "HighPowerLimit in Alarm configuration is either not defined or not valid.";
					logger.LogError(message);
					throw new Exception(message);
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
					string message = "LowVoltageLimit in Alarm configuration is either not defined or not valid.";
					logger.LogError(message);
					throw new Exception(message);
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
					string message = "HighVoltageLimit in Alarm configuration is either not defined or not valid.";
					logger.LogError(message);
					throw new Exception(message);
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
					string message = "LowCurrentLimit in Alarm configuration is either not defined or not valid.";
					logger.LogError(message);
					throw new Exception(message);
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
					string message = "HighCurrentLimit in Alarm configuration is either not defined or not valid.";
					logger.LogError(message);
					throw new Exception(message);
				}
			}

			return data;
		}
	}
}
