using CECommon;
using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using Logger = Outage.Common.LoggerWrapper;

namespace NetworkModelServiceFunctions
{
	public class NMSManager
	{
        #region Fields
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
		}
		public List<long> GetAllEnergySources(TransactionFlag flag)
		{
			List<long> gids = new List<long>();
			Dictionary<long, ResourceDescription> entities;
			if (flag == TransactionFlag.InTransaction)
			{
				Logger.Instance.LogDebug("Getting all energy sources in transaction.");
				entities = transactionModelEntities;
			}
			else
			{
				Logger.Instance.LogDebug("Getting all energy sources in no transaction.");
				entities = modelEntities;
			}

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
		public List<long> GetAllReferencedElements(long gid, TransactionFlag flag)
		{
			List<long> elements = new List<long>();
			Dictionary<long, ResourceDescription> entities;
			if (flag == TransactionFlag.InTransaction)
			{
				Logger.Instance.LogDebug($"Getting all referenced elements for GID {gid} in transaction.");
				entities = transactionModelEntities;
			}
			else
			{
				Logger.Instance.LogDebug($"Getting all referenced elements for GID {gid} in no transaction.");
				entities = modelEntities;
			}
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
				default:
					break;
			}

			return propertyIds;
		}
		public bool PrepareForTransaction(Dictionary<DeltaOpType, List<long>> delta)
		{
			Logger.Instance.LogInfo("NMSManager prepare for transaction started.");

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
							Logger.Instance.LogDebug($"Element with GID {elementGid} is being deleted.");
							transactionModelEntities.Remove(elementGid);
						}
						else if (pair.Key == DeltaOpType.Insert)
						{
							List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIdsForEntityId(elementGid);
							ResourceDescription newEl = networkModelGDA.GetValues(elementGid, properties);
							if (!transactionModelEntities.TryGetValue(elementGid, out ResourceDescription element))
							{
								Logger.Instance.LogDebug($"Element with GID {elementGid} is being inserted.");
								transactionModelEntities.Add(newEl.Id, newEl);
							}
							else
							{
								Logger.Instance.LogDebug($"Element with GID {elementGid} is already inserted.");
							}
							//descented values ???
						}
						else if (pair.Key == DeltaOpType.Update)
						{
							List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIdsForEntityId(elementGid);
							ResourceDescription updatedEl = networkModelGDA.GetValues(elementGid, properties);
							if (transactionModelEntities.TryGetValue(elementGid, out ResourceDescription element))
							{
								Logger.Instance.LogDebug($"Element with GID {elementGid} is being updated.");
								element = updatedEl;
							}
							else
							{
								Logger.Instance.LogDebug($"Element with GID {elementGid} does not exist.");
							}
						}
					}

				}
			}
			catch (Exception ex)
			{
				Logger.Instance.LogInfo($"NMSManager failed to prepare for transaction. Exception message: {ex.Message}");
				success = false;
			}
			Logger.Instance.LogInfo("NMSManager is prepared for transaction.");
			return success;
		}
		public void CommitTransaction()
		{
			modelEntities = new Dictionary<long, ResourceDescription>(transactionModelEntities);
		}
		public void RollbackTransaction()
		{
			transactionModelEntities = null;
		}
	}
}
