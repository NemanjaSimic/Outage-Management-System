using Common.CeContracts;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.WcfClient.CE;
using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly string baseLogString;
		private readonly object syncObj = new object();
		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		private readonly NetworkModelGDA networkModelGda;
		private readonly IMeasurementProviderContract measurementProviderClient;

		private static long noScadaGuid = 1;
		private readonly ModelResourcesDesc modelResourcesDesc;
		private Dictionary<long, IMeasurement> Measurements { get; set; }
		private Dictionary<long, ITopologyElement> TopologyElements { get; set; }
		private List<long> EnergySources { get; set; }
		private Dictionary<long, float> BaseVoltages { get; set; }
		private HashSet<long> Reclosers { get; set; }
		private Dictionary<long, List<long>> ElementConnections { get; set; }
		private Dictionary<long, long> MeasurementToConnectedTerminalMap { get; set; }
		private Dictionary<long, List<long>> TerminalToConnectedElementsMap { get; set; }
		#endregion

		public ModelManager()
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);

			networkModelGda = new NetworkModelGDA();
			measurementProviderClient = MeasurementProviderClient.CreateClient();

			modelResourcesDesc = new ModelResourcesDesc();
			TopologyElements = new Dictionary<long, ITopologyElement>();
			Measurements = new Dictionary<long, IMeasurement>();
			EnergySources = new List<long>();
			ElementConnections = new Dictionary<long, List<long>>();
			MeasurementToConnectedTerminalMap = new Dictionary<long, long>();
			TerminalToConnectedElementsMap = new Dictionary<long, List<long>>();
			BaseVoltages = new Dictionary<long, float>();
			Reclosers = new HashSet<long>();

			string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
			Logger.LogDebug(debugMessage);
		}

		#region Functions
		public async Task<IModelDelta> TryGetAllModelEntitiesAsync()
		{
			string verboseMessage = $"{baseLogString} entering TryGetAllModelEntities method.";
			Logger.LogVerbose(verboseMessage);

			ModelDelta modelDelta = new ModelDelta();

			TopologyElements.Clear();
			Measurements.Clear();
			EnergySources.Clear();
			ElementConnections.Clear();
			MeasurementToConnectedTerminalMap.Clear();
			TerminalToConnectedElementsMap.Clear();
			BaseVoltages.Clear();
			Reclosers.Clear();

			try
			{
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

				foreach (var measurement in Measurements.Values)
				{
					PutMeasurementsInElements(measurement);
					await measurementProviderClient.AddMeasurementElementPair(measurement.Id, measurement.ElementId);
				}

				foreach (var element in TopologyElements.Values)
				{
					if (element.Measurements.Count == 0)
					{
						await CreateNoScadaMeasurementAsync(element);
					}
				}

				modelDelta.TopologyElements = TopologyElements;
				modelDelta.ElementConnections = ElementConnections;
				modelDelta.Reclosers = Reclosers;
				modelDelta.EnergySources = EnergySources;
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
		private void PutMeasurementsInElements(IMeasurement measurement)
		{
			string verboseMessage = $"{baseLogString} entering PutMeasurementsInElements method. Measurement GID {measurement?.Id:X16}.";
			Logger.LogVerbose(verboseMessage);

			if (MeasurementToConnectedTerminalMap.TryGetValue(measurement.Id, out long terminalId))
			{
				if (TerminalToConnectedElementsMap.TryGetValue(terminalId, out List<long> connectedElements))
				{
					try
					{
						var elementId = connectedElements.Find(
							e => GetDMSTypeOfTopologyElement(e) != DMSType.CONNECTIVITYNODE
							&& GetDMSTypeOfTopologyElement(e) != DMSType.ANALOG);

						if (TopologyElements.TryGetValue(elementId, out ITopologyElement element))
						{
							element.Measurements.Add(measurement.Id, measurement.GetMeasurementType());
							measurement.ElementId = elementId;

							if (measurement.GetMeasurementType().Equals(AnalogMeasurementType.FEEDER_CURRENT.ToString()))
							{
								TopologyElements[elementId] = new Feeder(element);
							}
						}
						else
						{
							Logger.LogError($"{baseLogString} PutMeasurementsInElement => Element with GID {elementId:16X} does not exist in elements dictionary.");
						}
					}
					catch (Exception)
					{
						Logger.LogError($"{baseLogString} PutMeasurementsInElement =>  Failed to find appropriate element for mesuremnt with GID {measurement.Id:16X}. There is no conducting equipment connected to common terminal.");
					}
				}
				else
				{
					Logger.LogError($"{baseLogString} PutMeasurementsInElement => Terminal with GID {terminalId:X16} does not exist in terminal to element map.");
				}
			}
			else
			{
				Logger.LogError($"{baseLogString} PutMeasurementsInElement => Measurement with GID {measurement.Id:X16} does not exist in mesurement to terminal map.");
			}
		}
		private async Task TransformToTopologyElementAsync(ResourceDescription modelEntity)
		{
			string verboseMessage = $"{baseLogString} entering TransformToTopologyElement method.";
			Logger.LogVerbose(verboseMessage);

			DMSType dmsType;
			dmsType = GetDMSTypeOfTopologyElement(modelEntity.Id);

			if (dmsType == DMSType.DISCRETE)
			{
				Measurement newDiscrete = GetPopulatedDiscreteMeasurement(modelEntity);
				Measurements.Add(newDiscrete.Id, newDiscrete);
				await measurementProviderClient.AddDiscreteMeasurement(newDiscrete as DiscreteMeasurement);
			}
			else if (dmsType == DMSType.ANALOG)
			{
				Measurement newAnalog = GetPopulatedAnalogMeasurement(modelEntity);
				Measurements.Add(newAnalog.Id, newAnalog);
				await measurementProviderClient.AddAnalogMeasurement(newAnalog as AnalogMeasurement);
			}
			else if (dmsType != DMSType.MASK_TYPE && dmsType != DMSType.BASEVOLTAGE)
			{
				ITopologyElement newElement = GetPopulatedElement(modelEntity);

				lock (syncObj)
				{
					if (!TopologyElements.ContainsKey(newElement.Id))
					{
						TopologyElements.Add(newElement.Id, newElement);
					}
					else
					{
						Logger.LogDebug($"{baseLogString} TransformToTopologyElementAsync => TopologyElements contain key {newElement.Id:X16}");
					}

				}


				if (dmsType == DMSType.ENERGYSOURCE)
				{
					EnergySources.Add(newElement.Id);
				}
				lock (syncObj)
				{
					if (!ElementConnections.ContainsKey(modelEntity.Id))
					{
						ElementConnections.Add(modelEntity.Id, (GetAllReferencedElements(modelEntity)));
					}
					else
					{
						Logger.LogDebug($"{baseLogString} TransformToTopologyElementAsync => ElementConnections contain key {modelEntity.Id:X16}");
					}
				}
			}
		}
		private List<long> GetAllReferencedElements(ResourceDescription element)
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
				TerminalToConnectedElementsMap.Add(element.Id, new List<long>(elements));
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
		private ITopologyElement GetPopulatedElement(ResourceDescription rs)
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
					if (BaseVoltages.TryGetValue(baseVoltageGid, out float voltage))
					{
						topologyElement.NominalVoltage = voltage;
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
					Reclosers.Add(topologyElement.Id);
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
		private AnalogMeasurement GetPopulatedAnalogMeasurement(ResourceDescription rs)
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

				var connection = GetAllReferencedElements(rs);
				if (connection.Count < 0)
				{
					Logger.LogError($"{baseLogString} GetPopulatedAnalogMeasurement => Analog measurement with GID: {rs.Id:X16} is not connected to any element.");

				}
				else if (connection.Count > 1)
				{
					Logger.LogWarning($"{baseLogString} GetPopulatedAnalogMeasurement => Analog measurement with GID: {rs.Id:X16} is connected to more then one element.");
					MeasurementToConnectedTerminalMap.Add(rs.Id, connection.First());
				}
				else
				{
					MeasurementToConnectedTerminalMap.Add(rs.Id, connection.First());
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
		private DiscreteMeasurement GetPopulatedDiscreteMeasurement(ResourceDescription rs)
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

				var connection = GetAllReferencedElements(rs);
				if (connection.Count < 0)
				{
					Logger.LogError($"{baseLogString} GetPopulatedDiscreteMeasurement => Discrete measurement with GID {rs.Id:X16} is not connected to any element.");

				}
				else if (connection.Count > 1)
				{
					Logger.LogWarning($"{baseLogString} GetPopulatedDiscreteMeasurement => Discrete measurement with GID {rs.Id:X16} is connected to more then one element.");
					MeasurementToConnectedTerminalMap.Add(rs.Id, connection.First());
				}
				else
				{
					MeasurementToConnectedTerminalMap.Add(rs.Id, connection.First());
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
		private async Task CreateNoScadaMeasurementAsync(ITopologyElement element)
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
				await measurementProviderClient.AddDiscreteMeasurement(measurement);
				await measurementProviderClient.AddMeasurementElementPair(measurement.Id, element.Id);
			}
		}
        #endregion
    }
}
