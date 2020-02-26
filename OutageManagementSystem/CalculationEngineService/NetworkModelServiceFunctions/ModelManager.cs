using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using CECommon.Providers;
using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkModelServiceFunctions
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
			ModelCode.ANALOG,
			ModelCode.DISCRETE
		};
		#endregion

		#region Fields
		ILogger logger = LoggerWrapper.Instance;
		private readonly ModelResourcesDesc modelResourcesDesc;
		private readonly NetworkModelGDA networkModelGDA;
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
			modelResourcesDesc = new ModelResourcesDesc();
			networkModelGDA = new NetworkModelGDA();

			TopologyElements = new Dictionary<long, ITopologyElement>();
			Measurements = new Dictionary<long, IMeasurement>();
			EnergySources = new List<long>();
			ElementConnections = new Dictionary<long, List<long>>();
			MeasurementToConnectedTerminalMap = new Dictionary<long, long>();
			TerminalToConnectedElementsMap = new Dictionary<long, List<long>>();
			BaseVoltages = new Dictionary<long, float>();
			Reclosers = new HashSet<long>();
		}

		#region Functions
		public bool TryGetAllModelEntities(
			out Dictionary<long, ITopologyElement> topologyElements, 
			out Dictionary<long, List<long>> elementConnections, 
			out HashSet<long> reclosers, 
			out List<long> energySources)
		{
			TopologyElements.Clear();
			Measurements.Clear();
			EnergySources.Clear();
			ElementConnections.Clear();
			MeasurementToConnectedTerminalMap.Clear();
			TerminalToConnectedElementsMap.Clear();
			BaseVoltages.Clear();
			Reclosers.Clear();

			bool success = true;
			try
			{
				logger.LogInfo("Getting all network model elements and converting them...");
				GetBaseVoltages();
				Parallel.For(0, ConcreteModels.Count, (i) =>
				{
					var model = ConcreteModels.ElementAt(i);
					if (model != ModelCode.BASEVOLTAGE)
					{
						List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(model);
						var elements = networkModelGDA.GetExtentValues(model, properties);
						foreach (var element in elements)
						{
							TransformToTopologyElement(element);
						}
					}
				});
				
				foreach (var measurement in Measurements.Values)
				{
					PutMeasurementsInElements(measurement);
					Provider.Instance.MeasurementProvider.AddMeasurementElementPair(measurement.Id, measurement.ElementId);
				}

				topologyElements = TopologyElements;
				elementConnections = ElementConnections;
				reclosers = Reclosers;
				energySources = EnergySources;
			}
			catch (Exception ex)
			{
				logger.LogError($"[NMSManager] Failed in get all network model elements. Exception message: {ex.Message}");
				topologyElements = null;
				elementConnections = null;
				reclosers = null;
				energySources = null;
				success = false;
			}
			return success;
		}
		private void GetBaseVoltages()
		{
			List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.BASEVOLTAGE);
			var elements = networkModelGDA.GetExtentValues(ModelCode.BASEVOLTAGE, properties);
			foreach (var element in elements)
			{
				TransformToTopologyElement(element);
			}
		}
		private void PutMeasurementsInElements(IMeasurement measurement)
		{
			string message = $"[NMSManager]Putting measurement with GID 0x{measurement.Id.ToString("X16")} in element.";
			if (MeasurementToConnectedTerminalMap.TryGetValue(measurement.Id, out long terminalId))
			{
				if (TerminalToConnectedElementsMap.TryGetValue(terminalId, out List<long> connectedElements))
				{
					try
					{
						var elementId = connectedElements.Find(
							e => GetDMSTypeOfTopologyElement(e) != DMSType.CONNECTIVITYNODE
							&& GetDMSTypeOfTopologyElement(e) != DMSType.ANALOG);

						if (TopologyElements.ContainsKey(elementId))
						{
							TopologyElements[elementId].Measurements.Add(measurement.Id);
							measurement.ElementId = elementId;
						}
						else
						{
							logger.LogWarn($"{message} Element with GID {elementId.ToString("X")} does not exist in elements dictionary.");
						}
					}
					catch (Exception)
					{
							logger.LogWarn($"{message} Failed to find appropriate element for mesuremnt with GID {measurement.Id.ToString("X")}. There is no conducting equipment connected to common terminal.");
					}
				}
				else
				{
					logger.LogWarn($"{message} Terminal with GID 0x{terminalId.ToString("X16")} does not exist in terminal to element map.");
				}
			}
			else
			{
				logger.LogWarn($"{message} Measurement with GID 0x{measurement.Id.ToString("X16")} does not exist in mesurement to terminal map.");
			}

		}
		private void TransformToTopologyElement(ResourceDescription modelEntity)
		{
			DMSType dmsType;
			dmsType = GetDMSTypeOfTopologyElement(modelEntity.Id);

			if (dmsType == DMSType.DISCRETE)
			{
				Measurement newDiscrete = GetPopulatedDiscreteMeasurement(modelEntity);
				Measurements.Add(newDiscrete.Id, newDiscrete);
				Provider.Instance.MeasurementProvider.AddDiscreteMeasurement(newDiscrete as DiscreteMeasurement);
			}
			else if (dmsType == DMSType.ANALOG)
			{
				Measurement newAnalog = GetPopulatedAnalogMeasurement(modelEntity);
				Measurements.Add(newAnalog.Id, newAnalog);
				Provider.Instance.MeasurementProvider.AddAnalogMeasurement(newAnalog as AnalogMeasurement);
			}
			else if (dmsType != DMSType.MASK_TYPE && dmsType != DMSType.BASEVOLTAGE)
			{
				ITopologyElement newElement = GetPopulatedElement(modelEntity);
				TopologyElements.Add(newElement.Id, newElement);
				if (dmsType == DMSType.ENERGYSOURCE)
				{
					EnergySources.Add(newElement.Id);
				}
				ElementConnections.Add(modelEntity.Id, (GetAllReferencedElements(modelEntity)));
			}
		}
		private List<long> GetAllReferencedElements(ResourceDescription element)
		{
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
			List<ModelCode> properties = new List<ModelCode>();
			if (!ReferenceProperties.TryGetValue(type, out properties))
			{
				logger.LogWarn($"[NMSManager] DMSType {type.ToString()} does not have declared reference properties.");
			}

			return properties;
		}
		private ITopologyElement GetPopulatedElement(ResourceDescription rs)
		{
			string errorMessage = $"[NMSManager] Failed to populate element with GID 0x{rs.Id:X16}. ";
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
						logger.LogError($"{errorMessage} BaseVoltage with GID 0x{baseVoltageGid.ToString("X16")} does not exist in baseVoltages collection.");
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
			}
			catch (Exception ex)
			{
				logger.LogError($"{errorMessage} Could not get all properties.Excepiton message: {ex.Message}");
			}		
			return topologyElement;
		}
		private AnalogMeasurement GetPopulatedAnalogMeasurement(ResourceDescription rs)
		{
			AnalogMeasurement measurement = new AnalogMeasurement();
			try
			{
				measurement.Id = rs.Id;
				measurement.Address = rs.GetProperty(ModelCode.MEASUREMENT_ADDRESS).AsString();
				measurement.isInput = rs.GetProperty(ModelCode.MEASUREMENT_ISINPUT).AsBool();
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
					logger.LogWarn($"Analog measurement with GID: 0x{rs.Id:X16} is not connected to any element.");

				}
				else if (connection.Count > 1)
				{
					logger.LogWarn($"Analog measurement with GID: 0x{rs.Id:X16} is connected to more then one element.");
					MeasurementToConnectedTerminalMap.Add(rs.Id, connection.First());
				}
				else
				{
					MeasurementToConnectedTerminalMap.Add(rs.Id, connection.First());
				}
			}
			catch (Exception)
			{
				logger.LogDebug($"Failed to populate analog measurement with GID: 0x{rs.Id:X16}.");
			}
			return measurement;
		}
		private DiscreteMeasurement GetPopulatedDiscreteMeasurement(ResourceDescription rs)
		{
			DiscreteMeasurement measurement = new DiscreteMeasurement();
			try
			{
				measurement.Id = rs.Id;
				measurement.Address = rs.GetProperty(ModelCode.MEASUREMENT_ADDRESS).AsString();
				measurement.isInput = rs.GetProperty(ModelCode.MEASUREMENT_ISINPUT).AsBool();
				measurement.CurrentOpen = rs.GetProperty(ModelCode.DISCRETE_CURRENTOPEN).AsBool();
				measurement.MaxValue = rs.GetProperty(ModelCode.DISCRETE_MAXVALUE).AsInt();
				measurement.MinValue = rs.GetProperty(ModelCode.DISCRETE_MINVALUE).AsInt();
				measurement.NormalValue = rs.GetProperty(ModelCode.DISCRETE_NORMALVALUE).AsInt();
				measurement.MeasurementType = (DiscreteMeasurementType)rs.GetProperty(ModelCode.DISCRETE_MEASUREMENTTYPE).AsEnum();

				var connection = GetAllReferencedElements(rs);
				if (connection.Count < 0)
				{
					logger.LogWarn($"[NMSManager] Discrete measurement with GID: 0x{rs.Id:X16} is not connected to any element.");

				}
				else if (connection.Count > 1)
				{
					logger.LogWarn($"[NMSManager] Discrete measurement with GID: 0x{rs.Id:X16} is connected to more then one element.");
					MeasurementToConnectedTerminalMap.Add(rs.Id, connection.First());
				}
				else
				{
					MeasurementToConnectedTerminalMap.Add(rs.Id, connection.First());
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"[NMSManager] Failed to populate discrete measurement with GID: 0x{rs.Id:X16}. Exception message: {ex.Message}");
			}
			return measurement;
		}
		private DMSType GetDMSTypeOfTopologyElement(long gid)
		{	
			return (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gid);
		}
        #endregion
    }
}
