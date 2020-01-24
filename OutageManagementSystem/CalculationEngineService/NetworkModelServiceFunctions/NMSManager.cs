using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;

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
			List<ModelCode> concreteClasses = modelResourcesDesc.NonAbstractClassIds;
			modelEntities = new Dictionary<long, ResourceDescription>();
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
		public List<long> GetAllReferencedElements(long gid)
		{
			logger.LogDebug($"Getting all referenced elements for GID {gid} in transaction.");
			List<long> elements = new List<long>();
			Dictionary<long, ResourceDescription> entities = GetEntities();

			DMSType type = ModelCodeHelper.GetTypeFromModelCode(modelResourcesDesc.GetModelCodeFromId(gid));

			foreach (var property in GetAllReferenceProperties(type))
			{
				if (entities.ContainsKey(gid))
				{
					if (property == ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS || property == ModelCode.CONDUCTINGEQUIPMENT_TERMINALS || property == ModelCode.CONNECTIVITYNODE_TERMINALS)
					{
						elements.AddRange(entities[gid].GetProperty(property).AsReferences());
					}
					else
					{
						elements.Add(entities[gid].GetProperty(property).AsReference());
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
					break;
				case DMSType.CONNECTIVITYNODE:
					propertyIds.Add(ModelCode.CONNECTIVITYNODE_TERMINALS);
					break;
				case DMSType.POWERTRANSFORMER:
					propertyIds.Add(ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS);
					break;
				case DMSType.ENERGYSOURCE:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					break;
				case DMSType.ENERGYCONSUMER:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					break;
				case DMSType.TRANSFORMERWINDING:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					propertyIds.Add(ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER);
					break;
				case DMSType.FUSE:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					break;
				case DMSType.DISCONNECTOR:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					break;
				case DMSType.BREAKER:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					break;
				case DMSType.LOADBREAKSWITCH:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					break;
				case DMSType.ACLINESEGMENT:
					propertyIds.Add(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS);
					break;
				case DMSType.ANALOG:
					propertyIds.Add(ModelCode.MEASUREMENT_TERMINAL);
					break;
				case DMSType.DISCRETE:
					propertyIds.Add(ModelCode.MEASUREMENT_TERMINAL);
					break;
				default:
					break;
			}

			return propertyIds;
		}

		public void PopulateElement(ref ITopologyElement element)
		{
			Dictionary<long, ResourceDescription> entities = GetEntities();
			if (entities.ContainsKey(element.Id))
			{
				try
				{
					ResourceDescription rs = entities[element.Id];
					element.Mrid = rs.GetProperty(ModelCode.IDOBJ_MRID).AsString();
					element.Name = rs.GetProperty(ModelCode.IDOBJ_NAME).AsString();
					element.Description = rs.GetProperty(ModelCode.IDOBJ_DESCRIPTION).AsString();
					if (rs.ContainsProperty(ModelCode.CONDUCTINGEQUIPMENT_ISREMOTE))
					{
						element.IsRemote = rs.GetProperty(ModelCode.CONDUCTINGEQUIPMENT_ISREMOTE).AsBool();
					}
					else
					{
						element.IsRemote = false;
					}
				}
				catch (Exception)
				{
					logger.LogError($"Failed to populate element with GID {element.Id}. Could not get all properties.");
				}
			}
			else
			{
				logger.LogError($"Failed to populate element with GID {element.Id}.");
			}
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
		public bool PrepareForTransaction(Dictionary<DeltaOpType, List<long>> delta)
		{
			logger.LogInfo("NMSManager prepare for transaction started.");

			bool success = true;
			transactionModelEntities = new Dictionary<long, ResourceDescription>(modelEntities);
			try
			{
				foreach (var pair in delta)
				{
					foreach (var elementGid in pair.Value)
					{
						if (pair.Key == DeltaOpType.Delete)
						{
							logger.LogDebug($"Element with GID {elementGid} is being deleted.");
							transactionModelEntities.Remove(elementGid);
						}
						else if (pair.Key == DeltaOpType.Insert)
						{
							List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIdsForEntityId(elementGid);
							ResourceDescription newEl = networkModelGDA.GetValues(elementGid, properties);
							if (!transactionModelEntities.TryGetValue(elementGid, out ResourceDescription element))
							{
								logger.LogDebug($"Element with GID {elementGid} is being inserted.");
								transactionModelEntities.Add(newEl.Id, newEl);
							}
							else
							{
								logger.LogDebug($"Element with GID {elementGid} is already inserted.");
							}
							//descented values ???
						}
						else if (pair.Key == DeltaOpType.Update)
						{
							List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIdsForEntityId(elementGid);
							ResourceDescription updatedEl = networkModelGDA.GetValues(elementGid, properties);
							if (transactionModelEntities.TryGetValue(elementGid, out ResourceDescription element))
							{
								logger.LogDebug($"Element with GID {elementGid} is being updated.");
								element = updatedEl;
							}
							else
							{
								logger.LogDebug($"Element with GID {elementGid} does not exist.");
							}
						}
					}
					transactionFlag = TransactionFlag.InTransaction;
				}
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
	}
}
