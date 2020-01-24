using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADAData.Configuration
{
    public class AlarmConfigData
    {

		public ushort LowPowerLimit { get; set; }
		public ushort HighPowerLimit { get; set; }
		public ushort LowVoltageLimit { get; set; }
		public ushort HighVolageLimit { get; set; }

		#region Instance

		private static AlarmConfigData _instance;

		public static AlarmConfigData Instance
		{
			get {	
					if(_instance == null)
					{
						_instance = new AlarmConfigData();
					}
					return _instance;
				}
		}
		#endregion

		private AlarmConfigData()
		{
			ImportAppSettings();
		}

		private void ImportAppSettings()
		{
			if(ConfigurationManager.AppSettings["LowPowerLimit"] is string lowPowerLimitSetting)
			{
				if(ushort.TryParse(lowPowerLimitSetting,out ushort lowPowerLimit))
				{
					LowPowerLimit = lowPowerLimit;
				}

			}
			if (ConfigurationManager.AppSettings["HighPowerLimit"] is string highPowerLimitSetting)
			{
				if (ushort.TryParse(highPowerLimitSetting, out ushort highPowerLimit))
				{
					HighPowerLimit = highPowerLimit;
				}

			}
			if (ConfigurationManager.AppSettings["LowVoltageLimit"] is string lowVoltageLimitSetting)
			{
				if (ushort.TryParse(lowVoltageLimitSetting, out ushort lowVolageLimit))
				{
					LowVoltageLimit = lowVolageLimit;
				}

			}
			if(ConfigurationManager.AppSettings["HighVoltageLimit"] is string highVoltageLimitSetting)
			{
				if(ushort.TryParse(highVoltageLimitSetting,out ushort highVoltageLimit))
				{
					HighVolageLimit = highVoltageLimit;
				}
			}
		}
	}
}
