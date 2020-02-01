using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADACommanding
{
    public class SCADACommandingCache
    {
		private ILogger logger = LoggerWrapper.Instance;
		private Dictionary<long, AnalogMeasurementInforamtion> analogMeasurements;
		private Dictionary<long, DiscreteMeasurementInforamtion> discreteMeasurements;
		private Dictionary<long, AnalogMeasurementInforamtion> AnalogMeasurements
		{
			get
			{
				return analogMeasurements;
			}
			set
			{
				analogMeasurements = value;
			}
		}
		private Dictionary<long, DiscreteMeasurementInforamtion> DiscreteMeasurements
		{
			get
			{
				return discreteMeasurements;
			}
			set
			{
				discreteMeasurements = value;
				//Event napraviti			
			}
		}

		#region Singleton
		private static object syncObj = new object();
		private static SCADACommandingCache instance;

		public static SCADACommandingCache Instance
		{
			get
			{
				lock (syncObj)
				{
					if (instance == null)
					{
						instance = new SCADACommandingCache();
					}
				}
				return instance;
			}
		}
		#endregion
		private SCADACommandingCache()
        {
			AnalogMeasurements = new Dictionary<long, AnalogMeasurementInforamtion>();
			DiscreteMeasurements = new Dictionary<long, DiscreteMeasurementInforamtion>();
		}

		public void AddAnalogMeasurement(long measuerementId, long elementId, float value)
		{
			if (!AnalogMeasurements.ContainsKey(measuerementId))
			{
				AnalogMeasurements.Add(measuerementId, new AnalogMeasurementInforamtion(elementId, value));
			}
			else
			{
				logger.LogWarn($"[SCADACommandCache] Failed to add analog measurement with GID {measuerementId} to element with GID {elementId}. Measurement already exists.");
			}
		}
		public void AddDiscreteMeasurement(long measuerementId, long elementId, ushort value)
		{
			if (!DiscreteMeasurements.ContainsKey(measuerementId))
			{
				DiscreteMeasurements.Add(measuerementId, new DiscreteMeasurementInforamtion(elementId, value));
			}
			else
			{
				logger.LogWarn($"[SCADACommandCache] Failed to add discrete measurement with GID {measuerementId} to element with GID {elementId}. Measurement already exists.");
			}
		}
		public void UpdateAnalogMeasurement(long measurementGid, float value)
		{
			if (AnalogMeasurements.ContainsKey(measurementGid))
			{
				AnalogMeasurementInforamtion info = AnalogMeasurements[measurementGid];
				info.value = value;
				AnalogMeasurements[measurementGid] = info;
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
			if (DiscreteMeasurements.ContainsKey(measurementGid))
			{
				DiscreteMeasurementInforamtion info = DiscreteMeasurements[measurementGid];
				info.value = value;
				DiscreteMeasurements[measurementGid] = info;
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
	}

	struct AnalogMeasurementInforamtion
	{
		public long elementGid;
		public float value;
		public AnalogMeasurementInforamtion(long elementGid, float value)
		{
			this.elementGid = elementGid;
			this.value = value;
		}
		
	}

	struct DiscreteMeasurementInforamtion
	{
		public long elementGid;
		public ushort value;
		public DiscreteMeasurementInforamtion(long elementGid, ushort value)
		{
			this.elementGid = elementGid;
			this.value = value;
		}
		
	}
}
