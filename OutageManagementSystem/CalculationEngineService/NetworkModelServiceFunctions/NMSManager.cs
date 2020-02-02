using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using CECommon.Providers;
using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkModelServiceFunctions
{
	public class NMSManager : IModelManager
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
		private Dictionary<long, IMeasurement> measurements;
		private Dictionary<long, ITopologyElement> elements;
		private List<long> energySources;
		private Dictionary<long, List<long>> allElementConnections;
		private Dictionary<long, long> measurementToConnectedTerminalMap;
		private Dictionary<long, List<long>> terminalToConnectedElementsMap;
		private Dictionary<long, float> baseVoltages;
		#endregion

		public NMSManager()
		{
			modelResourcesDesc = new ModelResourcesDesc();
			networkModelGDA = new NetworkModelGDA();
			GetAllModelEntities();
		}

		#region Funcions
		public List<long> GetAllEnergySources()
		{
			logger.LogDebug("[NMSManager] Returning all energy sources.");
			if (energySources != null)
			{
				return energySources;
			}
			else
			{
				return new List<long>();
			}
		}
		private void GetAllModelEntities()
		{
			elements = new Dictionary<long, ITopologyElement>();
			measurements = new Dictionary<long, IMeasurement>();
			energySources = new List<long>();
			allElementConnections = new Dictionary<long, List<long>>();
			measurementToConnectedTerminalMap = new Dictionary<long, long>();
			terminalToConnectedElementsMap = new Dictionary<long, List<long>>();
			baseVoltages = new Dictionary<long, float>();
			
			try
			{
				foreach (var item in ConcreteModels)
				{
					List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(item);
					var elements = networkModelGDA.GetExtentValues(item, properties);
					foreach (var element in elements)
					{
						TransformToTopologyElements(element);
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"[NMSManager] Failed in get all model entities. Exception message: {ex.Message}");
			}
		}
		private void PutMeasurementsInElements(Measurement measurement)
		{
			string message = $"[NMSManager]Putting measurement with GID {measurement.Id.ToString("X")} in element.";
			if (measurementToConnectedTerminalMap.TryGetValue(measurement.Id, out long terminalId))
			{
				if (terminalToConnectedElementsMap.TryGetValue(terminalId, out List<long> connectedElements))
				{
					try
					{
						var elementId = connectedElements.Find(e => GetDMSTypeOfTopologyElement(e) != DMSType.CONNECTIVITYNODE
								&& GetDMSTypeOfTopologyElement(e) != DMSType.ANALOG);
						if (elements.ContainsKey(elementId))
						{
							elements[elementId].Measurements.Add(measurement);
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
					logger.LogWarn($"{message} Terminal with GID {terminalId.ToString("X")} does not exist in terminal to element map.");
				}
			}
			else
			{
				logger.LogWarn($"{message} Measurement with GID {measurement.Id.ToString("X")} does not exist in mesurement to terminal map.");
			}

		}
		private void TransformToTopologyElements(ResourceDescription modelEntity)
		{
			DMSType dmsType;
			dmsType = GetDMSTypeOfTopologyElement(modelEntity.Id);

			if (dmsType == DMSType.DISCRETE)
			{
				Measurement newDiscrete = GetPopulatedDiscreteMeasurement(modelEntity);
				measurements.Add(newDiscrete.Id, newDiscrete);
				PutMeasurementsInElements(newDiscrete);
				Provider.Instance.CacheProvider.AddDiscreteMeasurement(newDiscrete as DiscreteMeasurement);
			}
			else if (dmsType == DMSType.ANALOG)
			{
				Measurement newAnalog = GetPopulatedAnalogMeasurement(modelEntity);
				PutMeasurementsInElements(newAnalog);
				measurements.Add(newAnalog.Id, newAnalog);
				Provider.Instance.CacheProvider.AddAnalogMeasurement(newAnalog as AnalogMeasurement);
			}
			else if (dmsType == DMSType.BASEVOLTAGE)
			{
				float voltage = modelEntity.GetProperty(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE).AsFloat();
				long baseVoltageGid = modelEntity.GetProperty(ModelCode.IDOBJ_GID).AsLong();
				if (baseVoltages.ContainsKey(baseVoltageGid))
				{
					logger.LogDebug($"[NMSManager] Basevoltage with GID {baseVoltageGid.ToString("X")} is already in basevoltage collection. Elements can share basevoltage.");
				}
				else
				{
					baseVoltages.Add(baseVoltageGid, voltage);
				}
			}
			else if (dmsType != DMSType.MASK_TYPE)
			{
				ITopologyElement newElement = GetPopulatedElement(modelEntity);
				elements.Add(newElement.Id, newElement);
				if (dmsType == DMSType.ENERGYSOURCE)
				{
					energySources.Add(newElement.Id);
				}
				allElementConnections.Add(modelEntity.Id, (GetAllReferencedElements(modelEntity)));
			}
		}
		public void GetAllModels(out Dictionary<long, ITopologyElement> elements, out Dictionary<long, IMeasurement> measurements, out Dictionary<long, List<long>> connections)
		{
			elements = this.elements;
			measurements = this.measurements;
			connections = allElementConnections;
		}
		public void PrepareTransaction()
		{
			GetAllModelEntities();
		}
		private List<long> GetAllReferencedElements(ResourceDescription element)
		{
			logger.LogDebug($"[NMSManager] Getting all referenced elements for GID {element.Id}.");
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
				terminalToConnectedElementsMap.Add(element.Id, new List<long>(elements));
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
			//return ReferenceProperties[type];
			//List<ModelCode> propertyIds = new List<ModelCode>();

			//switch (type)
			//{
			//	case DMSType.TERMINAL:
			//		propertyIds.Add(ModelCode.TERMINAL_CONDUCTINGEQUIPMENT);
			//		propertyIds.Add(ModelCode.TERMINAL_CONNECTIVITYNODE);
			//		propertyIds.Add(ModelCode.TERMINAL_MEASUREMENTS);
			//		break;
			//	case DMSType.CONNECTIVITYNODE:
			//		propertyIds.Add(ModelCode.CONNECTIVITYNODE_TERMINALS);
			//		break;
			//	case DMSType.POWERTRANSFORMER:
			//		propertyIds.Add(ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS);
			//		break;
			//	case DMSType.ENERGYSOURCE:
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
			//		break;
			//	case DMSType.ENERGYCONSUMER:
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
			//		break;
			//	case DMSType.TRANSFORMERWINDING:
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
			//		propertyIds.Add(ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER);
			//		break;
			//	case DMSType.FUSE:
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
			//		break;
			//	case DMSType.DISCONNECTOR:
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
			//		break;
			//	case DMSType.BREAKER:
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
			//		break;
			//	case DMSType.LOADBREAKSWITCH:
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
			//		break;
			//	case DMSType.ACLINESEGMENT:
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
			//		propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
			//		break;
			//	case DMSType.ANALOG:
			//		propertyIds.Add(ModelCode.MEASUREMENT_TERMINAL);
			//		break;
			//	case DMSType.DISCRETE:
			//		propertyIds.Add(ModelCode.MEASUREMENT_TERMINAL);
			//		break;
			//	case DMSType.BASEVOLTAGE:
			//		propertyIds.Add(ModelCode.BASEVOLTAGE_CONDUCTINGEQUIPMENTS);
			//		break;
			//	default:
			//		break;
			//}

			//return propertyIds;
		}
		public ITopologyElement GetPopulatedElement(ResourceDescription rs)
		{
			string errorMessage = $"[NMSManager] Failed to populate element with GID {rs.Id}. ";
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

				if (rs.ContainsProperty(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE))
				{
					long baseVoltageGid = rs.GetProperty(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE).AsLong();
					if (baseVoltages.TryGetValue(baseVoltageGid,out float voltage))
					{
						topologyElement.NominalVoltage = voltage;
					}
					else
					{
						logger.LogError($"{errorMessage} BaseVoltage with GID {baseVoltageGid.ToString("X")} does not exist in baseVoltages collection.");
					}
				}
				else
				{
					topologyElement.NominalVoltage = 0;
					logger.LogError($"{errorMessage} Failed to get BaseVoltage. Element with GID {rs.Id} does not have BASEVOLTAGE_NOMINALVOLTAGE property.");
				}			
			}
			catch (Exception ex)
			{
				logger.LogError($"{errorMessage} Could not get all properties.Excepiton message: {ex.Message}");
			}		
			return topologyElement;
		}
		public AnalogMeasurement GetPopulatedAnalogMeasurement(ResourceDescription rs)
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
					logger.LogWarn($"Analog measurement with GID: {rs.Id} is not connected to any element.");

				}
				else if (connection.Count > 1)
				{
					logger.LogWarn($"Analog measurement with GID: {rs.Id} is connected to more then one element.");
					measurementToConnectedTerminalMap.Add(rs.Id, connection.First());
				}
				else
				{
					measurementToConnectedTerminalMap.Add(rs.Id, connection.First());
				}
			}
			catch (Exception)
			{
				logger.LogDebug($"Failed to populate analog measurement with GID: {rs.Id}.");
			}
			return measurement;
		}
		public DiscreteMeasurement GetPopulatedDiscreteMeasurement(ResourceDescription rs)
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
					logger.LogWarn($"[NMSManager] Discrete measurement with GID: {rs.Id} is not connected to any element.");

				}
				else if (connection.Count > 1)
				{
					logger.LogWarn($"[NMSManager] Discrete measurement with GID: {rs.Id} is connected to more then one element.");
					measurementToConnectedTerminalMap.Add(rs.Id, connection.First());
				}
				else
				{
					measurementToConnectedTerminalMap.Add(rs.Id, connection.First());
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"[NMSManager] Failed to populate discrete measurement with GID: {rs.Id}. Exception message: {ex.Message}");
			}
			return measurement;
		}
		public DMSType GetDMSTypeOfTopologyElement(long gid)
		{	
			return (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gid);
		}
		public string GetDMSTypeOfTopologyElementString(long gid)
		{
			logger.LogDebug($"Getting element DMStype for GID {gid}.");		
			DMSType type = GetDMSTypeOfTopologyElement(gid);
			if (type != 0)
			{
				return type.ToString();
			}
			else
			{
				return "FIELD";
			}			
		}
        #endregion
    }
}
