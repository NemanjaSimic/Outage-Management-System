using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkModelServiceFunctions
{
	public class NMSManager
	{
		#region Fields
		ILogger logger = LoggerWrapper.Instance;
		private TransactionFlag transactionFlag;
        private readonly ModelResourcesDesc modelResourcesDesc;
		private readonly NetworkModelGDA networkModelGDA;
		private Dictionary<long, ResourceDescription> modelEntities;
		private Dictionary<long, ResourceDescription> transactionModelEntities;
		#endregion

		#region Singleton
		private static object syncObj = new object();
		private static NMSManager instance;

        public static NMSManager Instance
		{
			get 
			{
				lock (syncObj)
				{
					if (instance == null)
					{
						instance = new NMSManager();
					}
				}
				return instance;
			}
		}
        #endregion
        private NMSManager()
		{
			modelResourcesDesc = new ModelResourcesDesc();
			networkModelGDA = new NetworkModelGDA();
			transactionFlag = TransactionFlag.NoTransaction;
		}

		private Dictionary<long, ResourceDescription> GetEntities()
		{
			Dictionary<long, ResourceDescription> entities;
			if (transactionFlag == TransactionFlag.InTransaction)
			{
				entities = transactionModelEntities;
			}
			else
			{
				entities = modelEntities;
			}
			return entities;
		}

		public List<long> GetAllEnergySources()
		{
			logger.LogDebug("Getting all energy sources.");
			List<long> gids = new List<long>();
			Dictionary<long, ResourceDescription> entities = GetEntities();

			foreach (var pair in entities)
			{
				if (modelResourcesDesc.GetModelCodeFromId(pair.Key) == ModelCode.ENERGYSOURCE)
				{
					gids.Add(pair.Value.GetProperty(ModelCode.IDOBJ_GID).AsLong());
				}
			}
			return gids;
		}
		public void Initialize()
		{
			modelEntities = GetAllModelEntities();
		}

		//private Dictionary<long, IGraphElement> TransformToTopologyElements(Dictionary<long,ResourceDescription> modelEntities)
		//{
		//	Dictionary<long, IGraphElement> elements = new Dictionary<long, IGraphElement>();

		//	DMSType dmsType;
		//	foreach (var modelEntity in modelEntities.Keys)
		//	{
		//		dmsType = GetDMSTypeOfTopologyElement(modelEntity);
		//		if (dmsType == DMSType.DISCRETE)
		//		{
		//			Measurement newDiscrete = GetPopulatedDiscreteMeasurement(modelEntity);
		//			elements.Add(newDiscrete.Id, newDiscrete);
		//		}
		//		else if (dmsType == DMSType.ANALOG)
		//		{
		//			Measurement newAnalog = GetPopulatedAnalogMeasurement(modelEntity);
		//			elements.Add(newAnalog.Id, newAnalog);
		//		}
		//		else if (dmsType == DMSType.BASEVOLTAGE)
		//		{
		//			elements.Add(modelEntity, GetBaseVoltageForElement(modelEntity));
		//		}
		//		else if (dmsType != DMSType.TERMINAL && dmsType != DMSType.CONNECTIVITYNODE && dmsType != DMSType.MASK_TYPE)
		//		{
		//			ITopologyElement newElement = GetPopulatedElement(modelEntity);
		//			elements.Add(newElement.Id, newElement);
		//		}
		//	}
		//	return elements;
		//}
		public Dictionary<long, ResourceDescription> GetAllModelEntities()
		{
			List<ModelCode> concreteClasses = modelResourcesDesc.NonAbstractClassIds;
			Dictionary<long, ResourceDescription>  modelEntities = new Dictionary<long, ResourceDescription>();
			try
			{
				foreach (var item in concreteClasses)
				{
					List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(item);
					var elements = networkModelGDA.GetExtentValues(item, properties);
					foreach (var element in elements)
					{
						modelEntities.Add(element.Id, element);
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to initialize NMSManager. Exception message: {ex.Message}");
			}
			return modelEntities;
		}
		public List<long> GetAllReferencedElements(long gid)
		{
			logger.LogDebug($"Getting all referenced elements for GID {gid}.");
			List<long> elements = new List<long>();
			Dictionary<long, ResourceDescription> entities = GetEntities();

			DMSType type = GetDMSTypeOfTopologyElement(gid);

			foreach (var property in GetAllReferenceProperties(type))
			{
				if (entities.ContainsKey(gid))
				{
					if (property == ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS ||
						property == ModelCode.CONDUCTINGEQUIPMENT_TERMINALS ||
						property == ModelCode.CONNECTIVITYNODE_TERMINALS ||
						property == ModelCode.BASEVOLTAGE_CONDUCTINGEQUIPMENTS ||
						property == ModelCode.TERMINAL_MEASUREMENTS)
					{
						elements.AddRange(entities[gid].GetProperty(property).AsReferences());
					}
					else
					{
						var elementGid = entities[gid].GetProperty(property).AsReference();
						if (elementGid != 0)
						{
							elements.Add(elementGid);
						}
					}
				}
			}
			return elements;
		}
		private List<ModelCode> GetAllReferenceProperties(DMSType type)
		{
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
		public ITopologyElement GetPopulatedElement(long gid)
		{
			ITopologyElement topologyElement = new TopologyElement(gid);
			Dictionary<long, ResourceDescription> entities = GetEntities();
			if (entities.ContainsKey(topologyElement.Id))
			{
				try
				{
					ResourceDescription rs = entities[topologyElement.Id];
					topologyElement.Mrid = rs.GetProperty(ModelCode.IDOBJ_MRID).AsString();
					topologyElement.Name = rs.GetProperty(ModelCode.IDOBJ_NAME).AsString();
					topologyElement.Description = rs.GetProperty(ModelCode.IDOBJ_DESCRIPTION).AsString();

					if (rs.ContainsProperty(ModelCode.CONDUCTINGEQUIPMENT_ISREMOTE))
					{
						topologyElement.IsRemote = rs.GetProperty(ModelCode.CONDUCTINGEQUIPMENT_ISREMOTE).AsBool();
					}
					else
					{
						topologyElement.IsRemote = false;
					}
				}
				catch (Exception)
				{
					logger.LogError($"Failed to populate element with GID {topologyElement.Id}. Could not get all properties.");
				}
			}
			else
			{
				logger.LogError($"Failed to populate element with GID {topologyElement.Id}.");
			}
			return topologyElement;
		}
		public AnalogMeasurement GetPopulatedAnalogMeasurement(long gid)
		{
			AnalogMeasurement measurement = new AnalogMeasurement();
			Dictionary<long, ResourceDescription> elements = GetEntities();
			if (elements.ContainsKey(gid))
			{
				try
				{
					ResourceDescription rs = elements[gid];
					measurement.Id = gid;
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
					logger.LogDebug($"Failed to populate analog measurement with GID: {gid}.");
				}
			}
			return measurement;
		}
		public DiscreteMeasurement GetPopulatedDiscreteMeasurement(long gid)
		{
			DiscreteMeasurement measurement = new DiscreteMeasurement();
			Dictionary<long, ResourceDescription> elements = GetEntities();
			if (elements.ContainsKey(gid))
			{
				try
				{
					ResourceDescription rs = elements[gid];
					measurement.Id = gid;
					measurement.Address = rs.GetProperty(ModelCode.MEASUREMENT_ADDRESS).AsString();
					measurement.isInput = rs.GetProperty(ModelCode.MEASUREMENT_ISINPUT).AsBool();
					measurement.CurrentOpen = rs.GetProperty(ModelCode.DISCRETE_CURRENTOPEN).AsBool();
					measurement.MaxValue = rs.GetProperty(ModelCode.DISCRETE_MAXVALUE).AsInt();
					measurement.MinValue = rs.GetProperty(ModelCode.DISCRETE_MINVALUE).AsInt();
					measurement.NormalValue = rs.GetProperty(ModelCode.DISCRETE_NORMALVALUE).AsInt();
					measurement.MeasurementType = (DiscreteMeasurementType)rs.GetProperty(ModelCode.DISCRETE_MEASUREMENTTYPE).AsEnum();
				}
				catch (Exception)
				{
					logger.LogDebug($"Failed to populate discrete measurement with GID: {gid}.");
				}
			}
			return measurement;
		}
		public float GetBaseVoltageForElement(long gid)
		{
			float voltage = 0;
			Dictionary<long, ResourceDescription> entities = GetEntities();
			if (entities.ContainsKey(gid))
			{
				ResourceDescription rs = entities[gid];
				if (rs.ContainsProperty(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE))
				{
					voltage = rs.GetProperty(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE).AsFloat();
				}
				else
				{
					logger.LogError($"Failed to get BaseVoltage. Element with GID {gid} does not have BASEVOLTAGE_NOMINALVOLTAGE property.");
				}
			}
			else
			{
				logger.LogError($"Failed to get BaseVoltage. Element with GID {gid} does not exist.");
			}
			return voltage;
		}
		public bool PrepareForTransaction(Dictionary<DeltaOpType, List<long>> delta)
		{
			logger.LogInfo("NMSManager prepare for transaction started.");
			//Dictionary<long, ResourceDescription> newElements = GetAllEements();
			bool success = true;
			//transactionModelEntities = new Dictionary<long, ResourceDescription>(newElements);
			try
			{
				//Dictionary<long, ResourceDescription> newElements = GetAllModelEntities();
				transactionModelEntities = GetAllModelEntities();
				transactionFlag = TransactionFlag.InTransaction;
				//foreach (var pair in delta)
				//{
				//	foreach (var elementGid in pair.Value)
				//	{
				//		if (pair.Key == DeltaOpType.Delete)
				//		{
				//			logger.LogDebug($"Element with GID {elementGid} is being deleted.");
				//			transactionModelEntities.Remove(elementGid);
				//		}
				//		else if (pair.Key == DeltaOpType.Insert)
				//		{
				//			List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIdsForEntityId(elementGid);
				//			ResourceDescription newEl = networkModelGDA.GetValues(elementGid, properties);
				//			if (!transactionModelEntities.TryGetValue(elementGid, out ResourceDescription element))
				//			{
				//				logger.LogDebug($"Element with GID {elementGid} is being inserted.");
				//				transactionModelEntities.Add(newEl.Id, newEl);
				//			}
				//			else
				//			{
				//				logger.LogDebug($"Element with GID {elementGid} is already inserted.");
				//			}
				//			//descented values ???
				//		}
				//		else if (pair.Key == DeltaOpType.Update)
				//		{
				//			List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIdsForEntityId(elementGid);
				//			ResourceDescription updatedEl = networkModelGDA.GetValues(elementGid, properties);
				//			if (transactionModelEntities.TryGetValue(elementGid, out ResourceDescription element))
				//			{
				//				logger.LogDebug($"Element with GID {elementGid} is being updated.");
				//				element = updatedEl;
				//			}
				//			else
				//			{
				//				logger.LogDebug($"Element with GID {elementGid} does not exist.");
				//			}
				//		}
				//	}
				//	transactionFlag = TransactionFlag.InTransaction;
				//}
			}
			catch (Exception ex)
			{
				logger.LogInfo($"NMSManager failed to prepare for transaction. Exception message: {ex.Message}");
				success = false;
				transactionFlag = TransactionFlag.NoTransaction;
			}
			logger.LogInfo("NMSManager is prepared for transaction.");
			return success;
		}
		public void CommitTransaction()
		{
			modelEntities = new Dictionary<long, ResourceDescription>(transactionModelEntities);
			transactionFlag = TransactionFlag.NoTransaction;
			logger.LogDebug("NMSManager commited transaction successfully.");
		}
		public void RollbackTransaction()
		{
			transactionModelEntities = null;
			transactionFlag = TransactionFlag.NoTransaction;
			logger.LogDebug("NMSManager rolled back transaction.");

		}
		public DMSType GetDMSTypeOfTopologyElement(long gid) => ModelCodeHelper.GetTypeFromModelCode(modelResourcesDesc.GetModelCodeFromId(gid));
		public string GetDMSTypeOfTopologyElementString(long gid)
		{
			logger.LogDebug($"Getting element DMStype for GID {gid}.");
			try
			{
				return GetDMSTypeOfTopologyElement(gid).ToString();
			}
			catch (Exception)
			{
				return "FIELD";
			}
		}
	}
}
