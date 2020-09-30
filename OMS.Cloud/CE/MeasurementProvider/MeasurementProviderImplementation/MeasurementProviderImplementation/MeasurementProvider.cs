using Common.CE;
using Common.CeContracts;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.OMS.HistoryDBManager;
using OMS.Common.WcfClient.OMS.OutageLifecycle;
using OMS.Common.WcfClient.SCADA;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using NetworkType = OMS.Common.Cloud.NetworkType;

namespace CE.MeasurementProviderImplementation
{
    public class MeasurementProvider : IMeasurementProviderContract
	{
		#region Fields
		private readonly HashSet<CommandOriginType> ignorableOriginTypes;

		private readonly string baseLogString;
		private readonly IReliableStateManager stateManager;
		private object syncObj;
		private TransactionMode transactionMode;
		#endregion

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		#region ReliableDictionaries
		private bool isAnalogMeasurementInitialized = false;
		private bool isDiscreteMeasurementInitialized = false;
		private bool isElementToMeasurementInitialized = false;
		private bool isMeasurementToElementInitialized = false;

		private bool AreDictionariesInitialized
		{
			get
			{
				return isAnalogMeasurementInitialized
				&& isDiscreteMeasurementInitialized
				&& isElementToMeasurementInitialized
				&& isMeasurementToElementInitialized;
			}
		}

		private ReliableDictionaryAccess<short, Dictionary<long, AnalogMeasurement>> analogMeasurementsCache;
		private ReliableDictionaryAccess<short, Dictionary<long, AnalogMeasurement>> AnalogMeasurementsCache { get => analogMeasurementsCache; }

		private ReliableDictionaryAccess<short, Dictionary<long, DiscreteMeasurement>> discreteMeasurementsCache;
		private ReliableDictionaryAccess<short, Dictionary<long, DiscreteMeasurement>> DiscreteMeasurementsCache { get => discreteMeasurementsCache; }

		private ReliableDictionaryAccess<short, Dictionary<long, List<long>>> elementToMeasurementMapCache;
		private ReliableDictionaryAccess<short, Dictionary<long, List<long>>> ElementToMeasurementMapCache { get => elementToMeasurementMapCache; }

		private ReliableDictionaryAccess<short, Dictionary<long, long>> measurementToElementMapCache;
		private ReliableDictionaryAccess<short, Dictionary<long, long>> MeasurementToElementMapCache { get => measurementToElementMapCache; }

		private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs eventArgs)
		{
            try
            {
				await InitializeReliableCollections(eventArgs);
			}
            catch (FabricNotPrimaryException)
            {
				Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => NotPrimaryException. To be ignored.");
            }
		}

		private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs eventArgs)
        {
			if (eventArgs.Action == NotifyStateManagerChangedAction.Add)
			{
				var operation = eventArgs as NotifyStateManagerSingleEntityChangedEventArgs;
				string reliableStateName = operation.ReliableState.Name.AbsolutePath;

				if (reliableStateName == ReliableDictionaryNames.AnalogMeasurementsCache)
				{
					analogMeasurementsCache = await ReliableDictionaryAccess<short, Dictionary<long, AnalogMeasurement>>.Create(stateManager, ReliableDictionaryNames.AnalogMeasurementsCache);
					this.isAnalogMeasurementInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.AnalogMeasurementsCache}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.DiscreteMeasurementsCache)
				{
					discreteMeasurementsCache = await ReliableDictionaryAccess<short, Dictionary<long, DiscreteMeasurement>>.Create(stateManager, ReliableDictionaryNames.DiscreteMeasurementsCache);
					this.isDiscreteMeasurementInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.DiscreteMeasurementsCache}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.ElementsToMeasurementMapCache)
				{
					elementToMeasurementMapCache = await ReliableDictionaryAccess<short, Dictionary<long, List<long>>>.Create(stateManager, ReliableDictionaryNames.ElementsToMeasurementMapCache);
					this.isElementToMeasurementInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.ElementsToMeasurementMapCache}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.MeasurementsToElementMapCache)
				{
					measurementToElementMapCache = await ReliableDictionaryAccess<short, Dictionary<long, long>>.Create(stateManager, ReliableDictionaryNames.MeasurementsToElementMapCache);
					await measurementToElementMapCache.SetAsync((short)MeasurementPorviderCacheType.Origin, new Dictionary<long, long>());
					this.isMeasurementToElementInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.MeasurementsToElementMapCache}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
			}
		}
		#endregion ReliableDictionaries

		public MeasurementProvider(IReliableStateManager stateManager)
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);

			this.stateManager = stateManager;
			stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
			syncObj = new object();
			transactionMode = TransactionMode.NoTransaction;

			ignorableOriginTypes = new HashSet<CommandOriginType>()
			{ 
				/*CommandOriginType.USER_COMMAND,*/ 
				CommandOriginType.ISOLATING_ALGORITHM_COMMAND
			};

			string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
			Logger.LogDebug(debugMessage);
		}

		#region IMeasurementMapContract
		public async Task AddAnalogMeasurement(AnalogMeasurement analogMeasurement)
		{
			string verboseMessage = $"{baseLogString} entering AddAnalogMeasurement method.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

			try
			{
				if (analogMeasurement == null)
				{
					string message = $"{baseLogString} AddAnalogMeasurement => analog measurement parameter is null.";
					Logger.LogError(message);
					//throw new Exception(message);
					return;
				}

				var analogMeasurements = await GetAnalogMeasurementsFromCache();

				if (!analogMeasurements.ContainsKey(analogMeasurement.Id))
				{
					analogMeasurements.Add(analogMeasurement.Id, analogMeasurement);
				}

				await AnalogMeasurementsCache.SetAsync((short)MeasurementPorviderCacheType.Origin, analogMeasurements);
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} AddAnalogMeasurement => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);
			}
		}

		public async Task AddDiscreteMeasurement(DiscreteMeasurement discreteMeasurement)
		{
			string verboseMessage = $"{baseLogString} entering AddDiscreteMeasurement method.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

			try
			{
				if (discreteMeasurement == null)
				{
					string message = $"{baseLogString} AddDiscreteMeasurement => discrete measurement parameter is null.";
					Logger.LogError(message);
					//throw new Exception(message);
					return;
				}


				var discreteMeasurementsCache = await DiscreteMeasurementsCache.GetEnumerableDictionaryAsync();
				if(!discreteMeasurementsCache.ContainsKey((short)MeasurementPorviderCacheType.Origin))
				{
					Logger.LogWarning($"{baseLogString} AddDiscreteMeasurement => {MeasurementPorviderCacheType.Origin} was not present in discreteMeasurementsCache");
					return;
				}

				var discreteMeasurements = discreteMeasurementsCache[(short)MeasurementPorviderCacheType.Origin];
				if (!discreteMeasurements.ContainsKey(discreteMeasurement.Id))
				{
					discreteMeasurements.Add(discreteMeasurement.Id, discreteMeasurement);
				}
				else
				{
					Logger.LogDebug($"{baseLogString} AddDiscreteMeasurement => Updating discrete measurement with GID {discreteMeasurement.Id:X16}.");
					discreteMeasurements.Remove(discreteMeasurement.Id);
					discreteMeasurements.Add(discreteMeasurement.Id, discreteMeasurement);
				}

				await DiscreteMeasurementsCache.SetAsync((short)MeasurementPorviderCacheType.Origin, discreteMeasurements);
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} AddDiscreteMeasurement => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);
			}
		}

		public async Task UpdateAnalogMeasurement(Dictionary<long, AnalogModbusData> data)
		{
			string verboseMessage = $"{baseLogString} entering UpdateAnalogMeasurement method with dictionary parameter.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

			try
			{
				foreach (long gid in data.Keys)
				{
					AnalogModbusData measurementData = data[gid];

					await UpdateAnalogMeasurement(gid, (float)measurementData.Value, measurementData.CommandOrigin, measurementData.Alarm);
				}
				//DiscreteMeasurementDelegate?.Invoke(); //MODO: vec bilo je zakomentarisano, da li je potrebno...?
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} UpdateAnalogMeasurement => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);
			}
		}

		public async Task UpdateDiscreteMeasurement(Dictionary<long, DiscreteModbusData> data)
		{
			string verboseMessage = $"{baseLogString} entering UpdateDiscreteMeasurement method with dictionary parameter.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

			try
			{
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
				var topologyProviderClient = TopologyProviderClient.CreateClient();
				topologyProviderClient.DiscreteMeasurementDelegate();
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} UpdateDiscreteMeasurement => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);
			}
		}

		public async Task<AnalogMeasurement> GetAnalogMeasurement(long measurementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetAnalogMeasurement method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

			AnalogMeasurement measurement;

			try
			{
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
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} GetAnalogMeasurement => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);
				measurement = null;
			}

			return measurement;
		}

		public async Task<DiscreteMeasurement> GetDiscreteMeasurement(long measurementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetDiscreteMeasurement method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

			DiscreteMeasurement measurement;

			try
			{
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
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} GetDiscreteMeasurement => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);
				measurement = null;
			}

			return measurement;
		}

		public async Task<float> GetAnalogValue(long measurementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetAnalogValue method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

			float value = -1;

			try
			{
				var analogMeasurements = await GetAnalogMeasurementsFromCache();

				if (analogMeasurements.ContainsKey(measurementGid))
				{
					value = analogMeasurements[measurementGid].CurrentValue;
				}
				else
				{
					Logger.LogWarning($"{baseLogString} GetAnalogValue => analog measurement with GID {measurementGid:X16} does not exist in collection.");
				}
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} GetAnalogValue => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);
			}

			return value;
		}

		public async Task<bool> GetDiscreteValue(long measurementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetDiscreteValue method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

			bool isOpen = false; //MODO: mozda vracati Contional value kako bi postaojala i indikacija da li je metoda uspesno izvrsena i vrednost 'isOpen'

			try
			{
				var discreteMeasurements = await GetDiscreteMeasurementsFromCache();

				if (discreteMeasurements.ContainsKey(measurementGid))
				{
					isOpen = discreteMeasurements[measurementGid].CurrentOpen;
				}
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} GetDiscreteValue => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);
			}

			return isOpen;
		}

		#region Measurement-Element Mapping
		public async Task AddMeasurementElementPair(long measurementId, long elementId)
		{
			string verboseMessage = $"{baseLogString} entering AddMeasurementElementPair method for measurement GID {measurementId:X16}.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

			if (measurementId == 0 || elementId == 0)
			{
				//string message = $"Measurement with GID {measurementId:X16} already exists in measurement-element mapping.";
				Logger.LogWarning($"{baseLogString} AddMeasurementElementPair => mesurementID : {measurementId} | elementID : {elementId}");
			}

			try
            {
				var measurementToElementMapChe = await MeasurementToElementMapCache.GetEnumerableDictionaryAsync();
				var measurementToElementMap = measurementToElementMapChe[(short)MeasurementPorviderCacheType.Origin];
				//var measurementToElementMap = await GetMeasurementToElementMapFromCache();

				if (measurementToElementMap.ContainsKey(measurementId))
				{
					//string message = $"Measurement with GID {measurementId:X16} already exists in measurement-element mapping.";
					//Logger.LogWarning(message);
					//throw new ArgumentException(message);
					return;
				}

				measurementToElementMap.Add(measurementId, elementId);
				await MeasurementToElementMapCache.SetAsync((short)MeasurementPorviderCacheType.Origin, measurementToElementMap);

				var elementToMeasurementMap = await GetElementToMeasurementMapFromCache();

				if (elementToMeasurementMap.TryGetValue(elementId, out List<long> measurements))
				{
					measurements.Add(measurementId);
				}
				else
				{
					elementToMeasurementMap.Add(elementId, new List<long>() { measurementId });
				}

				await ElementToMeasurementMapCache.SetAsync((short)MeasurementPorviderCacheType.Origin, elementToMeasurementMap);

				Logger.LogDebug($"{baseLogString} AddMeasurementElementPair => method finished for measurement GID {measurementId} and element GID {elementId}.");
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} AddMeasurementElementPair => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);
			}
		}

		public async Task<Dictionary<long, List<long>>> GetElementToMeasurementMap()
		{
			string verboseMessage = $"{baseLogString} entering GetElementToMeasurementMap method.";
			Logger.LogVerbose(verboseMessage);

			Dictionary<long, List<long>> result;

			try
            {
				result = await GetElementToMeasurementMapFromCache();
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} GetElementToMeasurementMap => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);

				result = new Dictionary<long, List<long>>();
			}

			return result;
		}

		public async Task<long> GetElementGidForMeasurement(long measurementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetElementGidForMeasurement method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

			long signalGid = 0;

            try
            {
				var discreteMeasurements = await GetDiscreteMeasurementsFromCache();

				if (discreteMeasurements.TryGetValue(measurementGid, out DiscreteMeasurement measurement))
				{
					signalGid = measurement.ElementId;
				}
			}
            catch (Exception e)
            {
				string errorMessage = $"{baseLogString} GetElementGidForMeasurement => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);
			}

			return signalGid;
		}

		public async Task<Dictionary<long, long>> GetMeasurementToElementMap()
		{
			string verboseMessage = $"{baseLogString} entering GetMeasurementToElementMap method.";
			Logger.LogVerbose(verboseMessage);

			Dictionary<long, long> result;

			try
			{
				var measurementToElementMapChe = await MeasurementToElementMapCache.GetEnumerableDictionaryAsync();
				result = measurementToElementMapChe[(short)MeasurementPorviderCacheType.Origin];
				//result = await GetMeasurementToElementMapFromCache();
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} GetMeasurementToElementMap => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);

				result = new Dictionary<long, long>();
			}

			return result;
		}

		public async Task<List<long>> GetMeasurementsOfElement(long elementGid)
		{
			string verboseMessage = $"{baseLogString} entering GetMeasurementsOfElement method for element GID {elementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			List<long> measurements;

            try
            {
				var elementToMeasurementMap = await GetElementToMeasurementMapFromCache();

				if (elementToMeasurementMap.TryGetValue(elementGid, out measurements))
				{
					Logger.LogDebug($"{baseLogString} GetMeasurementsOfElement => method finished for element GID {elementGid}.");
				}
				else
				{
					Logger.LogDebug($"{baseLogString} GetMeasurementsOfElement => method finished for element GID {elementGid} and returned no measurements.");
					measurements = new List<long>();
				}
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} GetMeasurementsOfElement => Exception: {e.Message}";
				Logger.LogError(errorMessage, e);

				measurements = new List<long>();
			}

			return measurements;
		}
		#endregion

		#region Commanding
		public async Task<bool> SendSingleAnalogCommand(long measurementGid, float commandingValue, CommandOriginType commandOrigin)
		{
			string verboseMessage = $"{baseLogString} entering SendSingleAnalogCommand method. Measurement GID {measurementGid:X16}; Commanding value {commandingValue}; Command Origin {commandOrigin}";
			Logger.LogVerbose(verboseMessage);

			try
			{
				Logger.LogDebug($"{baseLogString} SendSingleAnalogCommand => Calling Send single analog command from scada commanding client.");
				var scadaCommandingClient = ScadaCommandingClient.CreateClient();
				var success = await scadaCommandingClient.SendSingleAnalogCommand(measurementGid, commandingValue, commandOrigin);
				Logger.LogDebug($"{baseLogString} SendSingleAnalogCommand => Send single analog command from scada commanding client called.");

				return success;
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} SendSingleAnalogCommand => Failed. Exception message: {e.Message}.";
				Logger.LogError(message, e);
				return false;
			}
		}

		public async Task<bool> SendSingleDiscreteCommand(long measurementGid, int value, CommandOriginType commandOrigin)
		{
			string verboseMessage = $"{baseLogString} entering SendSingleDiscreteCommand method. Measurement GID {measurementGid:X16}; Commanding value {value}; Command Origin {commandOrigin}";
			Logger.LogVerbose(verboseMessage);

			try
			{
				bool success;
				DiscreteMeasurement measurement = await GetDiscreteMeasurement(measurementGid);

				if (measurement != null && !(measurement is ArtificalDiscreteMeasurement))
				{
					Logger.LogDebug($"{baseLogString} SendSingleDiscreteCommand => Calling Send single discrete command from scada commanding client.");
					var scadaCommandingClient = ScadaCommandingClient.CreateClient();
					success = await scadaCommandingClient.SendSingleDiscreteCommand(measurementGid, (ushort)value, commandOrigin);
					Logger.LogDebug($"{baseLogString} SendSingleDiscreteCommand => Send single discrete command from scada commanding client called.");
				}
				else
				{
					Dictionary<long, DiscreteModbusData> data = new Dictionary<long, DiscreteModbusData>(1)
					{
						{ measurementGid, new DiscreteModbusData((ushort)value, AlarmType.NO_ALARM, measurementGid, commandOrigin) }
					};

					await UpdateDiscreteMeasurement(data);
					success = true;
				}

				return success;
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} SendSingleDiscreteCommand => Failed. Exception message: {e.Message}.";
				Logger.LogError(message, e);
				return false;
			}
		}

		public async Task<bool> SendMultipleAnalogCommand(Dictionary<long, float> commands, CommandOriginType commandOrigin)
		{
			string verboseMessage = $"{baseLogString} entering SendMultipleAnalogCommand method. Commands count: {commands.Count}, Command Origin: {commandOrigin}";
			Logger.LogVerbose(verboseMessage);

			try
			{
				if(commands.Count == 0)
				{
					Logger.LogDebug($"{baseLogString} SendMultipleAnalogCommand => No commands to send.");
					return true;
				}
				
				Logger.LogDebug($"{baseLogString} SendMultipleAnalogCommand => Calling Send Multiple analog command from scada commanding client.");
				var scadaCommandingClient = ScadaCommandingClient.CreateClient();
				var success = await scadaCommandingClient.SendMultipleAnalogCommand(commands, commandOrigin);
				Logger.LogDebug($"{baseLogString} SendMultipleAnalogCommand => Send Multiple analog command from scada commanding client called.");
				return success;
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} SendMultipleAnalogCommand => Failed. Exception message: {e.Message}.";
				Logger.LogError(message, e);
				return false;
			}
		}

		public async Task<bool> SendMultipleDiscreteCommand(Dictionary<long, int> commands, CommandOriginType commandOrigin)
		{
			string verboseMessage = $"{baseLogString} entering SendMultipleDiscreteCommand method. Commands count: {commands.Count}, Command Origin: {commandOrigin}";
			Logger.LogVerbose(verboseMessage);

			try
			{
				if (commands.Count == 0)
				{
					Logger.LogDebug($"{baseLogString} SendMultipleDiscreteCommand => No commands to send.");
					return true;
				}

				var nonArtificalCommands = new Dictionary<long, ushort>();
				var artificalCommands = new Dictionary<long, DiscreteModbusData>();

				foreach (var measurementGid in commands.Keys)
                {
					ushort value = (ushort)commands[measurementGid];
					DiscreteMeasurement measurement = await GetDiscreteMeasurement(measurementGid);

					if(measurement == null)
                    {
						Logger.LogError($"{baseLogString} SendMultipleDiscreteCommand => Measurement for measurementGid: 0x{measurementGid:X16} is null");
						return false;
					}

					if (measurement is ArtificalDiscreteMeasurement)
					{
						artificalCommands.Add(measurementGid, new DiscreteModbusData(value, AlarmType.NO_ALARM, measurementGid, commandOrigin));
					}
					else
					{
						nonArtificalCommands.Add(measurementGid, value);
					}
				}

				Logger.LogDebug($"{baseLogString} SendMultipleDiscreteCommand => Calling Send multiple discrete command from scada commanding client.");
				var scadaCommandingClient = ScadaCommandingClient.CreateClient();
				var success = await scadaCommandingClient.SendMultipleDiscreteCommand(nonArtificalCommands, commandOrigin);
				Logger.LogDebug($"{baseLogString} SendMultipleDiscreteCommand => Send multiple discrete command from scada commanding client called.");

				await UpdateDiscreteMeasurement(artificalCommands);

				return success;
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} SendMultipleDiscreteCommand => Failed. Exception message: {e.Message}.";
				Logger.LogError(message, e);
				return false;
			}
		}
		#endregion Commanding

		public Task<bool> IsAlive()
		{
			return Task.Run(() => { return true; });
		}
		#endregion IMeasurementMapContract

		#region Transaction Manager
		public async Task<bool> PrepareForTransaction()
		{
			string verboseMessage = $"{baseLogString} entering PrepareForTransaction method.";
			Logger.LogVerbose(verboseMessage);

			bool success = true;

			try
			{
				Logger.LogDebug($"{baseLogString} PrepareForTransaction => Measurement provider preparing for transaction.");

				transactionMode = TransactionMode.InTransaction;

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

            try
            {
				await AnalogMeasurementsCache.TryRemoveAsync((short)MeasurementPorviderCacheType.Copy);
				await DiscreteMeasurementsCache.TryRemoveAsync((short)MeasurementPorviderCacheType.Copy);
				await ElementToMeasurementMapCache.TryRemoveAsync((short)MeasurementPorviderCacheType.Copy);
				await MeasurementToElementMapCache.TryRemoveAsync((short)MeasurementPorviderCacheType.Copy);

				logger.LogDebug("Measurement provider commited transaction successfully.");
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} CommitTransaction => Failed. Exception message: {e.Message}.";
				Logger.LogError(message, e);
			}
		}

		public async Task RollbackTransaction()
		{
			string verboseMessage = $"{baseLogString} entering RollbackTransaction method.";
			Logger.LogVerbose(verboseMessage);

            try
            {
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
            catch (Exception e)
            {
				string message = $"{baseLogString} RollbackTransaction => Failed. Exception message: {e.Message}.";
				Logger.LogError(message, e);
			}

			
		}
		#endregion

		#region Private Methods
		private async Task UpdateAnalogMeasurement(long measurementGid, float value, CommandOriginType commandOrigin, AlarmType alarmType)
		{
			string verboseMessage = $"{baseLogString} entering UpdateAnalogMeasurement method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

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

		private async Task<bool> UpdateDiscreteMeasurement(long measurementGid, int value, CommandOriginType commandOrigin)
		{
			string verboseMessage = $"{baseLogString} entering UpdateDiscreteMeasurement method for measurement GID {measurementGid:X16}.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

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

				if (measurement.CurrentOpen)
				{
					var ceModelProviderClient = CeModelProviderClient.CreateClient();
					if (!await ceModelProviderClient.IsRecloser(measurement.ElementId))
					{
						var potentialOutageReportingClient = PotentialOutageReportingClient.CreateClient();
						await potentialOutageReportingClient.EnqueuePotentialOutageCommand(measurement.ElementId, commandOrigin, NetworkType.SCADA_NETWORK); //TODO: proveriti da li ce ovde uvek biti skada deo mreze....
					}
					else
					{
						Logger.LogDebug($"{baseLogString} UpdateDiscreteMeasurement => Element with gid 0x{measurement.ElementId:X16} is a Recloser. EnqueuePotentialOutageCommand call is not required.");
					}
				}
				else
				{
					//var historyDBManagerClient = HistoryDBManagerClient.CreateClient();
					//await historyDBManagerClient.OnSwitchClosed(measurement.ElementId);
				}
			}
			else
			{
				Logger.LogWarning($"{baseLogString} Failed to update discrete measurement with GID {measurementGid:X16}. There is no such a measurement.");
				success = false;
			}

			var measurementToElementMapChe = await MeasurementToElementMapCache.GetEnumerableDictionaryAsync();
			var measurementToElementMap = measurementToElementMapChe[(short)MeasurementPorviderCacheType.Origin];

			var modelProviderClient = CeModelProviderClient.CreateClient();

			//TODO: see this
			//if (measurementToElementMap.TryGetValue(measurementGid, out long recloserGid)
			//	&& await modelProviderClient.IsRecloser(recloserGid)
			//	&& (commandOrigin == CommandOriginType.USER_COMMAND
			//		|| commandOrigin == CommandOriginType.ISOLATING_ALGORITHM_COMMAND 
			//		|| commandOrigin == CommandOriginType.LOCATION_AND_ISOLATING_ALGORITHM_COMMAND))
			if (measurementToElementMap.TryGetValue(measurementGid, out long recloserGid)
				&& await modelProviderClient.IsRecloser(recloserGid)
				&& commandOrigin == CommandOriginType.USER_COMMAND)
			{
				Logger.LogDebug($"{baseLogString} UpdateDiscreteMeasurement => Calling ResetRecloser on topology provider.");
				var topologyProviderClient = TopologyProviderClient.CreateClient();
				await topologyProviderClient.ResetRecloser(recloserGid);
				Logger.LogDebug($"{baseLogString} UpdateDiscreteMeasurement => ResetRecloser from topology provider returned success.");

			}
			return success;
		}

		#region CacheGetter
		private async Task<Dictionary<long, AnalogMeasurement>> GetAnalogMeasurementsFromCache(MeasurementPorviderCacheType cacheType = MeasurementPorviderCacheType.Origin)
		{
			string verboseMessage = $"{baseLogString} entering GetAnalogMeasurementsFromCache method.";
			Logger.LogVerbose(verboseMessage);

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

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

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

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

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

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

			while (!AreDictionariesInitialized)
			{
				await Task.Delay(1000);
			}

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
        #endregion CacheGetter
        #endregion Private Methods
    }
}