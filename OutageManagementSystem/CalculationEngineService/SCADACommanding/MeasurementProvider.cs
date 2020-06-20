using CECommon.Interfaces;
using CECommon.Model;
using CECommon.Providers;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.OMS;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.Outage;
using System;
using System.Collections.Generic;

namespace CalculationEngine.SCADAFunctions
{
	public class MeasurementProvider : IMeasurementProvider
    {
        #region Fields
        private readonly ILogger logger = LoggerWrapper.Instance;
		private Dictionary<long, AnalogMeasurement> analogMeasurements;
		private Dictionary<long, DiscreteMeasurement> discreteMeasurements;
		private Dictionary<long, List<long>> elementToMeasurementMap;
		private Dictionary<long, long> measurementToElementMap;

		private Dictionary<long, AnalogMeasurement> tempAnalogMeasurements;
		private Dictionary<long, DiscreteMeasurement> tempDiscreteMeasurements;
		private Dictionary<long, List<long>> tempElementToMeasurementMap;
		private Dictionary<long, long> tempMeasurementToElementMap;

		private ProxyFactory proxyFactory;
		private readonly HashSet<CommandOriginType> ignorableOriginTypes;
        #endregion

        public MeasurementProvider()
		{
			elementToMeasurementMap = new Dictionary<long, List<long>>();
			measurementToElementMap = new Dictionary<long, long>();
			analogMeasurements = new Dictionary<long, AnalogMeasurement>();
			discreteMeasurements = new Dictionary<long, DiscreteMeasurement>();
			ignorableOriginTypes = new HashSet<CommandOriginType>() { /*CommandOriginType.USER_COMMAND,*/ CommandOriginType.ISOLATING_ALGORITHM_COMMAND };

			proxyFactory = new ProxyFactory();
			Provider.Instance.MeasurementProvider = this;
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
			float value = -1;
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
		private void UpdateAnalogMeasurement(long measurementGid, float value, CommandOriginType commandOrigin, AlarmType alarmType)
		{
			if (analogMeasurements.TryGetValue(measurementGid, out AnalogMeasurement measurement))
			{
				measurement.CurrentValue = value;
				measurement.Alarm = alarmType;
			}
			else
			{
				logger.LogWarn($"Failed to update analog measurement with GID 0x{measurementGid.ToString("X16")}. There is no such a measurement.");
			}
		}
		public void UpdateAnalogMeasurement(Dictionary<long, AnalogModbusData> data)
		{
			foreach (long gid in data.Keys)
			{
				AnalogModbusData measurementData = data[gid];

				UpdateAnalogMeasurement(gid, (float)measurementData.Value, measurementData.CommandOrigin, measurementData.Alarm);
			}
			//DiscreteMeasurementDelegate?.Invoke();
		}
		private bool UpdateDiscreteMeasurement(long measurementGid, int value, CommandOriginType commandOrigin)
		{
			bool success = true;
			if (discreteMeasurements.TryGetValue(measurementGid, out DiscreteMeasurement measurement))
			{
				if (value == (ushort)DiscreteCommandingType.CLOSE)
				{
					measurement.CurrentOpen = false;
				}
				else
				{
					measurement.CurrentOpen = true;
				}

				using (ReportPotentialOutageProxy reportPotentialOutageProxy = proxyFactory.CreateProxy<ReportPotentialOutageProxy, IReportPotentialOutageContract>(EndpointNames.ReportPotentialOutageEndpoint))
				{
					if (reportPotentialOutageProxy == null)
					{
						string message = "UpdateDiscreteMeasurement => ReportPotentialOutageProxy is null.";
						logger.LogError(message);
						throw new NullReferenceException(message);
					}

					try
					{
						if (measurement.CurrentOpen)
						{
							reportPotentialOutageProxy.ReportPotentialOutage(measurement.ElementId, commandOrigin);
						}
						else
						{
							reportPotentialOutageProxy.OnSwitchClose(measurement.ElementId);
						}
					}
					catch (Exception e)
					{
						logger.LogError("Failed to report potential outage.", e);
					}
				}

			}
			else
			{
				logger.LogWarn($"Failed to update discrete measurement with GID 0x{measurementGid.ToString("X16")}. There is no such a measurement.");
				success = false;
			}

			if (measurementToElementMap.TryGetValue(measurementGid, out long recloserGid) 
				&& Provider.Instance.ModelProvider.IsRecloser(recloserGid)
				&& commandOrigin != CommandOriginType.CE_COMMAND
				&& commandOrigin != CommandOriginType.OUTAGE_SIMULATOR)
			{
				Provider.Instance.TopologyProvider.ResetRecloser(recloserGid);
			}
			return success;
		}
		public void UpdateDiscreteMeasurement(Dictionary<long, DiscreteModbusData> data)
		{
			List<long> signalGids = new List<long>();
			foreach (long gid in data.Keys)
			{
				DiscreteModbusData measurementData = data[gid];

				if (UpdateDiscreteMeasurement(measurementData.MeasurementGid, measurementData.Value, measurementData.CommandOrigin))
				{
					signalGids.Add(gid);
				}
			}
			DiscreteMeasurementDelegate?.Invoke();
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
        public bool TryGetDiscreteMeasurement(long measurementGid, out DiscreteMeasurement measurement)
		{
			bool success = false;
			if (discreteMeasurements.TryGetValue(measurementGid, out measurement))
			{
				success = true;
			}
			else
			{
				measurement = null;
			}
			return success;
		}
		public bool TryGetAnalogMeasurement(long measurementGid, out AnalogMeasurement measurement)
		{
			bool success = false;
			if (analogMeasurements.TryGetValue(measurementGid, out measurement))
			{
				success = true;
			}
			else
			{
				measurement = null;
			}
			return success;
		}
		public void AddMeasurementElementPair(long measurementId, long elementId)
		{
			if (measurementToElementMap.ContainsKey(measurementId))
			{
				string message = $"Measurement with id 0x{measurementId:X16} already exists in measurement-element mapping.";
				logger.LogError(message);
				throw new ArgumentException(message);
			}

			measurementToElementMap.Add(measurementId, elementId);
			
			if (elementToMeasurementMap.TryGetValue(elementId, out List<long> measurements))
			{
				measurements.Add(measurementId);
			}
			else
			{
				elementToMeasurementMap.Add(elementId, new List<long>() { measurementId });
			}
		}

		#region IMeasurementMapContract
		public List<long> GetMeasurementsOfElement(long elementGid)
		{
			if (elementToMeasurementMap.TryGetValue(elementGid, out var measurements))
			{
				return measurements;
			}
			else
			{
				return new List<long>();
			}
		}
		public Dictionary<long, List<long>> GetElementToMeasurementMap()
		{
			return elementToMeasurementMap;
		}
		public Dictionary<long, long> GetMeasurementToElementMap()
		{
			return measurementToElementMap;
		}
        #endregion

        #region Transaction Manager
		public bool PrepareForTransaction()
		{
			bool success = true;
			try
			{
				tempAnalogMeasurements = new Dictionary<long, AnalogMeasurement>(analogMeasurements);
				tempDiscreteMeasurements = new Dictionary<long, DiscreteMeasurement>(discreteMeasurements);
				tempElementToMeasurementMap = new Dictionary<long, List<long>>(elementToMeasurementMap);
				tempMeasurementToElementMap = new Dictionary<long, long>(measurementToElementMap);

				elementToMeasurementMap = new Dictionary<long, List<long>>();
				measurementToElementMap = new Dictionary<long, long>();
				analogMeasurements = new Dictionary<long, AnalogMeasurement>();
				discreteMeasurements = new Dictionary<long, DiscreteMeasurement>();
			}
			catch (Exception)
			{
				success = false;
			}
			return success;
		}

		public void CommitTransaction()
		{
			tempAnalogMeasurements = null;
			tempDiscreteMeasurements = null;
			tempElementToMeasurementMap = null;
			tempMeasurementToElementMap = null;
		}

		public void RollbackTransaction()
		{
			elementToMeasurementMap = tempElementToMeasurementMap;
			measurementToElementMap = tempMeasurementToElementMap;
			analogMeasurements = tempAnalogMeasurements;
			discreteMeasurements = tempDiscreteMeasurements;
		}
		#endregion
	}
}
