using CECommon.Model;
using Common.CE;
using Common.CeContracts;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.SCADA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeasurementProviderImplementation
{
	public class MeasurementProvider : IMeasurementProviderContract
	{
		#region Fields
		private ReliableDictionaryAccess<short, Dictionary<long, AnalogMeasurement>> analogMeasurementsCache;
		public ReliableDictionaryAccess<short, Dictionary<long, AnalogMeasurement>> AnalogMeasurementsCache { get => analogMeasurementsCache; }

		private ReliableDictionaryAccess<short, Dictionary<long, DiscreteMeasurement>> discreteMeasurementsCache;
		public ReliableDictionaryAccess<short, Dictionary<long, DiscreteMeasurement>> DiscreteMeasurementsCache { get => discreteMeasurementsCache; }

		private ReliableDictionaryAccess<short, Dictionary<long, List<long>>> elementToMeasurementMapCache;
		public ReliableDictionaryAccess<short, Dictionary<long, List<long>>> ElementToMeasurementMapCache { get => elementToMeasurementMapCache; }

		private ReliableDictionaryAccess<short, Dictionary<long, long>> measurementToElementMapCache;
		public ReliableDictionaryAccess<short, Dictionary<long, long>> MeasurementToElementMapCache { get => measurementToElementMapCache; }


		private readonly HashSet<CommandOriginType> ignorableOriginTypes;
		#endregion

		private readonly string baseLogString;
		private readonly IReliableStateManager stateManager;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		private readonly TopologyProviderClient topologyProviderClient;
		private readonly ModelProviderClient modelProviderClient;
		private readonly ScadaCommandingClient scadaCommandingClient;

		public MeasurementProvider(IReliableStateManager stateManager)
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);

			topologyProviderClient = TopologyProviderClient.CreateClient();
			modelProviderClient = ModelProviderClient.CreateClient();
			scadaCommandingClient = ScadaCommandingClient.CreateClient();

			this.stateManager = stateManager;
			stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;

			ignorableOriginTypes = new HashSet<CommandOriginType>() 
			{ 
				/*CommandOriginType.USER_COMMAND,*/ 
				CommandOriginType.ISOLATING_ALGORITHM_COMMAND 
			};

			string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
			Logger.LogDebug(debugMessage);
		}

		private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
		{
			if (e.Action == NotifyStateManagerChangedAction.Add)
			{
				var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
				string reliableStateName = operation.ReliableState.Name.AbsolutePath;

				if (reliableStateName == ReliableDictionaryNames.AnalogMeasurementsCache)
				{
					analogMeasurementsCache = await ReliableDictionaryAccess<short, Dictionary<long, AnalogMeasurement>>.Create(stateManager, ReliableDictionaryNames.AnalogMeasurementsCache);
					//this.isElementCacheInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.AnalogMeasurementsCache}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.DiscreteMeasurementsCache)
				{
					discreteMeasurementsCache = await ReliableDictionaryAccess<short, Dictionary<long, DiscreteMeasurement>>.Create(stateManager, ReliableDictionaryNames.DiscreteMeasurementsCache);
					//this.isElementConnectionCacheInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.DiscreteMeasurementsCache}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.ElementsToMeasurementMapCache)
				{
					elementToMeasurementMapCache = await ReliableDictionaryAccess<short, Dictionary<long, List<long>>>.Create(stateManager, ReliableDictionaryNames.ElementsToMeasurementMapCache);
					//this.isEnergySourceCacheInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.ElementsToMeasurementMapCache}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.MeasurementsToElementMapCache)
				{
					measurementToElementMapCache = await ReliableDictionaryAccess<short, Dictionary<long, long>>.Create(stateManager, ReliableDictionaryNames.MeasurementsToElementMapCache);
					//this.isRecloserCacheInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.MeasurementsToElementMapCache}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}

			}
		}
		public async Task AddAnalogMeasurement(AnalogMeasurement analogMeasurement)
		{
			string verboseMessage = $"{baseLogString} entering AddAnalogMeasurement method.";
			Logger.LogVerbose(verboseMessage);

			if (analogMeasurement == null)
			{
				string message = $"{baseLogString} AddAnalogMeasurement => analog measurement parameter is null.";
				Logger.LogError(message);
				throw new Exception(message);
			}

			var analogMeasurements = await GetAnalogMeasurementsFromCache();

			if (!analogMeasurements.ContainsKey(analogMeasurement.Id))
			{
				analogMeasurements.Add(analogMeasurement.Id, analogMeasurement);
			}
			else
			{
				Logger.LogWarning($"{baseLogString} AddAnalogMeasurement => analog measurement with GID {analogMeasurement.Id:X16} is already in collection.");
			}

			await AnalogMeasurementsCache.SetAsync((short)MeasurementPorviderCacheType.Origin, analogMeasurements);
		}
		public async Task AddDiscreteMeasurement(DiscreteMeasurement discreteMeasurement)
		{
			string verboseMessage = $"{baseLogString} entering AddDiscreteMeasurement method.";
			Logger.LogVerbose(verboseMessage);

			if (discreteMeasurement == null)
			{
				string message = $"{baseLogString} AddDiscreteMeasurement => discrete measurement parameter is null.";
				Logger.LogError(message);
				throw new Exception(message);
			}

			var discreteMeasurements = await GetDiscreteMeasurementsFromCache();

			if (!discreteMeasurements.ContainsKey(discreteMeasurement.Id))
			{
				discreteMeasurements.Add(discreteMeasurement.Id, discreteMeasurement);
			}
			else
			{
				Logger.LogWarning($"{baseLogString} AddDiscreteMeasurement => discrete measurement with GID {discreteMeasurement.Id:X16} is already in collection.");
			}

			await DiscreteMeasurementsCache.SetAsync((short)MeasurementPorviderCacheType.Origin, discreteMeasurements);
		}
		public async Task<float> GetAnalogValue(long measurementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetAnalogValue method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			var analogMeasurements = await GetAnalogMeasurementsFromCache();

			float value = -1;
			if (analogMeasurements.ContainsKey(measurementGid))
			{
				value = analogMeasurements[measurementGid].CurrentValue;
			}
			else
			{ 
				Logger.LogWarning($"{baseLogString} GetAnalogValue => analog measurement with GID {measurementGid:X16} does not exist in collection.");
			}

			return value;
		}
		public async Task<bool> GetDiscreteValue(long measurementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetDiscreteValue method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			var discreteMeasurements = await GetDiscreteMeasurementsFromCache();

			bool isOpen = false;
			if (discreteMeasurements.ContainsKey(measurementGid))
			{
				isOpen = discreteMeasurements[measurementGid].CurrentOpen;
			}
			return isOpen;
		}
		private async Task UpdateAnalogMeasurement(long measurementGid, float value, CommandOriginType commandOrigin, AlarmType alarmType)
		{
			string verboseMessage = $"{baseLogString} entering UpdateAnalogMeasurement method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			var analogMeasurements = await GetAnalogMeasurementsFromCache();

			if (analogMeasurements.TryGetValue(measurementGid, out AnalogMeasurement measurement))
			{
				measurement.CurrentValue = value;
				measurement.Alarm = alarmType;
			}
			else
			{
				Logger.LogWarning($"{baseLogString} UpdateAnalogMeasurement => Failed to update analog measurement with GID {measurementGid:X16}. There is no such a measurement.");
			}

			await AnalogMeasurementsCache.SetAsync((short)MeasurementPorviderCacheType.Origin, analogMeasurements);
		}
		public async Task UpdateAnalogMeasurement(Dictionary<long, AnalogModbusData> data)
		{
			string verboseMessage = $"{baseLogString} entering UpdateAnalogMeasurement method with dictionary parameter.";
			Logger.LogVerbose(verboseMessage);

			foreach (long gid in data.Keys)
			{
				AnalogModbusData measurementData = data[gid];

				await UpdateAnalogMeasurement(gid, (float)measurementData.Value, measurementData.CommandOrigin, measurementData.Alarm);
			}
			//DiscreteMeasurementDelegate?.Invoke();
		}
		private async Task<bool> UpdateDiscreteMeasurement(long measurementGid, int value, CommandOriginType commandOrigin)
		{
			string verboseMessage = $"{baseLogString} entering UpdateDiscreteMeasurement method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			bool success = true;
			var discreteMeasurements = await GetDiscreteMeasurementsFromCache();

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

				//using (ReportPotentialOutageProxy reportPotentialOutageProxy = proxyFactory.CreateProxy<ReportPotentialOutageProxy, IReportPotentialOutageContract>(EndpointNames.ReportPotentialOutageEndpoint))
				//{
				//	if (reportPotentialOutageProxy == null)
				//	{
				//		string message = "UpdateDiscreteMeasurement => ReportPotentialOutageProxy is null.";
				//		logger.LogError(message);
				//		throw new NullReferenceException(message);
				//	}

				//	try
				//	{
				//		if (measurement.CurrentOpen)
				//		{
				//			reportPotentialOutageProxy.ReportPotentialOutage(measurement.ElementId, commandOrigin);
				//		}
				//		else
				//		{
				//			reportPotentialOutageProxy.OnSwitchClose(measurement.ElementId);
				//		}
				//	}
				//	catch (Exception e)
				//	{
				//		logger.LogError("Failed to report potential outage.", e);
				//	}
				//}

			}
			else
			{
				Logger.LogWarning($"{baseLogString} Failed to update discrete measurement with GID {measurementGid:X16}. There is no such a measurement.");
				success = false;
			}

			var measurementToElementMap = await GetMeasurementToElementMapFromCache();

			if (measurementToElementMap.TryGetValue(measurementGid, out long recloserGid) 
				&& await modelProviderClient.IsRecloser(recloserGid)
				&& commandOrigin != CommandOriginType.CE_COMMAND
				&& commandOrigin != CommandOriginType.OUTAGE_SIMULATOR)
			{
				Logger.LogDebug($"{baseLogString} UpdateDiscreteMeasurement => Calling ResetRecloser on topology provider.");
				await topologyProviderClient.ResetRecloser(recloserGid);
				Logger.LogDebug($"{baseLogString} UpdateDiscreteMeasurement => ResetRecloser from topology provider returned success.");

			}
			return success;
		}
		public async Task UpdateDiscreteMeasurement(Dictionary<long, DiscreteModbusData> data)
		{
			string verboseMessage = $"{baseLogString} entering UpdateDiscreteMeasurement method with dictionary parameter.";
			Logger.LogVerbose(verboseMessage);

			List<long> signalGids = new List<long>();
			foreach (long gid in data.Keys)
			{
				DiscreteModbusData measurementData = data[gid];

				if (await UpdateDiscreteMeasurement(measurementData.MeasurementGid, measurementData.Value, measurementData.CommandOrigin))
				{
					signalGids.Add(gid);
				}
			}

			Logger.LogDebug($"{baseLogString} UpdateDiscreteMeasurement => Invoking Discrete Measurement Delegate in topology provider service.");
			topologyProviderClient.DiscreteMeasurementDelegate();
		}
		public async Task<long> GetElementGidForMeasurement(long measurementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetElementGidForMeasurement method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			long signalGid = 0;
			var discreteMeasurements = await GetDiscreteMeasurementsFromCache();

			if (discreteMeasurements.TryGetValue(measurementGid, out DiscreteMeasurement measurement))
			{
				signalGid = measurement.ElementId;
			}
			return signalGid;
		}
        public async Task<DiscreteMeasurement> GetDiscreteMeasurement(long measurementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetDiscreteMeasurement method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			DiscreteMeasurement measurement;
			bool success = false;
			var discreteMeasurements = await GetDiscreteMeasurementsFromCache();

			if (discreteMeasurements.TryGetValue(measurementGid, out measurement))
			{
				success = true;
			}
			else
			{
				measurement = null;
			}

			Logger.LogDebug($"{baseLogString} GetDiscreteMeasurement => method returned success: {success} for measurement GID {measurementGid:X16}.");
			return measurement;
		}
		public async Task<AnalogMeasurement> GetAnalogMeasurement(long measurementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetAnalogMeasurement method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			AnalogMeasurement measurement;
			bool success = false;
			var analogMeasurements = await GetAnalogMeasurementsFromCache();

			if (analogMeasurements.TryGetValue(measurementGid, out measurement))
			{
				success = true;
			}
			else
			{
				measurement = null;
			}

			Logger.LogDebug($"{baseLogString} GetAnalogMeasurement => method returned success: {success} for measurement GID {measurementGid:X16}.");
			return measurement;
		}
		public async Task AddMeasurementElementPair(long measurementId, long elementId)
		{
			string verboseMessage = $"{baseLogString} entering AddMeasurementElementPair method for measurement GID {measurementId:X16}.";
			Logger.LogVerbose(verboseMessage);

			var measurementToElementMap = await GetMeasurementToElementMapFromCache();

			if (measurementToElementMap.ContainsKey(measurementId))
			{
				string message = $"Measurement with GID {measurementId:X16} already exists in measurement-element mapping.";
				Logger.LogError(message);
				throw new ArgumentException(message);
			}

			measurementToElementMap.Add(measurementId, elementId);

			var elementToMeasurementMap = await GetElementToMeasurementMapFromCache();

			if (elementToMeasurementMap.TryGetValue(elementId, out List<long> measurements))
			{
				measurements.Add(measurementId);
			}
			else
			{
				elementToMeasurementMap.Add(elementId, new List<long>() { measurementId });
			}

			await MeasurementToElementMapCache.SetAsync((short)MeasurementPorviderCacheType.Origin, measurementToElementMap);
			await ElementToMeasurementMapCache.SetAsync((short)MeasurementPorviderCacheType.Origin, elementToMeasurementMap);

			Logger.LogDebug($"{baseLogString} AddMeasurementElementPair => method finished for measurement GID {measurementId} and element GID {elementId}.");
		}

		#region IMeasurementMapContract
		public async Task<List<long>> GetMeasurementsOfElement(long elementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetMeasurementsOfElement method for element GID {elementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			var elementToMeasurementMap = await GetElementToMeasurementMapFromCache();

			if (elementToMeasurementMap.TryGetValue(elementGid, out var measurements))
			{
				Logger.LogDebug($"{baseLogString} GetMeasurementsOfElement => method finished for element GID {elementGid}.");
				return measurements;
			}
			else
			{
				Logger.LogDebug($"{baseLogString} GetMeasurementsOfElement => method finished for element GID {elementGid} and returned no measurements.");
				return new List<long>();
			}

		}
		public async Task<Dictionary<long, List<long>>> GetElementToMeasurementMap()
		{
			string verboseMessage = $"{baseLogString} entering GetElementToMeasurementMap method.";
			Logger.LogVerbose(verboseMessage);

			return await GetElementToMeasurementMapFromCache();
		}
		public async Task<Dictionary<long, long>> GetMeasurementToElementMap()
		{
			string verboseMessage = $"{baseLogString} entering GetMeasurementToElementMap method.";
			Logger.LogVerbose(verboseMessage);
			
			return await GetMeasurementToElementMapFromCache();
		}
        #endregion

        #region Transaction Manager
		public async Task<bool> PrepareForTransaction()
		{
			string verboseMessage = $"{baseLogString} entering PrepareForTransaction method.";
			Logger.LogVerbose(verboseMessage);

			bool success = true;
			try
			{
				Logger.LogDebug($"{baseLogString} PrepareForTransaction => Measurement provider preparing for transaction.");

				var tempAnalogMeasurements = await GetAnalogMeasurementsFromCache(MeasurementPorviderCacheType.Origin);
				var tempDiscreteMeasurements = await GetDiscreteMeasurementsFromCache(MeasurementPorviderCacheType.Origin);
				var tempElementToMeasurementMap = await GetElementToMeasurementMapFromCache(MeasurementPorviderCacheType.Origin);
				var tempMeasurementToElementMap = await GetMeasurementToElementMapFromCache(MeasurementPorviderCacheType.Origin);

				await AnalogMeasurementsCache.SetAsync((short)MeasurementPorviderCacheType.Copy, tempAnalogMeasurements);
				await DiscreteMeasurementsCache.SetAsync((short)MeasurementPorviderCacheType.Copy, tempDiscreteMeasurements);
				await ElementToMeasurementMapCache.SetAsync((short)MeasurementPorviderCacheType.Copy, tempElementToMeasurementMap);
				await MeasurementToElementMapCache.SetAsync((short)MeasurementPorviderCacheType.Copy, tempMeasurementToElementMap);

				await AnalogMeasurementsCache.SetAsync((short)MeasurementPorviderCacheType.Origin, new Dictionary<long, AnalogMeasurement>());
				await DiscreteMeasurementsCache.SetAsync((short)MeasurementPorviderCacheType.Origin, new Dictionary<long, DiscreteMeasurement>());
				await ElementToMeasurementMapCache.SetAsync((short)MeasurementPorviderCacheType.Origin, new Dictionary<long, List<long>>());
				await MeasurementToElementMapCache.SetAsync((short)MeasurementPorviderCacheType.Origin, new Dictionary<long, long>());

				Logger.LogDebug($"{baseLogString} PrepareForTransaction => Measurement provider prepared for transaction successfully.");

			}
			catch (Exception e)
			{
				Logger.LogFatal($"{baseLogString} PrepareForTransaction => Model provider failed to prepare for transaction." +
					$"{Environment.NewLine} Exception message: {e.Message} " +
					$"{Environment.NewLine} Stack trace: {e.StackTrace}");
				success = false;
			}

			return success;
		}

		public async Task CommitTransaction()
		{
			string verboseMessage = $"{baseLogString} entering CommitTransaction method.";
			Logger.LogVerbose(verboseMessage);

			await AnalogMeasurementsCache.TryRemoveAsync((short)MeasurementPorviderCacheType.Copy);
			await DiscreteMeasurementsCache.TryRemoveAsync((short)MeasurementPorviderCacheType.Copy);
			await ElementToMeasurementMapCache.TryRemoveAsync((short)MeasurementPorviderCacheType.Copy);
			await MeasurementToElementMapCache.TryRemoveAsync((short)MeasurementPorviderCacheType.Copy);

			logger.LogDebug("Measurement provider commited transaction successfully.");
		}

		public async Task RollbackTransaction()
		{
			string verboseMessage = $"{baseLogString} entering RollbackTransaction method.";
			Logger.LogVerbose(verboseMessage);

			var tempAnalogMeasurements = await GetAnalogMeasurementsFromCache(MeasurementPorviderCacheType.Copy);
			var tempDiscreteMeasurements = await GetDiscreteMeasurementsFromCache(MeasurementPorviderCacheType.Copy);
			var tempElementToMeasurementMap = await GetElementToMeasurementMapFromCache(MeasurementPorviderCacheType.Copy);
			var tempMeasurementToElementMap = await GetMeasurementToElementMapFromCache(MeasurementPorviderCacheType.Copy);

			await AnalogMeasurementsCache.SetAsync((short)MeasurementPorviderCacheType.Origin, tempAnalogMeasurements);
			await DiscreteMeasurementsCache.SetAsync((short)MeasurementPorviderCacheType.Origin, tempDiscreteMeasurements);
			await ElementToMeasurementMapCache.SetAsync((short)MeasurementPorviderCacheType.Origin, tempElementToMeasurementMap);
			await MeasurementToElementMapCache.SetAsync((short)MeasurementPorviderCacheType.Origin, tempMeasurementToElementMap);

			logger.LogDebug("Measurement provider rolled back successfully.");
		}
		#endregion

		#region CacheGetter
		private async Task<Dictionary<long, AnalogMeasurement>> GetAnalogMeasurementsFromCache(MeasurementPorviderCacheType cacheType = MeasurementPorviderCacheType.Origin)
		{
			string verboseMessage = $"{baseLogString} entering GetAnalogMeasurementsFromCache method.";
			Logger.LogVerbose(verboseMessage);

			ConditionalValue<Dictionary<long, AnalogMeasurement>> analogMeasurements;

			if (await AnalogMeasurementsCache.ContainsKeyAsync((short)cacheType))
			{
				analogMeasurements = await AnalogMeasurementsCache.TryGetValueAsync((short)cacheType);
			}
			else if (cacheType == MeasurementPorviderCacheType.Origin)
			{
				Dictionary<long, AnalogMeasurement> newDict = new Dictionary<long, AnalogMeasurement>();
				await AnalogMeasurementsCache.SetAsync((short)cacheType, newDict);
				return newDict;
			}
			else
			{
				string errorMessage = $"{baseLogString} GetAnalogMeasurementsFromCache => Transaction flag is InTransaction, but there is no transaction model.";
				Logger.LogError(errorMessage);
				throw new Exception(errorMessage);
			}

			if (!analogMeasurements.HasValue)
			{
				string errorMessage = $"{baseLogString} GetAnalogMeasurementsFromCache => TryGetValueAsync() returns no value";
				Logger.LogError(errorMessage);
				throw new Exception(errorMessage);
			}

			return analogMeasurements.Value;
		}
		private async Task<Dictionary<long, DiscreteMeasurement>> GetDiscreteMeasurementsFromCache(MeasurementPorviderCacheType cacheType = MeasurementPorviderCacheType.Origin)
		{
			string verboseMessage = $"{baseLogString} entering GetDicreteMeasurementsFromCache method.";
			Logger.LogVerbose(verboseMessage);

			ConditionalValue<Dictionary<long, DiscreteMeasurement>> discreteMeasurements;

			if (await DiscreteMeasurementsCache.ContainsKeyAsync((short)cacheType))
			{
				discreteMeasurements = await DiscreteMeasurementsCache.TryGetValueAsync((short)cacheType);
			}
			else if (cacheType == MeasurementPorviderCacheType.Origin)
			{
				Dictionary<long, DiscreteMeasurement> newDict = new Dictionary<long, DiscreteMeasurement>();
				await DiscreteMeasurementsCache.SetAsync((short)cacheType, newDict);
				return newDict;
			}
			else
			{
				string errorMessage = $"{baseLogString} GetDicreteMeasurementsFromCache => Transaction flag is InTransaction, but there is no transaction model.";
				Logger.LogError(errorMessage);
				throw new Exception(errorMessage);
			}

			if (!discreteMeasurements.HasValue)
			{
				string errorMessage = $"{baseLogString} GetDicreteMeasurementsFromCache => TryGetValueAsync() returns no value";
				Logger.LogError(errorMessage);
				throw new Exception(errorMessage);
			}

			return discreteMeasurements.Value;
		}
		private async Task<Dictionary<long, List<long>>> GetElementToMeasurementMapFromCache(MeasurementPorviderCacheType cacheType = MeasurementPorviderCacheType.Origin)
		{
			string verboseMessage = $"{baseLogString} entering GetElementToMeasurementMapFromCache method.";
			Logger.LogVerbose(verboseMessage);

			ConditionalValue<Dictionary<long, List<long>>> elementToMeasurement;

			if (await ElementToMeasurementMapCache.ContainsKeyAsync((short)cacheType))
			{
				elementToMeasurement = await ElementToMeasurementMapCache.TryGetValueAsync((short)cacheType);
			}
			else if (cacheType == MeasurementPorviderCacheType.Origin)
			{
				Dictionary<long, List<long>> newDict = new Dictionary<long, List<long>>();
				await ElementToMeasurementMapCache.SetAsync((short)cacheType, newDict);
				return newDict;
			}
			else
			{
				string errorMessage = $"{baseLogString} GetElementToMeasurementMapFromCache => Transaction flag is InTransaction, but there is no transaction model.";
				Logger.LogError(errorMessage);
				throw new Exception(errorMessage);
			}

			if (!elementToMeasurement.HasValue)
			{
				string errorMessage = $"{baseLogString} GetElementToMeasurementMapFromCache => TryGetValueAsync() returns no value";
				Logger.LogError(errorMessage);
				throw new Exception(errorMessage);
			}

			return elementToMeasurement.Value;
		}
		private async Task<Dictionary<long, long>> GetMeasurementToElementMapFromCache(MeasurementPorviderCacheType cacheType = MeasurementPorviderCacheType.Origin)
		{
			string verboseMessage = $"{baseLogString} entering GetMeasurementToElementMapFromCache method.";
			Logger.LogVerbose(verboseMessage);

			ConditionalValue<Dictionary<long, long>> measurementsToElement;

			if (await MeasurementToElementMapCache.ContainsKeyAsync((short)cacheType))
			{
				measurementsToElement = await MeasurementToElementMapCache.TryGetValueAsync((short)cacheType);
			}
			else if (cacheType == MeasurementPorviderCacheType.Origin)
			{
				Dictionary<long, long> newDict = new Dictionary<long, long>();
				await MeasurementToElementMapCache.SetAsync((short)cacheType, newDict);
				return newDict;
			}
			else
			{
				string errorMessage = $"{baseLogString} GetMeasurementToElementMapFromCache => Transaction flag is InTransaction, but there is no transaction model.";
				Logger.LogError(errorMessage);
				throw new Exception(errorMessage);
			}

			if (!measurementsToElement.HasValue)
			{
				string errorMessage = $"{baseLogString} GetMeasurementToElementMapFromCache => TryGetValueAsync() returns no value";
				Logger.LogError(errorMessage);
				throw new Exception(errorMessage);
			}

			return measurementsToElement.Value;
		}
		#endregion

		public async Task SendAnalogCommand(long measurementGid, float commandingValue, CommandOriginType commandOrigin)
		{
			string verboseMessage = $"{baseLogString} entering SendAnalogCommand method. Measurement GID {measurementGid:X16}; Commanding value {commandingValue}; Command Origin {commandOrigin}";
			Logger.LogVerbose(verboseMessage);

			try
			{
				Logger.LogDebug($"{baseLogString} SendAnalogCommand => Calling Send single analog command from scada commanding client.");
				await scadaCommandingClient.SendSingleAnalogCommand(measurementGid, commandingValue, commandOrigin);
				Logger.LogDebug($"{baseLogString} SendAnalogCommand => Send single analog command from scada commanding client successfully called.");
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} SendAnalogCommand => Failed. Exception message: {e.Message}.";
				Logger.LogError(message);
				throw new Exception(message);
			}
		}

		public async Task SendDiscreteCommand(long measurementGid, int value, CommandOriginType commandOrigin)
		{
			string verboseMessage = $"{baseLogString} entering SendDiscreteCommand method. Measurement GID {measurementGid:X16}; Commanding value {value}; Command Origin {commandOrigin}";
			Logger.LogVerbose(verboseMessage);

			try
			{
				DiscreteMeasurement measurement = await GetDiscreteMeasurement(measurementGid);

				if ( measurement != null && !(measurement is ArtificalDiscreteMeasurement))
				{
					Logger.LogDebug($"{baseLogString} SendDiscreteCommand => Calling Send single discrete command from scada commanding client.");
					await scadaCommandingClient.SendSingleDiscreteCommand(measurementGid, (ushort)value, commandOrigin);
					Logger.LogDebug($"{baseLogString} SendDiscreteCommand => Send single discrete command from scada commanding client successfully called.");
				}
				else
				{
					Dictionary<long, DiscreteModbusData> data = new Dictionary<long, DiscreteModbusData>(1)
					{
						{ measurementGid, new DiscreteModbusData((ushort)value, AlarmType.NO_ALARM, measurementGid, commandOrigin) }
					};
					await UpdateDiscreteMeasurement(data);
				}
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} SendDiscreteCommand => Failed. Exception message: {e.Message}.";
				Logger.LogError(message);
				throw;
			}
		}
	}
}