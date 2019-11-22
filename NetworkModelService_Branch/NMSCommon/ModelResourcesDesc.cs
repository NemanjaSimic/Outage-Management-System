using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTN.Common;
using System.Xml;
using NMSCommon;

namespace NMSCommon
{	
	public class ResourcePropertiesDesc
	{
		/// <summary>
		/// Code of the class type.
		/// </summary>
		private ModelCode resourceId;

		/// <summary>
		/// Name of the class type.
		/// </summary>
		private string resourceName;

		/// <summary>
		/// Collection of the property codes for class type.
		/// </summary>
		private Dictionary<ModelCode, string> propertyIds = new Dictionary<ModelCode, string>(new ModelCodeComparer());

		/// <summary>
		/// Initializes a new instance of the ResourcePropertiesDesc class.
		/// </summary>
		/// <param name="resourceId">Model type code</param>
		public ResourcePropertiesDesc(ModelCode resourceId)
		{
			this.resourceId = resourceId;
			this.resourceName = resourceId.ToString();			
		}

		public ResourcePropertiesDesc(ModelCode resourceId, string resourceName)
		{
			this.resourceId = resourceId;
			this.resourceName = resourceName;
		}
		
		/// <summary>
		/// Gets code of the class type.
		/// </summary>
		public ModelCode ResourceId
		{
			get
			{
				return resourceId;
			}
		}

		/// <summary>
		/// Gets name of the class type.
		/// </summary>
		public string ResourceName
		{
			get
			{
				return resourceName;
			}
		}

		/// <summary>
		/// Gets read-only collection of property codes for class type.
		/// </summary>
		public IEnumerable<ModelCode> PropertyIds
		{
			get
			{
				return propertyIds.Keys.AsEnumerable();
			}
		}
	
		/// <summary>
		/// Adds new property code to class type.
		/// </summary>
		/// <param name="propertyId">Property code</param>	
		public void AddPropertyId(ModelCode propertyId)
		{
			propertyIds[propertyId] = null;
		}

		public void AddPropertyId(ModelCode propertyId, string propertyName)
		{
			propertyIds[propertyId] = propertyName;
		}

		/// <summary>
		/// Gets property name for the specified property code.
		/// </summary>
		/// <param name="propertyId">Property code</param>
		/// <returns>Name of the property</returns>
		public string GetPropertyName(ModelCode propertyId)
		{
			if (propertyIds.ContainsKey(propertyId))
			{
				// if this is true, the property ID is from core
				if (propertyIds[propertyId] == null)
				{
					return ((ModelCode)propertyId).ToString();
				}
				else // if this is true, the property ID is from extensibility
				{
					return propertyIds[propertyId];
				}
			}

			string message = String.Format("Specified property ( ID = {0} ) does not exists for {1} resource.", (ModelCode)propertyId, resourceName);
			CommonTrace.WriteTrace(CommonTrace.TraceError, message);
			throw new Exception(message);
		}

		/// <summary>
		/// Checks if property with specified code exists in class type.
		/// </summary>
		/// <param name="propertyId">Code of the property</param>
		/// <returns>TRUE if property exists in class type, FALSE otherwise</returns>		
		public bool PropertyExists(ModelCode propertyId)
		{
			return propertyIds.ContainsKey(propertyId);
		}
	}

	/// <summary>
	/// A class that represents collection of class type descriptions for used model.
	/// Contains descriptions for model classes, mappings from type to model code 
	/// and sorted list of type identificators in order that should be followed for insert operations
	/// </summary>
	public class ModelResourcesDesc
	{
		/// <summary>
		/// Dictionary of model type code to model type description mappings.
		/// </summary>
		private Dictionary<long, ResourcePropertiesDesc> resourceDescs;

		/// <summary>
		/// Dictionary of DMS type to class type code mappings.
		/// </summary>
		private Dictionary<DMSType, ModelCode> type2modelCode = new Dictionary<DMSType, ModelCode>(new DMSTypeComparer());

		/// <summary>
		/// Insert order for Data model.
		/// </summary>
		private List<ModelCode> typeIdsInInsertOrder = new List<ModelCode>();

		/// <summary>
		/// List of non abstract class Ids.
		/// </summary>
		private List<ModelCode> nonAbstractClassIds = new List<ModelCode>();

		/// <summary>
		/// HashSet of not settable property Ids.
		/// </summary>
		private HashSet<ModelCode> notSettablePropertyIds = new HashSet<ModelCode>(new ModelCodeComparer());

		/// <summary>
		/// Dictionary of not accessible property Ids.
		/// </summary>
		private Dictionary<ModelCode, bool> notAccessiblePropertyIds = new Dictionary<ModelCode, bool>(new ModelCodeComparer());

		/// <summary>
		/// All ModelCodes, codes and from ModelCode enum end from extensibility
		/// </summary>
		private HashSet<ModelCode> allModelCodes = new HashSet<ModelCode>(new ModelCodeComparer());

		/// <summary>
		/// All types, codes and from DMSType enum end from extensibility
		/// </summary>
		private HashSet<DMSType> allDMSTypes = new HashSet<DMSType>(new DMSTypeComparer());

		public HashSet<ModelCode> AllModelCodes
		{
			get { return allModelCodes; }
		}

		public HashSet<DMSType> AllDMSTypes
		{
			get { return allDMSTypes; }
		}

		/// <summary>
		/// Initializes a new instance of the ModelResourcesDesc class.
		/// </summary>
		public ModelResourcesDesc()
		{
			Initialize();
		}

		private void Initialize()
		{
			// populating model type and property descriptions
			this.resourceDescs = new Dictionary<long, ResourcePropertiesDesc>();

			ResourcePropertiesDesc desc = null;

			InitializeTypeIdsInInsertOrder();

			//// Initialize metadata for core entities			
			InitializeNotSettablePropertyIds();			

			foreach (ModelCode code in Enum.GetValues(typeof(ModelCode)))
			{
				allModelCodes.Add(code);
			}

			foreach (DMSType type in Enum.GetValues(typeof(DMSType)))
			{
				allDMSTypes.Add(type);
			}


			# region get all class model codes from core

			//// Get all class model codes for all services and for all classes, abstract or not. Extensibility included
			List<ModelCode> classIds = new List<ModelCode>();
			foreach (ModelCode code in allModelCodes)
			{
				// don't insert attribute codes
				if (((long)code & (long)ModelCodeMask.MASK_ATTRIBUTE_TYPE) == 0)
				{
					classIds.Add(code);
				}
			}

			# endregion get all class model codes from core

			# region Initialize non abstract class and resourceDescription map from core

			foreach (ModelCode classId in classIds)
			{
				//// Initialize leafs
				if (((long)classId & (long)ModelCodeMask.MASK_TYPE) != 0)
				{
					nonAbstractClassIds.Add(classId);
				}

				//// Initialize resourceDescription map
				desc = this.AddResourceDesc(classId);
				long classIdMask = unchecked((long)0xffffffff00000000);

				foreach (ModelCode propertyId in allModelCodes)
				{
					PropertyType propertyType = Property.GetPropertyType(propertyId);

					if (propertyType == 0)
					{
						continue;
					}

					if (((long)propertyId & classIdMask) == ((long)classId & classIdMask))
					{
						if (!desc.PropertyExists(propertyId))
						{
							desc.AddPropertyId(propertyId);
						}
					}
				}
			}

			# endregion Initialize non abstract class and resourceDescription map  from core

			# region Initialize type 2 model code map

			//// Model codes names from enum, these are only names for core model codes  
			string[] modelCodeNamesFromCore = Enum.GetNames(typeof(ModelCode));

			classIds = new List<ModelCode>();
			foreach (ModelCode code in allModelCodes)
			{
				// don't insert attribute codes
				if (((long)code & (long)ModelCodeMask.MASK_ATTRIBUTE_TYPE) == 0)
				{
					classIds.Add(code);
				}
			}

			//// Initialize DMSType 2 ModelCode map
			foreach (DMSType t in allDMSTypes)
			{

				DMSType type = t & DMSType.MASK_TYPE;

				////  if DMSType is not addad and it is not mask
				if (!this.type2modelCode.ContainsKey(type) && type != DMSType.MASK_TYPE)
				{
					//// DMSTypes that have same name as appropriete model code
					if (modelCodeNamesFromCore.Contains(type.ToString()))
					{
						this.type2modelCode[type] = GetModelCodeFromTypeForCore(type);
					}
					else // for DMSTypes which don't have it's ModelCode with same name or are not in core
					{
						try // for DMSTypes which have it's ModelCode but with different name
						{
							foreach (ModelCode classId in classIds)
							{
								if (GetTypeFromModelCode(classId) == type)
								{
									this.type2modelCode[type] = classId;
									break;
								}
							}
						}
						catch { }
					}
				}
			}

			# endregion Initialize type 2 model code map
		}

		/// <summary>
		/// Gets insert order for Data model.
		/// </summary>		
		public List<ModelCode> TypeIdsInInsertOrder
		{
			get
			{
				return typeIdsInInsertOrder;
			}
		}

		/// <summary>
		/// Gets list of non abstract class Ids.
		/// </summary>
		public List<ModelCode> NonAbstractClassIds
		{
			get { return nonAbstractClassIds; }
			set { nonAbstractClassIds = value; }
		}

		/// <summary>
		/// Gets dictionary of not settable property Ids.
		/// </summary>
		public HashSet<ModelCode> NotSettablePropertyIds
		{
			get { return notSettablePropertyIds; }
			set { notSettablePropertyIds = value; }
		}

		/// <summary>
		/// Gets dictionary of not accessible property Ids.
		/// </summary>
		public Dictionary<ModelCode, bool> NotAccessiblePropertyIds
		{
			get { return notAccessiblePropertyIds; }
			set { notAccessiblePropertyIds = value; }
		}

		public static DMSType GetTypeFromModelCode(ModelCode code)
		{
			return (DMSType)((long)((long)code & (long)ModelCodeMask.MASK_TYPE) >> 16);
		}

		public static ModelCode GetPropertyOwnerFromProperty(ModelCode propertyCode)
		{
			return (ModelCode)GetPropertyOwnerFromProperty((long)propertyCode);
		}

		public static long GetPropertyOwnerFromProperty(long propertyCode)
		{
			ulong propertyOwnerMask = 0xffffffffffff0000;
			long propertyOwner = (long)(propertyOwnerMask & (ulong)propertyCode);

			return propertyOwner;
		}

		/// <summary>
		/// Finds model code of the parent class, according to the given ModelCode of the class
		/// </summary>
		/// <param name="typeId">entity type identifier</param>
		/// <returns>identifier of the parent class</returns>
		public static ModelCode FindFirstParent(ModelCode typeId)
		{
			return (ModelCode)FindFirstParent((long)typeId);
		}

		public static long FindFirstParent(long typeId)
		{
			ulong firstNullMask = (ulong)0xf000000000000000;
			ulong parentMask = 0;
			while (((firstNullMask / 16) & (ulong)typeId) != 0)
			{
				parentMask += firstNullMask;
				firstNullMask = firstNullMask / 16;
			}

			ulong parentModelCode = (parentMask & (ulong)typeId);

			firstNullMask *= 16;

			while (parentModelCode != 0 && (firstNullMask & parentModelCode) == firstNullMask)
			{
				if ((firstNullMask & parentModelCode) == firstNullMask)
				{
					parentModelCode &= ~firstNullMask;
				}

				firstNullMask *= 16;
			}

			return (long)parentModelCode;
		}

		/// <summary>
		/// Gets all leaves which inherit specified entity type  
		/// </summary>
		/// <param name="entityType">ModelCode that represents entity type</param>
		/// <returns>List of leaves (DMSType)</returns>
		public static List<DMSType> GetLeavesForCoreEntities(ModelCode entityType)
		{
			List<DMSType> children = new List<DMSType>();

			foreach (ModelCode leafCM in Enum.GetValues(typeof(ModelCode)))
			{
				//// if it is not property code and it is leaf code and it is inhereted from submited type
				if (((long)leafCM & (long)ModelCodeMask.MASK_ATTRIBUTE_TYPE) == 0 && ((long)leafCM & (long)ModelCodeMask.MASK_TYPE) != 0 && InheritsFrom(entityType, leafCM))
				{
					children.Add(GetTypeFromModelCode(leafCM));
				}
			}

			return children;
		}

		public List<DMSType> GetLeaves(ModelCode entityType)
		{
			List<DMSType> children = new List<DMSType>();

			foreach (ModelCode leafCM in allModelCodes)
			{
				//// if it is not property code and it is leaf code and it is inhereted from submited type
				if (((long)leafCM & (long)ModelCodeMask.MASK_ATTRIBUTE_TYPE) == 0 && ((long)leafCM & (long)ModelCodeMask.MASK_TYPE) != 0 && InheritsFrom(entityType, leafCM))
				{
					children.Add(GetTypeFromModelCode(leafCM));
				}
			}

			return children;
		}

		public static bool InheritsFrom(ModelCode parentModelCode, ModelCode childModelCode)
		{
			ulong nibbleMask = (ulong)0xf000000000000000;
			bool zeroAtStart = ((ulong)parentModelCode & nibbleMask) == 0;

			while (nibbleMask > (ulong)0x00000000f0000000)
			{
				ulong childNibbleExt = ((ulong)childModelCode) & nibbleMask;
				ulong parentNibbleExt = ((ulong)parentModelCode) & nibbleMask;

				if (parentNibbleExt == 0 && !zeroAtStart)
				{
					return true;
				}
				else if (parentNibbleExt != childNibbleExt)
				{
					return false;
				}
				else if (parentNibbleExt != 0)
				{
					zeroAtStart = false;
				}

				nibbleMask /= 16;
			}

			return true;
		}

		/// <summary>
		/// Adds new model type description to collection.
		/// </summary>
		/// <param name="resourceId">Code of the model type</param>
		/// <returns>Just created model type description</returns>
		public ResourcePropertiesDesc AddResourceDesc(ModelCode resourceId)
		{
			ResourcePropertiesDesc desc = new ResourcePropertiesDesc(resourceId);
			this.resourceDescs[((long)resourceId & (long)ModelCodeMask.MASK_INHERITANCE_ONLY)] = desc;
			return desc;
		}

		public ResourcePropertiesDesc AddResourceDesc(ModelCode resourceId, string resourceName)
		{
			ResourcePropertiesDesc desc = new ResourcePropertiesDesc(resourceId, resourceName);
			this.resourceDescs[((long)resourceId & (long)ModelCodeMask.MASK_INHERITANCE_ONLY)] = desc;
			return desc;
		}

		/// <summary>
		/// Adds new model type description to collection.
		/// </summary>
		/// <param name="resourceId">Code of the model type</param>
		/// <param name="desc">Model type description</param>
		public void AddResourceDesc(ModelCode resourceId, ResourcePropertiesDesc desc)
		{
			this.resourceDescs[((long)resourceId & (long)ModelCodeMask.MASK_INHERITANCE_ONLY)] = desc;
		}

		/// <summary>
		/// Checks if the model type description exists in collection for specified model type code.
		/// </summary>
		/// <param name="resourceId">Code of the model type</param>
		/// <returns>TRUE if model type description exists in collection, FALSE otherwise</returns>
		public bool ResourceExists(ModelCode resourceId)
		{
			return ResourceExistsForType(((long)resourceId & (long)ModelCodeMask.MASK_INHERITANCE_ONLY));
		}

		/// <summary>
		/// Gets model type description for specified model type code.
		/// </summary>
		/// <param name="resourceId">Code of the model type</param>
		/// <returns>Model type description for the specified model code</returns>
		public ResourcePropertiesDesc GetResourcePropertiesDesc(ModelCode resourceId)
		{
			long temp = ((long)resourceId & (long)ModelCodeMask.MASK_INHERITANCE_ONLY);

            if (!resourceDescs.ContainsKey(temp))
            {
                throw new Exception(string.Format("Invalid Model Code: {0}", resourceId));
            }

			return resourceDescs[temp];
		}

		/// <summary>
		/// Finds list of all ancestors of specified model type.
		/// </summary>
		/// <param name="typeId">Code of the model type</param>
		/// <param name="ancestors">List of model type codes of ancestors of the specified model type</param>		
		public void GetResourceAncestors(ModelCode typeId, ref List<ModelCode> ancestors)
		{
			if (ancestors == null)
			{
				ancestors = new List<ModelCode>();
			}

			long typeIdLong = (long)typeId;

			//// model code with inheratence only
			typeIdLong &= (long)ModelCodeMask.MASK_INHERITANCE_ONLY;
			if (typeIdLong == (long)ModelCode.IDOBJ)
			{
				return;
			}

			long oldestAncestorId = typeIdLong & (long)ModelCodeMask.MASK_FIRSTNBL;
			long maskDelNbls = (long)ModelCodeMask.MASK_DELFROMNBL8;
			long maskfNbl = 0x0000000f00000000;
			long ancestorId = (long)typeIdLong & maskDelNbls;
			long ancestorIdOld = 0;
			while (ancestorId != oldestAncestorId)
			{
				long ancestorIdF = ancestorId & maskfNbl;
				if (ancestorId != (long)typeIdLong && (ancestorId & maskfNbl) != maskfNbl && ResourceExistsForType(ancestorId))
				{
					ancestors.Insert(0, resourceDescs[ancestorId].ResourceId);
				}

				maskDelNbls <<= 4;
				maskfNbl <<= 4;
				ancestorIdOld = ancestorId;
				ancestorId &= maskDelNbls;
			}

			if (oldestAncestorId != (long)typeIdLong && ResourceExistsForType(oldestAncestorId))
			{
				ancestors.Insert(0, resourceDescs[oldestAncestorId].ResourceId);
			}
		}

		/// <summary>
		/// Gets property ids that corresponds to attributes of the class. Inherited properties are not included.
		/// </summary>
		/// <param name="code">ModelCode representing class.</param>
		/// <returns>List of property ids that corresponds to class attributes.</returns>		
		public List<ModelCode> GetClassPropertyIds(ModelCode classId)
		{
			List<ModelCode> properties = new List<ModelCode>();

			properties.AddRange(this.GetResourcePropertiesDesc(classId).PropertyIds);

			int i = 0;
			while (i < properties.Count)
			{
				ModelCode propertyId = properties[i];
				if (notAccessiblePropertyIds.ContainsKey(propertyId))
				{
					properties.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}

			return properties;
		}

		/// <summary>
		/// Gets property ids that corresponds to all attributes of the entity.
		/// </summary>
		/// <param name="code">ModelCode representing entity type.</param>
		/// <returns>List of property ids that corresponds to entity attributes.</returns>
		public List<ModelCode> GetAllPropertyIds(ModelCode code)
		{
			List<ModelCode> properties = new List<ModelCode>();
			List<ModelCode> ancestorIds = new List<ModelCode>();

			this.GetResourceAncestors(code, ref ancestorIds);

			properties.AddRange(this.GetResourcePropertiesDesc(code).PropertyIds);
			foreach (ModelCode ancestorId in ancestorIds)
			{
				properties.AddRange(this.GetResourcePropertiesDesc(ancestorId).PropertyIds);
			}

			int i = 0;
			while (i < properties.Count)
			{
				ModelCode propertyId = properties[i];
				if (notAccessiblePropertyIds.ContainsKey(propertyId))
				{
					properties.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}

			return properties;
		}

		public List<ModelCode> GetAllPropertyIdsWithNotAccessibleProperties(ModelCode code)
		{
			List<ModelCode> properties = new List<ModelCode>();
			List<ModelCode> ancestorIds = new List<ModelCode>();

			this.GetResourceAncestors(code, ref ancestorIds);

			properties.AddRange(this.GetResourcePropertiesDesc(code).PropertyIds);
			foreach (ModelCode ancestorId in ancestorIds)
			{
				properties.AddRange(this.GetResourcePropertiesDesc(ancestorId).PropertyIds);
			}

			return properties;
		}

		/// <summary>
		/// Gets property ids that corresponds to all attributes of the entity of specified type.
		/// </summary>
		/// <param name="typeId">DMS type code representing entity type.</param>
		/// <returns>List of property ids that corresponds to entity attributes.</returns>
		public List<ModelCode> GetAllPropertyIds(DMSType typeId)
		{
			return GetAllPropertyIds(GetModelCodeFromType(typeId));
		}

		public List<ModelCode> GetAllPropertyIds(short type)
		{
			return GetAllPropertyIds((DMSType)type);
		}

		/// <summary>
		/// Gets property ids defined for entity type and that satisfy specified property type
		/// </summary>
		/// <param name="typeId">DMS type code representing entity type.</param>
		/// <param name="propertyType">Type of properties that should be returned.</param>
		/// <returns>List of property ids that corresponds to entity attributes that satisfy entity type.</returns>		
		public List<ModelCode> GetPropertyIds(DMSType typeId, PropertyType propertyType)
		{
			List<ModelCode> propertyIds = GetAllPropertyIds(GetModelCodeFromType(typeId));
			List<ModelCode> propertyIdsFiltered = new List<ModelCode>();

			int i = 0;
			while (i < propertyIds.Count)
			{
				//if (ModelCodeHelper.ExtractPropertyTypeFromModelCode(propertyIds[i]) != propertyType)
				if (Property.GetPropertyType(propertyIds[i]) != propertyType)
				{
					propertyIds.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}

			return propertyIds;
		}

		/// <summary>
		/// Gets property ids that corresponds to all attributes of the entity.
		/// </summary>
		/// <param name="globalId">GlobalId</param>
		/// <returns>List of property ids that corresponds to entity attributes.</returns>
		public List<ModelCode> GetAllPropertyIdsForEntityId(long globalId)
		{
			DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
			return GetAllPropertyIds(type);
		}

		/// <summary>
		/// Gets property ids that corresponds to all settable attributes of the entity.
		/// </summary>
		/// <param name="typeId">ModelCode representing entity type.</param>
		/// <returns>List of property ids that corresponds to settable entity attributes.</returns>
		public List<ModelCode> GetAllSettablePropertyIds(ModelCode typeId, bool includeInherited)
		{
			List<ModelCode> properties = (includeInherited) ? GetAllPropertyIds(typeId) : GetClassPropertyIds(typeId);

			int i = 0;
			while (i < properties.Count)
			{
				ModelCode propertyId = properties[i];
				if (notSettablePropertyIds.Contains(propertyId) || Property.GetPropertyType(propertyId) == PropertyType.ReferenceVector)
				{
					properties.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}

			return properties;
		}

		/// <summary>
		/// Gets property ids that corresponds to all settable attributes of the entity.
		/// </summary>
		/// <param name="type">DMSType representing entity type.</param>
		/// <returns>List of property ids that corresponds to settable entity attributes.</returns>
		public List<ModelCode> GetAllSettablePropertyIds(DMSType type)
		{
			ModelCode code = GetModelCodeFromType(type);
			return GetAllSettablePropertyIds(code, true);
		}

		/// <summary>
		/// Gets property ids that corresponds to all settable attributes of the entity.
		/// </summary>
		/// <param name="type">DMSType representing entity type.</param>
		/// <returns>List of property ids that corresponds to settable entity attributes.</returns>
		public List<ModelCode> GetAllSettablePropertyIds(short type)
		{
			ModelCode code = GetModelCodeFromType((DMSType)type);
			return GetAllSettablePropertyIds(code, true);
		}

		/// <summary>
		/// Gets property ids that corresponds to all settable attributes of the entity.
		/// </summary>
		/// <param name="globalId">GlobalId</param>
		/// <returns>List of property ids that corresponds to settable entity attributes.</returns>
		public List<ModelCode> GetAllSettablePropertyIdsForEntityId(long globalId)
		{
			DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
			return GetAllSettablePropertyIds(type);
		}

		/// <summary>
		/// Clears all resources from collection.
		/// </summary>
		public void Clear()
		{
			resourceDescs.Clear();
		}

		/// <summary>
		/// Gets model type code for specified DMS type.
		/// </summary>
		/// <param name="type">DMS type</param>
		/// <returns>Model type code</returns>
		public ModelCode GetModelCodeFromType(DMSType type)
		{
			ModelCode modelCode;
			if (this.type2modelCode.TryGetValue(type, out modelCode))
			{
				return modelCode;
			}
			else
			{
				string message = string.Format("Failed to get model type code for DMS type: {0}. Invalid DMS type", type);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}

		public bool TypeHasModelCode(DMSType type)
		{
			return this.type2modelCode.ContainsKey(type);
		}	

		/// <summary>
		/// Gets model type code for specified entity id.
		/// </summary>
		/// <param name="id">entity id</param>
		/// <returns>Model type code</returns>		
		public ModelCode GetModelCodeFromId(long id)
		{
			DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(id);

			ModelCode modelCode;
			if (this.type2modelCode.TryGetValue(type, out modelCode))
			{
				return modelCode;
			}
			else
			{
				string message = string.Format("Failed to get model type code for DMS type: {0}. Invalid DMS type. ID = 0x{1:x16}", type, id);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}

		public ModelCode GetModelCodeFromModelCodeName(string modelCodeName)
		{
			ModelCode modelCode;

			// Call Enum.TryParse method.
			if (Enum.TryParse(modelCodeName, true, out modelCode) && Enum.IsDefined(typeof(ModelCode), modelCode))
			{
				return modelCode;
			}

			throw new Exception(string.Format("ModelCode with name {0} dose not exist.", modelCodeName));
		}
				
		public bool ContainsModelCode(ModelCode modelCode)
		{
			return allModelCodes.Contains(modelCode);
		}

		public bool ContainsModelCode(DMSType dmsType)
		{
			return allDMSTypes.Contains(dmsType);
		}

		/// <summary>
		/// Checks if the model type description exists in collection for specified model type code.
		/// </summary>
		/// <param name="resourceId">Code of the model type</param>
		/// <returns>TRUE if model type description exists in collection, FALSE otherwise</returns>
		private bool ResourceExistsForType(long resourceId)
		{
			return resourceDescs.ContainsKey(resourceId);
		}

		/// <summary>
		/// Gets ModelCode from DMSType
		/// </summary>
		/// <param name="type">DMSType</param>
		/// <returns>ModelCode</returns>
		private ModelCode GetModelCodeFromTypeForCore(DMSType type)
		{
			return (ModelCode)Enum.Parse(typeof(ModelCode), type.ToString());
		}

		# region Initialization of metadata

		private void InitializeTypeIdsInInsertOrder()
		{			
			typeIdsInInsertOrder.Add(ModelCode.BASEVOLTAGE);
			typeIdsInInsertOrder.Add(ModelCode.LOCATION);
			typeIdsInInsertOrder.Add(ModelCode.POWERTR);
			typeIdsInInsertOrder.Add(ModelCode.POWERTRWINDING);
			typeIdsInInsertOrder.Add(ModelCode.WINDINGTEST);			
		}

		private void InitializeNotSettablePropertyIds()
		{			
			notSettablePropertyIds.Add(ModelCode.IDOBJ_GID);
			notSettablePropertyIds.Add(ModelCode.BASEVOLTAGE_CONDEQS);
			notSettablePropertyIds.Add(ModelCode.LOCATION_PSRS);
			notSettablePropertyIds.Add(ModelCode.POWERTRWINDING_TESTS);
            notSettablePropertyIds.Add(ModelCode.POWERTR_WINDINGS);	
		}
	
		# endregion Initialization of metadata

		# region Switching between enums and values

		private List<ModelCode> SwitchLongsToModelCodes(List<long> longValues)
		{
			List<ModelCode> result = new List<ModelCode>();

			if (longValues != null)
			{
				foreach (long value in longValues)
				{
					result.Add((ModelCode)value);
				}
			}

			return result;
		}

		private List<long> SwitchModelCodesToLongs(List<ModelCode> modelCodeValues)
		{
			List<long> result = new List<long>();

			if (modelCodeValues != null)
			{
				foreach (ModelCode value in modelCodeValues)
				{
					result.Add((long)value);
				}
			}

			return result;
		}

		private List<DMSType> SwitchShortsToDMSTypes(List<short> shortValues)
		{
			List<DMSType> result = new List<DMSType>();

			if (shortValues != null)
			{
				foreach (long value in shortValues)
				{
					result.Add((DMSType)value);
				}
			}

			return result;
		}

		private List<short> SwitchDMSTypesToShorts(List<DMSType> modelCodeValues)
		{
			List<short> result = new List<short>();

			if (modelCodeValues != null)
			{
				foreach (DMSType value in modelCodeValues)
				{
					result.Add((short)value);
				}
			}

			return result;
		}

		# endregion Switching between enums and values

	}


	#region utility

	/// <summary>
	/// Comparer that should be used when DMSType enum is key in dictionary to avoid unnecessary boxing
	/// </summary>
	public class DMSTypeComparer : IEqualityComparer<DMSType>
	{
		#region IEqualityComparer<DMSType> Members

		public bool Equals(DMSType x, DMSType y)
		{
			return (short)x == (short)y;
		}

		public int GetHashCode(DMSType obj)
		{
			return (int)obj;
		}

		#endregion
	}

	/// <summary>
	/// Comparer that should be used when ModelCode enum is key in dictionary to avoid unnecessary boxing
	/// </summary>
	public class ModelCodeComparer : IEqualityComparer<ModelCode>
	{
		IEqualityComparer<long> comparer;

		public ModelCodeComparer()
		{
			comparer = EqualityComparer<long>.Default;
		}

		#region IEqualityComparer<ModelCode> Members

		public bool Equals(ModelCode x, ModelCode y)
		{
			return (long)x == (long)y;
		}

		public int GetHashCode(ModelCode obj)
		{
			return comparer.GetHashCode((long)obj);
		}

		#endregion
	}

	#endregion utility
}
