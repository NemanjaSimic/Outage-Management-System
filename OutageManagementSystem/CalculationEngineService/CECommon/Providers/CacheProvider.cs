using CECommon.Interfaces;
using CECommon.Model;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Providers
{
    public class CacheProvider : ICacheProvider
    {
		private ILogger logger = LoggerWrapper.Instance;
		private Dictionary<long, AnalogMeasurement> analogMeasurements;
		//private Dictionary<long, AnalogMeasurementInfo> analogMeasurements;
		private Dictionary<long, DiscreteMeasurement> discreteMeasurements;
		//private Dictionary<long, DiscreteMeasurementInfo> discreteMeasurements;
		public CacheProvider()
		{
			analogMeasurements = new Dictionary<long, AnalogMeasurement>();
			//analogMeasurements = new Dictionary<long, AnalogMeasurementInfo>();
			discreteMeasurements = new Dictionary<long, DiscreteMeasurement>();
			//discreteMeasurements = new Dictionary<long, DiscreteMeasurementInfo>();
			Provider.Instance.CacheProvider = this;
        }

		public DiscreteMeasurementDelegate DiscreteMeasurementDelegate { get; set; }
		public void AddAnalogMeasurement(AnalogMeasurement analogMeasurement)
		{
			if (!analogMeasurements.ContainsKey(analogMeasurement.Id))
			{
				analogMeasurements.Add(analogMeasurement.Id, analogMeasurement);
			}
		}
		public void AddDiscreteMeasurement(DiscreteMeasurement discreteMeasurement)
		{
			if (!discreteMeasurements.ContainsKey(discreteMeasurement.Id))
			{
				discreteMeasurements.Add(discreteMeasurement.Id, discreteMeasurement);
			}
		}
		public float GetAnalogValue(long measurementGid)
		{
			float value = 0;
			if (analogMeasurements.ContainsKey(measurementGid))
			{
				value = analogMeasurements[measurementGid].CurrentValue;
			}

			return value;
		}
		public bool GetDiscreteValue(long measurementGid)
		{
			bool isOpen = false;
			if (discreteMeasurements.ContainsKey(measurementGid))
			{
				isOpen = discreteMeasurements[measurementGid].CurrentOpen;			
			}
			return isOpen;
		}
		public void UpdateAnalogMeasurement(long measurementGid, float value)
		{
			if (analogMeasurements.ContainsKey(measurementGid))
			{
				AnalogMeasurement measurement = analogMeasurements[measurementGid] as AnalogMeasurement;
				measurement.CurrentValue = value;
				analogMeasurements[measurementGid] = measurement;
			}
			else
			{
				logger.LogWarn($"Failed to update analog measurement with GID {measurementGid}. There is no such a measurement.");
			}
		}
		public void UpdateAnalogMeasurement(Dictionary<long, AnalogModbusData> data)
		{
			foreach (var measurement in data)
			{
				UpdateAnalogMeasurement(measurement.Key, (float)measurement.Value.Value);
			}
		}
		public void UpdateDiscreteMeasurement(long measurementGid, ushort value)
		{
			if (discreteMeasurements.ContainsKey(measurementGid))
			{
				DiscreteMeasurement measurement = discreteMeasurements[measurementGid] as DiscreteMeasurement;
				if (value == 0)
				{
					measurement.CurrentOpen = false;
				}
				else
				{
					measurement.CurrentOpen = true;
				}
				discreteMeasurements[measurementGid] = measurement;
				DiscreteMeasurementDelegate?.Invoke(measurement.ElementId);
			}
			else
			{
				logger.LogWarn($"Failed to update analog measurement with GID {measurementGid}. There is no such a measurement.");
			}
		}
		public void UpdateDiscreteMeasurement(Dictionary<long, DiscreteModbusData> data)
		{
			foreach (var measurement in data)
			{
				UpdateDiscreteMeasurement(measurement.Key, measurement.Value.Value);
			}
		}

		public long GetElementGidForMeasurement(long measurementGid)
		{
			long signalGid = 0;
			if (discreteMeasurements.TryGetValue(measurementGid, out DiscreteMeasurement measurement))
			{
				signalGid = measurement.ElementId;
			}
			return signalGid;
		}

		struct AnalogMeasurementInfo
		{
			public long Gid;
			public long ElementGid;
			public float Value;
			public AnalogMeasurementInfo(long Gid, long ElementGid, float Value)
			{
				this.Gid = Gid;
				this.ElementGid = ElementGid;
				this.Value = Value;
			}
		}

		struct DiscreteMeasurementInfo
		{
			public long Gid;
			public long ElementGid;
			public bool IsOpen;
			public DiscreteMeasurementInfo(long Gid, long ElementGid, bool IsOpen)
			{
				this.Gid = Gid;
				this.ElementGid = ElementGid;
				this.IsOpen = IsOpen;
			}
		}
	}
}
