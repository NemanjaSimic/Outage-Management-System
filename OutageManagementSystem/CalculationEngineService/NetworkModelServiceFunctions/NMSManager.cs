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
	public class NMSManager : IModelManager
	{
		#region ReferenceProperties
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
		#endregion

		#region Fields
		ILogger logger = LoggerWrapper.Instance;
		private TransactionFlag transactionFlag;
		private readonly ModelResourcesDesc modelResourcesDesc;
		private readonly NetworkModelGDA networkModelGDA;
		private Dictionary<long, IMeasurement> measurements;
		private Dictionary<long, ITopologyElement> elements;
		private List<long> energySources;
		private Dictionary<long, List<long>> allElementConnections;
		private Dictionary<long, long> measurementConnection;
		#endregion

		public NMSManager()
		{
			modelResourcesDesc = new ModelResourcesDesc();
			networkModelGDA = new NetworkModelGDA();
			transactionFlag = TransactionFlag.NoTransaction;
			Initialize();
		}

		public List<long> GetAllEnergySources()
		{
			logger.LogDebug("Getting all energy sources.");
			return energySources;
			//List<long> gids = new List<long>();
			//Dictionary<long, ResourceDescription> entities = GetEntities();

			//foreach (var pair in entities)
			//{
			//	if (modelResourcesDesc.GetModelCodeFromId(pair.Key) == ModelCode.ENERGYSOURCE)
			//	{
			//		gids.Add(pair.Value.GetProperty(ModelCode.IDOBJ_GID).AsLong());
			//	}
			//}
			//return gids;
		}
		public void Initialize()
		{
			GetAllModelEntities(); 
			//PutMeasurementsInElements();
		}
		private void PutMeasurementsInElements()
		{
			foreach (var pair in measurements)
			{
				if (measurementConnection.ContainsKey(pair.Key))
				{
					var connectedEl = measurementConnection[pair.Key];
					
					elements[measurementConnection[pair.Key]].Measurements.Add(pair.Value);
				}
			}
		}
		private void TransformToTopologyElements(ResourceDescription modelEntity)
		{
			DMSType dmsType;
			dmsType = GetDMSTypeOfTopologyElement(modelEntity.Id);
			if (dmsType == DMSType.DISCRETE)
			{
				Measurement newDiscrete = GetPopulatedDiscreteMeasurement(modelEntity);
				Provider.Instance.CacheProvider.AddDiscreteMeasurement(newDiscrete as DiscreteMeasurement);
				measurements.Add(newDiscrete.Id, newDiscrete);
			}
			else if (dmsType == DMSType.ANALOG)
			{
				Measurement newAnalog = GetPopulatedAnalogMeasurement(modelEntity);
				Provider.Instance.CacheProvider.AddAnalogMeasurement(newAnalog as AnalogMeasurement);
				measurements.Add(newAnalog.Id, newAnalog);
			}
			else if (dmsType != DMSType.BASEVOLTAGE && dmsType != DMSType.MASK_TYPE)
			{
				ITopologyElement newElement = GetPopulatedElement(modelEntity);
				elements.Add(newElement.Id, newElement);
				if (dmsType == DMSType.ENERGYSOURCE)
				{
					energySources.Add(newElement.Id);
				}
			}
			allElementConnections.Add(modelEntity.Id, (GetAllReferencedElements(modelEntity)));
		}
		private void GetAllModelEntities()
		{
			elements = new Dictionary<long, ITopologyElement>();
			measurements = new Dictionary<long, IMeasurement>();
			energySources = new List<long>();
			allElementConnections = new Dictionary<long, List<long>>();
			measurementConnection = new Dictionary<long, long>();
			List<ModelCode> concreteClasses = modelResourcesDesc.NonAbstractClassIds;

			try
			{
				foreach (var item in concreteClasses)
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
				logger.LogError($"Failed to initialize NMSManager. Exception message: {ex.Message}");
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
			logger.LogDebug($"Getting all referenced elements for GID {element.Id}.");
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
			return elements;
		}
		private List<ModelCode> GetAllReferenceProperties(DMSType type)
		{
			//return ReferenceProperties[type];
			List<ModelCode> propertyIds = new List<ModelCode>();

			switch (type)
			{
				case DMSType.TERMINAL:
					propertyIds.Add(ModelCode.TERMINAL_CONDUCTINGEQUIPMENT);
					propertyIds.Add(ModelCode.TERMINAL_CONNECTIVITYNODE);
					propertyIds.Add(ModelCode.TERMINAL_MEASUREMENTS);
					break;
				case DMSType.CONNECTIVITYNODE:
					propertyIds.Add(ModelCode.CONNECTIVITYNODE_TERMINALS);
					break;
				case DMSType.POWERTRANSFORMER:
					propertyIds.Add(ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS);
					break;
				case DMSType.ENERGYSOURCE:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
					break;
				case DMSType.ENERGYCONSUMER:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
					break;
				case DMSType.TRANSFORMERWINDING:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					propertyIds.Add(ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER);
					break;
				case DMSType.FUSE:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					break;
				case DMSType.DISCONNECTOR:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					break;
				case DMSType.BREAKER:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					break;
				case DMSType.LOADBREAKSWITCH:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					break;
				case DMSType.ACLINESEGMENT:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE);
					break;
				case DMSType.ANALOG:
					propertyIds.Add(ModelCode.MEASUREMENT_TERMINAL);
					break;
				case DMSType.DISCRETE:
					propertyIds.Add(ModelCode.MEASUREMENT_TERMINAL);
					break;
				case DMSType.BASEVOLTAGE:
					propertyIds.Add(ModelCode.BASEVOLTAGE_CONDUCTINGEQUIPMENTS);
					break;
				default:
					break;
			}

			return propertyIds;
		}
		public ITopologyElement GetPopulatedElement(ResourceDescription rs)
		{
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

				if (rs.ContainsProperty(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE))
				{
					topologyElement.NominalVoltage = rs.GetProperty(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE).AsFloat();
				}
				else
				{
					topologyElement.NominalVoltage = 0;
					logger.LogWarn($"Failed to get BaseVoltage. Element with GID {rs.Id} does not have BASEVOLTAGE_NOMINALVOLTAGE property.");
				}			
			}
			catch (Exception)
			{
				logger.LogError($"Failed to populate element with GID {topologyElement.Id}. Could not get all properties.");
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
					logger.LogWarn($"Discrete measurement with GID: {rs.Id} is not connected to any element.");

				}
				else if (connection.Count > 1)
				{
					logger.LogWarn($"Discrete measurement with GID: {rs.Id} is connected to more then one element.");
					measurementConnection.Add(rs.Id, connection.First());
				}
				else
				{
					measurementConnection.Add(rs.Id, connection.First());
				}
			}
			catch (Exception)
			{
				logger.LogError($"Failed to populate discrete measurement with GID: {rs.Id}.");
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
	}
}
