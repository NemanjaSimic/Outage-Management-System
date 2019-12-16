using NetworkModelServiceFunctions;
using Outage.Common;
using Outage.Common.GDA;
using System.Collections.Generic;

namespace NetworkModelServiceFunctions
{
	public class GDAModelHelper
	{

		private readonly ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
		private readonly NetworkModelGDA networkModelGDA = new NetworkModelGDA();
		Dictionary<long, ResourceDescription> modelEntities;

		public List<long> GetAllEnergySousces()
		{
			return GetAllGids(ModelCode.ENERGYSOURCE);
		}

		public Dictionary<ModelCode, List<long>> GetAllModelEntities()
		{
			List<ModelCode> concreteClasses = modelResourcesDesc.NonAbstractClassIds;
			Dictionary<ModelCode, List<long>> modelEntities = new Dictionary<ModelCode, List<long>>();

			foreach (var concreteClass in concreteClasses)
			{
				modelEntities.Add(concreteClass, GetAllGids(concreteClass));
			}

			return modelEntities;
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

		public Dictionary<long, ResourceDescription> RetrieveAllElements()
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
			return modelEntities;
		}

		public List<long> GetAllReferencedElements(long gid)
		{
			List<long> elements = new List<long>();
			//Association association = new Association();
			DMSType type = ModelCodeHelper.GetTypeFromModelCode(modelResourcesDesc.GetModelCodeFromId(gid));

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

			foreach (var property in propertyIds)
			{
				//association.PropertyId = property;
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
				//elements.AddRange(networkModelGDA.GetRelatedValues(gid, new List<ModelCode>() { ModelCode.IDOBJ_GID }, association));
			}

			return elements;
		}



		public List<ModelCode> GetReferenceProperties(long gid)
		{
			DMSType type = ModelCodeHelper.GetTypeFromModelCode(modelResourcesDesc.GetModelCodeFromId(gid));

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
	}
}
