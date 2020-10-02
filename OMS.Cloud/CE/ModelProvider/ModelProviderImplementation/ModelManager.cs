using Common.CE;
using Common.CeContracts;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.WcfClient.CE;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CE.ModelProviderImplementation
{
	public class ModelManager : IModelManager
	{
		#region Readonly
		private readonly Dictionary<DMSType, List<ModelCode>> ReferenceProperties = new Dictionary<DMSType, List<ModelCode>>()
			{
				{ DMSType.TERMINAL, new List<ModelCode>(){ ModelCode.TERMINAL_CONDUCTINGEQUIPMENT,ModelCode.TERMINAL_CONNECTIVITYNODE, ModelCode.TERMINAL_MEASUREMENTS } },
				{ DMSType.CONNECTIVITYNODE, new List<ModelCode>(){ ModelCode.CONNECTIVITYNODE_TERMINALS } },
				{ DMSType.POWERTRANSFORMER, new List<ModelCode>(){ ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS } },
				{ DMSType.ENERGYSOURCE, new List<ModelCode>(){ ModelCode.CONDUCTINGEQUIPMENT_TERMINALS, ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE } },
				{ DMSType.ENERGYCONSUMER, new List<ModelCode>(){ ModelCode.CONDUCTINGEQUIPMENT_TERMINALS, ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE } },
				{ DMSType.TRANSFORMERWINDING, new List<ModelCode>(){ ModelCode.CONDUCTINGEQUIPMENT_TERMINALS, ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE, ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER } },
				{ DMSType.FUSE, new List<ModelCode>(){ ModelCode.CONDUCTINGEQUIPMENT_TERMINALS, ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE } },
				{ DMSType.DISCONNECTOR, new List<ModelCode>(){ ModelCode.CONDUCTINGEQUIPMENT_TERMINALS, ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE } },
				{ DMSType.BREAKER, new List<ModelCode>(){ ModelCode.CONDUCTINGEQUIPMENT_TERMINALS, ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE } },
				{ DMSType.LOADBREAKSWITCH, new List<ModelCode>(){ ModelCode.CONDUCTINGEQUIPMENT_TERMINALS, ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE } },
				{ DMSType.ACLINESEGMENT, new List<ModelCode>(){ ModelCode.CONDUCTINGEQUIPMENT_TERMINALS, ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE } },
				{ DMSType.SYNCHRONOUSMACHINE, new List<ModelCode>(){ ModelCode.CONDUCTINGEQUIPMENT_TERMINALS, ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE } },
				{ DMSType.ANALOG, new List<ModelCode>(){ ModelCode.MEASUREMENT_TERMINAL } },
				{ DMSType.DISCRETE, new List<ModelCode>(){ ModelCode.MEASUREMENT_TERMINAL } },
				{ DMSType.BASEVOLTAGE, new List<ModelCode>(){ ModelCode.BASEVOLTAGE_CONDUCTINGEQUIPMENTS } }
			};

		// Redosled konkretnih modela je bitan
		// BaseVoltage se ucitavaju prilikom obrade elemenata => moraju biti obradjeni pre samih elemenata
		// Merenja se dodeljuju elementima => sve vrste elemenata pre njih moraju biti obradjene
		// Bilo koja vrsta konekcija se odredjuje pomocu terminala i connectivity node-ova => oni moraju biti prvi obradjeni
		private readonly HashSet<ModelCode> ConcreteModels = new HashSet<ModelCode>()
		{
			ModelCode.TERMINAL,
			ModelCode.CONNECTIVITYNODE,
			ModelCode.BASEVOLTAGE,
			ModelCode.POWERTRANSFORMER,
			ModelCode.ENERGYSOURCE,
			ModelCode.ENERGYCONSUMER,
			ModelCode.TRANSFORMERWINDING,
			ModelCode.FUSE,
			ModelCode.DISCONNECTOR,
			ModelCode.BREAKER,
			ModelCode.LOADBREAKSWITCH,
			ModelCode.ACLINESEGMENT,
			ModelCode.SYNCHRONOUSMACHINE,
			ModelCode.ANALOG,
			ModelCode.DISCRETE
		};
		#endregion

		#region Fields
		private static long noScadaGuid = 1;

		private readonly string baseLogString;
		private readonly object syncObj = new object();
		private readonly IReliableStateManager stateManager;

		private readonly NetworkModelGDA networkModelGda;
		private readonly ModelResourcesDesc modelResourcesDesc;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		#endregion

		#region Reliable Dictionaries
		private bool isEnergySourcesInitialized;
		private bool isReclosersInitialized;
		private bool isMeasurementsInitialized;
		private bool isTopologyElementsInitialized;
		private bool isBaseVoltagesInitialized;
		private bool isElementConnectionsInitialized; 
		private bool isMeasurementToConnectedTerminalMapInitialized;
		private bool isTerminalToConnectedElementsMapInitialized;

		private bool ReliableDictionariesInitialized
		{
			get
			{
				return	isEnergySourcesInitialized &&
						isReclosersInitialized &&
						isMeasurementsInitialized &&
						isTopologyElementsInitialized &&
						isBaseVoltagesInitialized &&
						isElementConnectionsInitialized &&
						isMeasurementToConnectedTerminalMapInitialized &&
						isTerminalToConnectedElementsMapInitialized;
			}
		}

		private ReliableDictionaryAccess<string, List<long>> energySources;
		private ReliableDictionaryAccess<string, List<long>> EnergySources
		{
			get { return energySources; }
		}

		private ReliableDictionaryAccess<string, HashSet<long>> reclosers;
		private ReliableDictionaryAccess<string, HashSet<long>> Reclosers
	{
			get { return reclosers; }
		}

		private ReliableDictionaryAccess<long, IMeasurement> measurements;
		private ReliableDictionaryAccess<long, IMeasurement> Measurements
		{
			get { return measurements; }
		}

		private ReliableDictionaryAccess<long, ITopologyElement> topologyElements;
		private ReliableDictionaryAccess<long, ITopologyElement> TopologyElements
		{
			get { return topologyElements; }
		}

		private ReliableDictionaryAccess<long, float> baseVoltages;
		private ReliableDictionaryAccess<long, float> BaseVoltages
		{
			get { return baseVoltages; }
		}

		private ReliableDictionaryAccess<long, List<long>> elementConnections;
		private ReliableDictionaryAccess<long, List<long>> ElementConnections
		{
			get { return elementConnections; }
		}

		private ReliableDictionaryAccess<long, long> measurementToConnectedTerminalMap;
		private ReliableDictionaryAccess<long, long> MeasurementToConnectedTerminalMap
		{
			get { return measurementToConnectedTerminalMap; }
		}

		private ReliableDictionaryAccess<long, List<long>> terminalToConnectedElementsMap;
		private ReliableDictionaryAccess<long, List<long>> TerminalToConnectedElementsMap
		{
			get { return terminalToConnectedElementsMap; }
		}

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
			catch (FabricObjectClosedException)
			{
				Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => FabricObjectClosedException. To be ignored.");
			}
			catch (COMException)
			{
				Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => {typeof(COMException)}. To be ignored.");
			}
		}

		private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
		{
			if (e.Action == NotifyStateManagerChangedAction.Add)
			{
				var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
				string reliableStateName = operation.ReliableState.Name.AbsolutePath;

				if (reliableStateName == ReliableDictionaryNames.EnergySources)
				{
					this.energySources = await ReliableDictionaryAccess<string, List<long>>.Create(stateManager, ReliableDictionaryNames.EnergySources);
					this.isEnergySourcesInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.EnergySources}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.Reclosers)
				{
					this.reclosers = await ReliableDictionaryAccess<string, HashSet<long>>.Create(stateManager, ReliableDictionaryNames.Reclosers);
					this.isReclosersInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.Reclosers}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.Measurements)
				{
					this.measurements = await ReliableDictionaryAccess<long, IMeasurement>.Create(stateManager, ReliableDictionaryNames.Measurements);
					this.isMeasurementsInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.Measurements}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.TopologyElements)
				{
					this.topologyElements = await ReliableDictionaryAccess<long, ITopologyElement>.Create(stateManager, ReliableDictionaryNames.TopologyElements);
					this.isTopologyElementsInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.TopologyElements}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.BaseVoltages)
				{
					this.baseVoltages = await ReliableDictionaryAccess<long, float>.Create(stateManager, ReliableDictionaryNames.BaseVoltages);
					this.isBaseVoltagesInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.BaseVoltages}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.ElementConnections)
				{
					this.elementConnections = await ReliableDictionaryAccess<long, List<long>>.Create(stateManager, ReliableDictionaryNames.ElementConnections);
					this.isElementConnectionsInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.ElementConnections}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.MeasurementToConnectedTerminalMap)
				{
					this.measurementToConnectedTerminalMap = await ReliableDictionaryAccess<long, long>.Create(stateManager, ReliableDictionaryNames.MeasurementToConnectedTerminalMap);
					this.isMeasurementToConnectedTerminalMapInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.MeasurementToConnectedTerminalMap}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
				else if (reliableStateName == ReliableDictionaryNames.TerminalToConnectedElementsMap)
				{
					this.terminalToConnectedElementsMap = await ReliableDictionaryAccess<long, List<long>>.Create(stateManager, ReliableDictionaryNames.TerminalToConnectedElementsMap);
					this.isTerminalToConnectedElementsMapInitialized = true;

					string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.TerminalToConnectedElementsMap}' ReliableDictionaryAccess initialized.";
					Logger.LogDebug(debugMessage);
				}
			}
		}
		#endregion Reliable Dictionaries

		public ModelManager(IReliableStateManager stateManager)
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);

			networkModelGda = new NetworkModelGDA();
			modelResourcesDesc = new ModelResourcesDesc();

			this.isEnergySourcesInitialized = false;
			this.isReclosersInitialized = false;
			this.isMeasurementsInitialized = false;
			this.isTopologyElementsInitialized = false;
			this.isBaseVoltagesInitialized = false;
			this.isElementConnectionsInitialized = false;
			this.isMeasurementToConnectedTerminalMapInitialized = false;
			this.isTerminalToConnectedElementsMapInitialized = false;

			this.stateManager = stateManager;
			this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
		}

		#region Functions
		public async Task<IModelDelta> TryGetAllModelEntitiesAsync()
		{
			string verboseMessage = $"{baseLogString} entering TryGetAllModelEntities method.";
			Logger.LogVerbose(verboseMessage);

			while(!ReliableDictionariesInitialized)
            {
				await Task.Delay(1000);
            }

			ModelDelta modelDelta = new ModelDelta();

			try
			{
				//var clearTasks = new List<Task>
				//{
				await TopologyElements.ClearAsync();
				await Measurements.ClearAsync();
				await EnergySources.ClearAsync();
				await ElementConnections.ClearAsync();
				await MeasurementToConnectedTerminalMap.ClearAsync();
				await TerminalToConnectedElementsMap.ClearAsync();
				await BaseVoltages.ClearAsync();
				await Reclosers.ClearAsync();
				//};

				//Task.WaitAll(clearTasks.ToArray());

				await energySources.SetAsync(ReliableDictionaryNames.EnergySources, new List<long>());

				await reclosers.SetAsync( ReliableDictionaryNames.Reclosers, new HashSet<long>());

				Logger.LogDebug($"{baseLogString} TryGetAllModelEntities => Getting all network model elements and converting them.");

				await GetBaseVoltagesAsync();

				foreach (var model in ConcreteModels)
				{
					if (model != ModelCode.BASEVOLTAGE)
					{
						List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(model);
						var elements = await networkModelGda.GetExtentValuesAsync(model, properties);
						foreach (var element in elements)
						{
							try
							{
								await TransformToTopologyElementAsync(element);
							}
							catch (Exception e)
							{
								Logger.LogError($"{baseLogString} TryGetAllModelEntitiesAsync failed." +
									$" {Environment.NewLine} {e.Message} " +
									$"{Environment.NewLine} {e.StackTrace}");
							}
						}
					}
				}


				//Parallel.For(0, ConcreteModels.Count, async (i) =>
				//{
				//	var model = ConcreteModels.ElementAt(i);
				//	if (model != ModelCode.BASEVOLTAGE)
				//	{
				//		List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(model);
				//		var elements = await networkModelGda.GetExtentValuesAsync(model, properties);
				//		foreach (var element in elements)
				//		{
				//			try
				//			{
				//				await TransformToTopologyElementAsync(element);
				//			}
				//			catch (Exception e)
				//			{
				//				Logger.LogError($"{baseLogString} TryGetAllModelEntitiesAsync failed." +
				//					$" {Environment.NewLine} {e.Message} " +
				//					$"{Environment.NewLine} {e.StackTrace}");
				//			}
				//		}
				//	}
				//});

				var enumerableMeasurements = await Measurements.GetEnumerableDictionaryAsync();
				List<IMeasurement> updatedMeasurements = new List<IMeasurement>(enumerableMeasurements.Count);
				foreach (var measurement in enumerableMeasurements.Values)
				{
					var elementId = await PutMeasurementsInElements(measurement);
					var measurementProviderClient = MeasurementProviderClient.CreateClient();
					await measurementProviderClient.AddMeasurementElementPair(measurement.Id, elementId);
					updatedMeasurements.Add(measurement);
				}

				foreach (var updatedMeas in updatedMeasurements)
				{
					await Measurements.SetAsync(updatedMeas.Id, updatedMeas);
				}

				var enumerableTopologyElements = await TopologyElements.GetEnumerableDictionaryAsync();
				List<ITopologyElement> updatedElements = new List<ITopologyElement>(enumerableTopologyElements.Count);
				foreach (var element in enumerableTopologyElements.Values)
				{
					if (element.Measurements.Count == 0)
					{
						ITopologyElement updatedElement = await CreateNoScadaMeasurementAsync(element);
						updatedElements.Add(updatedElement);
					}
				}

				foreach (var updatedEl in updatedElements)
				{
					await TopologyElements.SetAsync(updatedEl.Id, updatedEl);
				}

				enumerableTopologyElements = await TopologyElements.GetEnumerableDictionaryAsync();
				modelDelta.TopologyElements = enumerableTopologyElements;
				modelDelta.ElementConnections = await ElementConnections.GetEnumerableDictionaryAsync();

				var reclosersResult = await Reclosers.TryGetValueAsync(ReliableDictionaryNames.Reclosers);
				if (reclosersResult.HasValue)
                {
					modelDelta.Reclosers = reclosersResult.Value;
				}
				else
                {
					Logger.LogWarning($"{baseLogString} Reliable collection '{ReliableDictionaryNames.Reclosers}' was not defined yet. Handling...");
					await Reclosers.SetAsync(ReliableDictionaryNames.Reclosers, new HashSet<long>());

					modelDelta.Reclosers = new HashSet<long>();
                }
				
				var enegySourcesResult = await EnergySources.TryGetValueAsync(ReliableDictionaryNames.EnergySources);
				if (reclosersResult.HasValue)
				{
					modelDelta.EnergySources = enegySourcesResult.Value;
				}
				else
                {
					Logger.LogWarning($"{baseLogString} Reliable collection '{ReliableDictionaryNames.EnergySources}' was not defined yet. Handling...");
					await EnergySources.SetAsync(ReliableDictionaryNames.EnergySources, new List<long>());

					modelDelta.EnergySources = new List<long>();
				}
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} TryGetAllModelEntities => Failed in get all network model elements." +
					$"{Environment.NewLine} Exception message: {e.Message}" +
					$"{Environment.NewLine} Stack trace: {e.StackTrace}";
				Logger.LogError(message);
				throw new Exception(message);
			}

			return modelDelta;
		}
		
		private async Task GetBaseVoltagesAsync()
		{
			string verboseMessage = $"{baseLogString} entering GetBaseVoltagesAsync method.";
			Logger.LogVerbose(verboseMessage);

			List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.BASEVOLTAGE);

			var elements = await networkModelGda.GetExtentValuesAsync(ModelCode.BASEVOLTAGE, properties);

			foreach (var element in elements)
			{
				await TransformToTopologyElementAsync(element);
			}
		}
		private async Task<long> PutMeasurementsInElements(IMeasurement measurement)
		{
			string verboseMessage = $"{baseLogString} entering PutMeasurementsInElements method. Measurement GID {measurement?.Id:X16}.";
			Logger.LogVerbose(verboseMessage);

			var enumerableMeasurementToConnectedTerminalMap = await MeasurementToConnectedTerminalMap.GetEnumerableDictionaryAsync();
			if (enumerableMeasurementToConnectedTerminalMap.TryGetValue(measurement.Id, out long terminalId))
			{
				var enumerableTerminalToConnectedElementsMap = await TerminalToConnectedElementsMap.GetEnumerableDictionaryAsync();
				if (enumerableTerminalToConnectedElementsMap.TryGetValue(terminalId, out List<long> connectedElements))
				{
					try
					{
						var elementId = connectedElements.Find(
							e => GetDMSTypeOfTopologyElement(e) != DMSType.CONNECTIVITYNODE
							&& GetDMSTypeOfTopologyElement(e) != DMSType.ANALOG);

						var enumerableTopologyElements = await TopologyElements.GetEnumerableDictionaryAsync();
						if (enumerableTopologyElements.TryGetValue(elementId, out ITopologyElement element))
						{
							if(!element.Measurements.ContainsKey(measurement.Id))
                            {
								element.Measurements.Add(measurement.Id, measurement.GetMeasurementType());
                            }
							else
                            {
								Logger.LogWarning($"{baseLogString} PutMeasurementsInElements => element.Measurements contains key: 0x{measurement.Id:X16}");
                            }

							measurement.ElementId = elementId;

							if (measurement is DiscreteMeasurement)
							{
								var measurementProviderClient = MeasurementProviderClient.CreateClient();
								await measurementProviderClient.AddDiscreteMeasurement((DiscreteMeasurement)measurement);
							}

							if (measurement.GetMeasurementType().Equals(AnalogMeasurementType.FEEDER_CURRENT.ToString()))
							{
								await TopologyElements.SetAsync(elementId, new Feeder(element));
							}
							else
                            {
								await TopologyElements.SetAsync(elementId, element);
							}
						}
						else
						{
							Logger.LogError($"{baseLogString} PutMeasurementsInElement => Element with GID 0x{elementId:16X} does not exist in elements dictionary.");
						}
					}
					catch (Exception e)
					{
						Logger.LogError($"{baseLogString} PutMeasurementsInElement =>  {e.Message} {Environment.NewLine} {e.StackTrace}");
						//Logger.LogError($"{baseLogString} PutMeasurementsInElement =>  Failed to find appropriate element for mesuremnt with GID {measurement.Id:16X}. There is no conducting equipment connected to common terminal.");
					}
				}
				else
				{
					Logger.LogError($"{baseLogString} PutMeasurementsInElement => Terminal with GID 0x{terminalId:X16} does not exist in terminal to element map.");
				}
			}
			else
			{
				Logger.LogError($"{baseLogString} PutMeasurementsInElement => Measurement with GID {measurement.Id:X16} does not exist in mesurement to terminal map.");
			}
			return measurement.ElementId;
		}
		private async Task TransformToTopologyElementAsync(ResourceDescription modelEntity)
		{
			string verboseMessage = $"{baseLogString} entering TransformToTopologyElement method.";
			Logger.LogVerbose(verboseMessage);

			DMSType dmsType;
			dmsType = GetDMSTypeOfTopologyElement(modelEntity.Id);

			if (dmsType == DMSType.DISCRETE)
			{
				Measurement newDiscrete = await GetPopulatedDiscreteMeasurement(modelEntity);
				if (!await Measurements.ContainsKeyAsync(newDiscrete.Id))
				{
					await Measurements.SetAsync(newDiscrete.Id, newDiscrete); //contains moze da bude false, a da kad doje ova linija na red, da vrednost bude popunjena, zato SetAsync, ali onda je sam if suvisan (ne znam da li je kljucan za neku logiku...)
				}
				var measurementProviderClient = MeasurementProviderClient.CreateClient();
				await measurementProviderClient.AddDiscreteMeasurement(newDiscrete as DiscreteMeasurement);
			}
			else if (dmsType == DMSType.ANALOG)
			{
				Measurement newAnalog = await GetPopulatedAnalogMeasurement(modelEntity);
				if (!await Measurements.ContainsKeyAsync(newAnalog.Id))
				{
					await Measurements.SetAsync(newAnalog.Id, newAnalog); //contains moze da bude false, a da kad doje ova linija na red, da vrednost bude popunjena, zato SetAsync, ali onda je sam if suvisan (ne znam da li je kljucan za neku logiku...)
				}
				var measurementProviderClient = MeasurementProviderClient.CreateClient();
				await measurementProviderClient.AddAnalogMeasurement(newAnalog as AnalogMeasurement);
			}
			else if (dmsType != DMSType.MASK_TYPE && dmsType != DMSType.BASEVOLTAGE)
			{
				ITopologyElement newElement = await GetPopulatedElement(modelEntity);

				//lock (syncObj)
				//{
				if (!await TopologyElements.ContainsKeyAsync(newElement.Id))
				{
					await TopologyElements.SetAsync(newElement.Id, newElement); //contains moze da bude false, a da kad doje ova linija na red, da vrednost bude popunjena, zato SetAsync, ali onda je sam if suvisan (ne znam da li je kljucan za neku logiku...)
				}
				else
				{
					Logger.LogDebug($"{baseLogString} TransformToTopologyElementAsync => TopologyElements contain key {newElement.Id:X16}");
				}
				//}

				if (dmsType == DMSType.ENERGYSOURCE)
				{
					var energySourcesResult = await EnergySources.TryGetValueAsync(ReliableDictionaryNames.EnergySources);
					
					if(energySourcesResult.HasValue)
                    {
						var energySources = energySourcesResult.Value;
						energySources.Add(newElement.Id);

						await EnergySources.SetAsync(ReliableDictionaryNames.EnergySources, energySources);
					}
					else
                    {
						Logger.LogWarning($"{baseLogString} Reliable collection '{ReliableDictionaryNames.EnergySources}' was not defined yet. Handling...");
						await EnergySources.SetAsync(ReliableDictionaryNames.EnergySources, new List<long>() { newElement.Id });
					}
				}

				//lock (syncObj)
				//{
				if (!await ElementConnections.ContainsKeyAsync(modelEntity.Id))
				{
					await ElementConnections.SetAsync(modelEntity.Id, await GetAllReferencedElements(modelEntity)); //contains moze da bude false, a da kad doje ova linija na red, da vrednost bude popunjena, zato SetAsync, ali onda je sam if suvisan (ne znam da li je kljucan za neku logiku...)
				}
				else
				{
					Logger.LogDebug($"{baseLogString} TransformToTopologyElementAsync => ElementConnections contain key {modelEntity.Id:X16}");
				}
				//}
			}
		}
		private async Task<List<long>> GetAllReferencedElements(ResourceDescription element)
		{
			string verboseMessage = $"{baseLogString} entering GetAllReferencedElements method.";
			Logger.LogVerbose(verboseMessage);

			List<long> elements = new List<long>();
			DMSType type = GetDMSTypeOfTopologyElement(element.Id);

			foreach (var property in GetAllReferenceProperties(type))
			{
				if (property == ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS ||
					property == ModelCode.CONDUCTINGEQUIPMENT_TERMINALS ||
					property == ModelCode.CONNECTIVITYNODE_TERMINALS ||
					property == ModelCode.BASEVOLTAGE_CONDUCTINGEQUIPMENTS ||
					property == ModelCode.TERMINAL_MEASUREMENTS)
				{
					elements.AddRange(element.GetProperty(property).AsReferences());
				}
				else
				{
					var elementGid = element.GetProperty(property).AsReference();
					if (elementGid != 0)
					{
						elements.Add(elementGid);
					}
				}

			}

			if (type == DMSType.TERMINAL)
			{
				await TerminalToConnectedElementsMap.SetAsync(element.Id, new List<long>(elements));
			}
			return elements;
		}
		private List<ModelCode> GetAllReferenceProperties(DMSType type)
		{
			string verboseMessage = $"{baseLogString} entering GetAllReferenceProperties method.";
			Logger.LogVerbose(verboseMessage);

			List<ModelCode> properties = new List<ModelCode>();
			if (!ReferenceProperties.TryGetValue(type, out properties))
			{
				Logger.LogError($"{baseLogString} GetAllReferenceProperties => DMSType {type.ToString()} does not have declared reference properties.");
			}

			return properties;
		}
		private async Task<ITopologyElement> GetPopulatedElement(ResourceDescription rs)
		{
			string verboseMessage = $"{baseLogString} entering GetPopulatedElement method.";
			Logger.LogVerbose(verboseMessage);

			ITopologyElement topologyElement = new TopologyElement(rs.Id);
			try
			{
				DMSType type = GetDMSTypeOfTopologyElement(rs.Id);
				topologyElement.Mrid = rs.GetProperty(ModelCode.IDOBJ_MRID).AsString();
				topologyElement.Name = rs.GetProperty(ModelCode.IDOBJ_NAME).AsString();
				topologyElement.Description = rs.GetProperty(ModelCode.IDOBJ_DESCRIPTION).AsString();
				topologyElement.DmsType = type.ToString();

				if (rs.ContainsProperty(ModelCode.CONDUCTINGEQUIPMENT_ISREMOTE))
				{
					topologyElement.IsRemote = rs.GetProperty(ModelCode.CONDUCTINGEQUIPMENT_ISREMOTE).AsBool();
				}
				else
				{
					topologyElement.IsRemote = false;
				}

				if (rs.ContainsProperty(ModelCode.BREAKER_NORECLOSING))
				{
					topologyElement.NoReclosing = rs.GetProperty(ModelCode.BREAKER_NORECLOSING).AsBool();
					if (!topologyElement.NoReclosing)
					{
						topologyElement = new Recloser(topologyElement);
					}
				}
				else
				{
					topologyElement.NoReclosing = true;
				}

				if (rs.ContainsProperty(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE))
				{
					long baseVoltageGid = rs.GetProperty(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE).AsLong();

					var voltageResult = await BaseVoltages.TryGetValueAsync(baseVoltageGid);
					if (voltageResult.HasValue)
					{
						topologyElement.NominalVoltage = voltageResult.Value;
					}
					else if (baseVoltageGid == 0)
					{
						Logger.LogError($"{baseLogString} GetPopulatedElement => BaseVoltage with GID {baseVoltageGid:X16} does not exist in baseVoltages collection.");
					}
				}
				else
				{
					topologyElement.NominalVoltage = 0;
				}

				if (rs.ContainsProperty(ModelCode.BREAKER_NORECLOSING) && !rs.GetProperty(ModelCode.BREAKER_NORECLOSING).AsBool())
				{
					var reclosersResult = await Reclosers.TryGetValueAsync(ReliableDictionaryNames.Reclosers);

					if (reclosersResult.HasValue)
					{
						var reclosers = reclosersResult.Value;
						reclosers.Add(topologyElement.Id);

						await Reclosers.SetAsync(ReliableDictionaryNames.Reclosers, reclosers);
					}
					else
					{
						Logger.LogWarning($"{baseLogString} Reliable collection '{ReliableDictionaryNames.Reclosers}' was not defined yet. Handling...");
						await Reclosers.SetAsync(ReliableDictionaryNames.Reclosers, new HashSet<long>() { topologyElement.Id });
					}
				}

				if (rs.ContainsProperty(ModelCode.ENERGYCONSUMER_TYPE))
				{
					topologyElement = new EnergyConsumer(topologyElement)
					{
						Type = (EnergyConsumerType)rs.GetProperty(ModelCode.ENERGYCONSUMER_TYPE).AsEnum()
					};

				}

				if (type == DMSType.SYNCHRONOUSMACHINE)
				{
					topologyElement = new SynchronousMachine(topologyElement);

					if (rs.ContainsProperty(ModelCode.SYNCHRONOUSMACHINE_CAPACITY))
					{
						((SynchronousMachine)topologyElement).Capacity = rs.GetProperty(ModelCode.SYNCHRONOUSMACHINE_CAPACITY).AsFloat();
					}

					if (rs.ContainsProperty(ModelCode.SYNCHRONOUSMACHINE_CURRENTREGIME))
					{
						((SynchronousMachine)topologyElement).CurrentRegime = rs.GetProperty(ModelCode.SYNCHRONOUSMACHINE_CURRENTREGIME).AsFloat();
					}
				}
			}
			catch (Exception e)
			{
				Logger.LogError($"{baseLogString} GetPopulatedElement => Could not get all properties." +
					$"{Environment.NewLine}Excepiton message: {e.Message}" +
					$"{Environment.NewLine} Stack trace: {e.StackTrace}");
			}
			return topologyElement;
		}
		private async Task<AnalogMeasurement> GetPopulatedAnalogMeasurement(ResourceDescription rs)
		{
			string verboseMessage = $"{baseLogString} entering GetPopulatedAnalogMeasurement method.";
			Logger.LogVerbose(verboseMessage);

			AnalogMeasurement measurement = new AnalogMeasurement();
			try
			{
				measurement.Id = rs.Id;
				measurement.Address = rs.GetProperty(ModelCode.MEASUREMENT_ADDRESS).AsString();
				measurement.IsInput = rs.GetProperty(ModelCode.MEASUREMENT_ISINPUT).AsBool();
				measurement.CurrentValue = rs.GetProperty(ModelCode.ANALOG_CURRENTVALUE).AsFloat();
				measurement.MaxValue = rs.GetProperty(ModelCode.ANALOG_MAXVALUE).AsFloat();
				measurement.MinValue = rs.GetProperty(ModelCode.ANALOG_MINVALUE).AsFloat();
				measurement.NormalValue = rs.GetProperty(ModelCode.ANALOG_NORMALVALUE).AsFloat();
				measurement.Deviation = rs.GetProperty(ModelCode.ANALOG_DEVIATION).AsFloat();
				measurement.ScalingFactor = rs.GetProperty(ModelCode.ANALOG_SCALINGFACTOR).AsFloat();
				measurement.SignalType = (AnalogMeasurementType)rs.GetProperty(ModelCode.ANALOG_SIGNALTYPE).AsEnum();

				var connection = await GetAllReferencedElements(rs);
				if (connection.Count < 0)
				{
					Logger.LogError($"{baseLogString} GetPopulatedAnalogMeasurement => Analog measurement with GID: {rs.Id:X16} is not connected to any element.");

				}
				else if (connection.Count > 1)
				{
					Logger.LogWarning($"{baseLogString} GetPopulatedAnalogMeasurement => Analog measurement with GID: {rs.Id:X16} is connected to more then one element.");

					if (!await MeasurementToConnectedTerminalMap.ContainsKeyAsync(rs.Id))
					{
						await MeasurementToConnectedTerminalMap.SetAsync(rs.Id, connection.First());
					}

				}
				else
				{
					if (!await MeasurementToConnectedTerminalMap.ContainsKeyAsync(rs.Id))
					{
						await MeasurementToConnectedTerminalMap.SetAsync(rs.Id, connection.First());
					}
				}
			}
			catch (Exception e)
			{
				Logger.LogError($"{baseLogString} GetPopulatedAnalogMeasurement => Failed to populate analog measurement with GID {rs.Id:X16}." +
					$"{Environment.NewLine} Exception message: {e.Message}" +
					$"{Environment.NewLine} Stack trace: {e.StackTrace}");
			}
			return measurement;
		}
		private async Task<DiscreteMeasurement> GetPopulatedDiscreteMeasurement(ResourceDescription rs)
		{
			string verboseMessage = $"{baseLogString} entering GetPopulatedDiscreteMeasurement method.";
			Logger.LogVerbose(verboseMessage);

			DiscreteMeasurement measurement = new DiscreteMeasurement();
			try
			{
				measurement.Id = rs.Id;
				measurement.Address = rs.GetProperty(ModelCode.MEASUREMENT_ADDRESS).AsString();
				measurement.IsInput = rs.GetProperty(ModelCode.MEASUREMENT_ISINPUT).AsBool();
				measurement.CurrentOpen = rs.GetProperty(ModelCode.DISCRETE_CURRENTOPEN).AsBool();
				measurement.MaxValue = rs.GetProperty(ModelCode.DISCRETE_MAXVALUE).AsInt();
				measurement.MinValue = rs.GetProperty(ModelCode.DISCRETE_MINVALUE).AsInt();
				measurement.NormalValue = rs.GetProperty(ModelCode.DISCRETE_NORMALVALUE).AsInt();
				measurement.MeasurementType = (DiscreteMeasurementType)rs.GetProperty(ModelCode.DISCRETE_MEASUREMENTTYPE).AsEnum();

				var connection = await GetAllReferencedElements(rs);
				if (connection.Count < 0)
				{
					Logger.LogError($"{baseLogString} GetPopulatedDiscreteMeasurement => Discrete measurement with GID {rs.Id:X16} is not connected to any element.");

				}
				else if (connection.Count > 1)
				{
					Logger.LogWarning($"{baseLogString} GetPopulatedDiscreteMeasurement => Discrete measurement with GID {rs.Id:X16} is connected to more then one element.");
					if (!await MeasurementToConnectedTerminalMap.ContainsKeyAsync(rs.Id))
					{
						await MeasurementToConnectedTerminalMap.SetAsync(rs.Id, connection.First());
					}
				}
				else
				{
					if (!await MeasurementToConnectedTerminalMap.ContainsKeyAsync(rs.Id))
					{
						await MeasurementToConnectedTerminalMap.SetAsync(rs.Id, connection.First());
					}
				}
			}
			catch (Exception e)
			{
				Logger.LogError($"[NMSManager] Failed to populate discrete measurement with GID: {rs.Id:X16}."+
					$"{Environment.NewLine} Exception message: {e.Message}" +
					$"{Environment.NewLine} Stack trace: {e.StackTrace}");
			}
			return measurement;
		}
		private DMSType GetDMSTypeOfTopologyElement(long gid)
		{
			string verboseMessage = $"{baseLogString} entering GetDMSTypeOfTopologyElement method for GID {gid:X16}.";
			Logger.LogVerbose(verboseMessage);
			return (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gid);
		}
		private ArtificalDiscreteMeasurement GetNoScadaDiscreteMeasurement()
		{
			string verboseMessage = $"{baseLogString} entering GetNoScadaDiscreteMeasurement method.";
			Logger.LogVerbose(verboseMessage);

			ArtificalDiscreteMeasurement discreteMeasurement = new ArtificalDiscreteMeasurement()
			{
				Id = noScadaGuid++,
				Address = "",
				IsInput = false,
				CurrentOpen = false,
				MaxValue = 1,
				MinValue = 0,
				NormalValue = 0,
				MeasurementType = DiscreteMeasurementType.SWITCH_STATUS
			};
			return discreteMeasurement;
		}
		private async Task<ITopologyElement> CreateNoScadaMeasurementAsync(ITopologyElement element)
		{
			string verboseMessage = $"{baseLogString} entering CreateNoScadaMeasurement method.";
			Logger.LogVerbose(verboseMessage);

			DMSType dMSType = GetDMSTypeOfTopologyElement(element.Id);

			if (dMSType == DMSType.LOADBREAKSWITCH
				|| dMSType == DMSType.BREAKER
				|| dMSType == DMSType.FUSE
				|| dMSType == DMSType.DISCONNECTOR)
			{
				ArtificalDiscreteMeasurement measurement = GetNoScadaDiscreteMeasurement();
				element.Measurements.Add(measurement.Id, "SWITCH_STATUS");
				measurement.ElementId = element.Id;
				var measurementProviderClient = MeasurementProviderClient.CreateClient();
				await measurementProviderClient.AddDiscreteMeasurement(measurement);

				measurementProviderClient = MeasurementProviderClient.CreateClient();
				await measurementProviderClient.AddMeasurementElementPair(measurement.Id, element.Id);
			}

			return element;
		}
        #endregion
    }
}
