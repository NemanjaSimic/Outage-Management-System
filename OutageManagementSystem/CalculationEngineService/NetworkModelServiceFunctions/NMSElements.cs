using Outage.Common;
using Outage.Common.GDA;
using System.Collections.Generic;
using Logger = Outage.Common.LoggerWrapper;

namespace NetworkModelServiceFunctions
{
	public class NMSElements
	{
        #region Fields
        private readonly ModelResourcesDesc modelResourcesDesc;
		private readonly NetworkModelGDA networkModelGDA;
		private Dictionary<long, ResourceDescription> modelEntities;
		private Dictionary<long, ResourceDescription> transactionModelEntities;

		#endregion

		#region Singleton
		private static object syncObj = new object();
		private static NMSElements instance;

        public static NMSElements Instance
		{
			get 
			{
				lock (syncObj)
				{
					if (instance == null)
					{
						instance = new NMSElements();
					}
				}
				return instance;
			}
		}
        #endregion
        private NMSElements()
		{
			modelResourcesDesc = new ModelResourcesDesc();
			networkModelGDA = new NetworkModelGDA();
			Logger.Instance.LogDebug("Retrieve all elements started.");
			RetrieveAllElements();
			Logger.Instance.LogDebug("Retrieve all elements finished.");
		}
		public List<long> GetAllEnergySources()
		{
			return GetAllGids(ModelCode.ENERGYSOURCE);
		}
		private List<long> GetAllGids(ModelCode concreteClass)
		{
			List<long> gids = new List<long>();

			List<ResourceDescription> resourceDescriptions = networkModelGDA.GetExtentValues(concreteClass, new List<ModelCode>() { ModelCode.IDOBJ_GID });
			foreach (var resourceDescription in resourceDescriptions)
			{
				gids.Add(resourceDescription.GetProperty(ModelCode.IDOBJ_GID).AsLong());
			}
			return gids;
		}
		private void RetrieveAllElements()
		{
			List<ModelCode> concreteClasses = modelResourcesDesc.NonAbstractClassIds;
			modelEntities = new Dictionary<long, ResourceDescription>();
			foreach (var item in concreteClasses)
			{
				List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCodeHelper.GetTypeFromModelCode(item));
				var elements = networkModelGDA.GetExtentValues(item, properties);
				foreach (var element in elements)
				{
					modelEntities.Add(element.Id, element);
				}
			}
		}
		public List<long> GetAllReferencedElements(long gid)
		{
			List<long> elements = new List<long>();
			DMSType type = ModelCodeHelper.GetTypeFromModelCode(modelResourcesDesc.GetModelCodeFromId(gid));

			foreach (var property in GetAllReferenceProperties(type))
			{
				if (modelEntities.ContainsKey(gid))
				{
					if (property == ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS || property == ModelCode.CONDUCTINGEQUIPMENT_TERMINALS || property == ModelCode.CONNECTIVITYNODE_TERMINALS)
					{
						elements.AddRange(modelEntities[gid].GetProperty(property).AsReferences());

					}
					else
					{
						elements.Add(modelEntities[gid].GetProperty(property).AsReference());
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

		public void PrepareTransaction(Dictionary<DeltaOpType, List<long>> delta)
		{
			transactionModelEntities = new Dictionary<long, ResourceDescription>(modelEntities);
			foreach (var pair in delta)
			{
				if (pair.Key == DeltaOpType.Delete)
				{
					foreach (var gid in pair.Value)
					{
						transactionModelEntities.Remove(gid);
					}
				}
				else if (pair.Key == DeltaOpType.Insert)
				{
					foreach (var gid in pair.Value)
					{
						transactionModelEntities.Add(gid);
					}
				}
			}
		}
	}
}
