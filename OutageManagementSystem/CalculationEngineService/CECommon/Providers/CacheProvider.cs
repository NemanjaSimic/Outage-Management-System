using CECommon.Interfaces;
using CECommon.Model;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.OMS;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.Outage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
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
		private Dictionary<long, List<long>> elementToMeasurementMap;
		private Dictionary<long, long> measurementToElementMap;
		private ProxyFactory proxyFactory; 

		public CacheProvider()
		{
			elementToMeasurementMap = new Dictionary<long, List<long>>();
			measurementToElementMap = new Dictionary<long, long>();
			analogMeasurements = new Dictionary<long, AnalogMeasurement>();
			//analogMeasurements = new Dictionary<long, AnalogMeasurementInfo>();
			discreteMeasurements = new Dictionary<long, DiscreteMeasurement>();
			//discreteMeasurements = new Dictionary<long, DiscreteMeasurementInfo>();

			proxyFactory = new ProxyFactory();
			Provider.Instance.CacheProvider = this;
		}

		public DiscreteMeasurementDelegate DiscreteMeasurementDelegate { get; set; }
		public void AddAnalogMeasurement(AnalogMeasurement analogMeasurement)
		{
			if (!analogMeasurements.ContainsKey(analogMeasurement.Id))
			{
				analogMeasurements.Add(analogMeasurement.Id, analogMeasurement);

				if (elementToMeasurementMap.TryGetValue(analogMeasurement.ElementId, out var measurements))
				{
					measurements.Add(analogMeasurement.Id);
				}
				else
				{
					elementToMeasurementMap.Add(
						analogMeasurement.ElementId,
						new List<long>()
						{
							analogMeasurement.Id
						});
				}

				if (!measurementToElementMap.ContainsKey(analogMeasurement.Id))
				{
					measurementToElementMap.Add(analogMeasurement.Id, analogMeasurement.ElementId);
				}
				else
				{
					//TOOD: log err/warn?
				}
			}
		}
		public void AddDiscreteMeasurement(DiscreteMeasurement discreteMeasurement)
		{
			if (!discreteMeasurements.ContainsKey(discreteMeasurement.Id))
			{
				discreteMeasurements.Add(discreteMeasurement.Id, discreteMeasurement);
				
				if (elementToMeasurementMap.TryGetValue(discreteMeasurement.ElementId, out var measurements))
				{
					measurements.Add(discreteMeasurement.Id);
				}
				else
				{
					elementToMeasurementMap.Add(
						discreteMeasurement.ElementId,
						new List<long>()
						{
							discreteMeasurement.Id
						});
				}

				if(!measurementToElementMap.ContainsKey(discreteMeasurement.Id))
				{
					measurementToElementMap.Add(discreteMeasurement.Id, discreteMeasurement.ElementId);
				}
				else
				{
					//TOOD: log err/warn?
				}
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
			DiscreteMeasurement measurement = discreteMeasurements[measurementGid] as DiscreteMeasurement;
			if (value == 0)
			{
				measurement.CurrentOpen = false;
			}
			else
			{
				if (!measurement.CurrentOpen)
				{
					using (OutageServiceProxy outageProxy = proxyFactory.CreateProxy<OutageServiceProxy, IOutageContract>(EndpointNames.OutageServiceEndpoint))
					{
						if (outageProxy == null)
						{
							string message = "UpdateDiscreteMeasurement => OutageServiceProxy is null.";
							logger.LogError(message);
							throw new NullReferenceException(message);
						}

						try
						{
							outageProxy.ReportOutage(measurement.ElementId);
						}
						catch (Exception e)
						{
							logger.LogError("Failed to report outage.", e);
						}
					}
				}
				measurement.CurrentOpen = true;
			}
			discreteMeasurements[measurementGid] = measurement;
		}
		public void UpdateDiscreteMeasurement(Dictionary<long, DiscreteModbusData> data)
		{
			List<long> signalGids = new List<long>();
			foreach (var measurementData in data)
			{
				if (discreteMeasurements.TryGetValue(measurementData.Key, out DiscreteMeasurement measurement))
				{
					signalGids.Add(measurement.ElementId);
					UpdateDiscreteMeasurement(measurementData.Key, measurementData.Value.Value);
				}
				else
				{
					logger.LogError($"Failed to update discrete measurement with GID {measurementData.Key}. Measurement does not exists.");
				}
			}
			DiscreteMeasurementDelegate?.Invoke(signalGids);
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

		public List<long> GetMeasurementsForElement(long elementGid)
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
				logger.LogDebug($"Discrete measurement with GID {measurementGid.ToString("X")} does not exist.");
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
				logger.LogDebug($"Aalog measurement with GID {measurementGid.ToString("X")} does not exist.");
			}
			return success;
		}
	}
}
