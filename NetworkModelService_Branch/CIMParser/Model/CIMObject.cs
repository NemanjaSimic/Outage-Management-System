using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CIM.Model
{
    /// <summary>
    /// Class CIMObject represents one CIM object.
    /// <para> Each object consists of: </para>
    /// <para> - informations about it's type and uniqe ID (string properties),</para>
    /// <para> - map of it's attributes (key in the map: value of FullName property of atrribute), </para>
    /// <para> - map of all parent objects IDs which are linked to it (packed in lists by value of CIMType property),</para>
    /// <para> - map of all child objects IDs which are linked to it (packed in lists by value of CIMType property).</para>
    /// <para></para> 
    /// <remarks>
    /// <para> parent == object that is referenced by this object (by one of it's attributes)</para>
    /// <para> child == object that has one reference to this object </para>
    /// </remarks>
    /// <para>@author: Stanislava Selena</para>
    /// </summary>
    public class CIMObject
    {
        protected string type; //// type of CIM object
        protected string id; //// unique object ID
        protected string name = string.Empty; //// value of the IdentifiedObject.name attribute (or XXX.name) if it is defined
        protected string namespaceString = CIMConstants.CIMNamespace;        
		protected CIMModelContext modelContext;
		protected int typeCode; //// code of CIM object type

        //// map of all atributes of this CIM object
        protected SortedDictionary<int, List<ObjectAttribute>> myAttributes; //// <int (code of fullName), Attribute>        

        //// the IDs of all objects linked to this object packed in diferent lists by object type
        protected SortedDictionary<string, List<string>> parents; //// <string (type), List<string (mrid) > >
        protected SortedDictionary<string, List<string>> children; //// <string (type), List<string (mrid) > >
        
        //// special category of children : embedded children - all of these children are also in the children map
        protected SortedDictionary<string, List<string>> embeddedChildren; //// <string (embeddedListName), List<string (mrid) > >
        protected string embeddedParentId = string.Empty;
        protected bool isDefinedAsEmbedded = false;

        //// the beginin/end text in source file where object is defined
        protected long sourceStartLine = 0;
        protected long sourceStartColumn = 0;
        protected long sourceEndLine = 0;
        protected long sourceEndColumn = 0;

        //// binded profile definition of object's type 
        protected ProfileElement isInstanceOfType = null;
        protected int validationStatus = 0;
        protected List<string> validationMessages = null;

        protected bool isModified = false;


		//public CIMObject()
		//{
		//    type = string.Empty;
		//    id = null;
		//    myAttributes = new SortedDictionary<int, List<ObjectAttribute>>();
		//    children = new SortedDictionary<string, List<string>>();
		//    parents = new SortedDictionary<string, List<string>>();
		//}

		public CIMObject(CIMModelContext cimModelContext, string typeName)
        {
			this.modelContext = cimModelContext;
			typeCode = -1;
			if (this.modelContext != null)
			{
				typeCode = modelContext.AddOrReadCodeForCIMType(typeName);
			}
            id = null;
            myAttributes = new SortedDictionary<int, List<ObjectAttribute>>();
            children = new SortedDictionary<string, List<string>>();
            parents = new SortedDictionary<string, List<string>>();            
        }
        /// <summary>
        /// Gets and sets the identifed type of this CIMObject.
        /// </summary>
        public string CIMType
        {
            set
            {
                type = value;
            }
            get
            {
                return type;
            }
        }

        /// <summary>
        /// Gets and sets the unique identifer of this CIMObject.
        /// </summary>
        public string ID
        {
            set
            {
                id = value;
            }
            get
            {
                return id;
            }
        }

        /// <summary>
        /// Gets the value of name attribute of this CIMObject or empty string if
        /// is not defined.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets and sets the namespace qualyfier of CIM object.
        /// </summary>
        public string NamespaceString
        {
            set
            {
                namespaceString = value;
            }
            get
            {
                return namespaceString;
            }
        }

        /// <summary>
        /// Gets the namespace qualyfier of CIM object with ":" at the end when namespace is not empty.
        /// </summary>
        public string NamespacePrintString
        {
            get
            {
                if (!string.IsNullOrEmpty(namespaceString))
                {
                    return namespaceString + Manager.StringManipulationManager.SeparatorColon;
                }
                else
                {
                    return namespaceString;
                }
            }
        }

        /// <summary>
        /// Gets and sets the information of whether this object is defined as inner instance.
        /// </summary>
        public bool IsDefinedAsEmbedded
        {
            get
            {
                return isDefinedAsEmbedded;
            }
            set
            {
                isDefinedAsEmbedded = value;
            }
        }        

        /// <summary>
        /// Gets and sets the indication of whether or not this
        /// CIMObject has been modified by user.
        /// </summary>
        public bool IsModified
        {
            get
            {
                return isModified;
            }
            set
            {
                isModified = value;
            }
        }

        /// <summary>
        /// Gets and sets the line index where data of this CIMObject
        /// starts inside the parent file.
        /// </summary>
        public long SourceStartLine
        {
            get
            {
                return sourceStartLine;
            }
            set
            {
                sourceStartLine = value;
            }
        }

        /// <summary>
        /// Gets and sets the column index where data of this CIMObject
        /// starts inside the parent file.
        /// </summary>
        public long SourceStartColumn
        {
            get
            {
                return sourceStartColumn;
            }
            set
            {
                sourceStartColumn = value;
            }
        }

        /// <summary>        
        /// Gets and sets the line index where data of this CIMObject
        /// ends inside the parent file.
        /// </summary>
        public long SourceEndLine
        {
            get
            {
                return sourceEndLine;
            }
            set
            {
                sourceEndLine = value;
            }
        }

        /// <summary>
        /// Gets and sets the column index where data of this CIMObject
        /// ends inside the parent file.
        /// </summary>
        public long SourceEndColumn
        {
            get
            {
                return sourceEndColumn;
            }
            set
            {
                sourceEndColumn = value;
            }
        }

        /// <summary>
        /// Gets and sets the map of all attributes (ObjectAttributes) of this CIMObject.
        /// <para>Key in the map matches the value of "FullName" property of each attribute.</para> 
        /// </summary>
		public SortedDictionary<int, List<ObjectAttribute>> MyAttributes
		{
			get
			{
				return myAttributes;
			}
			set
			{
				myAttributes = value;
			}
		}       

        /// <summary>
        /// Gets the list of all attributes (ObjectAttributes) of this CIMObject.
        /// <para>NOTE: This list may include duplicate attributes if they exist.</para>
        /// </summary>
        public List<ObjectAttribute> MyAttributesAsList
        {
			get
			{
				List<ObjectAttribute> attributesList = null;
				if ((myAttributes != null) && (myAttributes.Count > 0))
				{
					attributesList = new List<ObjectAttribute>();
					foreach (int attrCode in myAttributes.Keys)
					{
						foreach (ObjectAttribute attribute in myAttributes[attrCode])
						{
							attributesList.Add(attribute);
						}
					}
				}
				return attributesList;
			}
		}

        /// <summary>
        /// Gets the boolean indicator whether or not this CIMObject has some ObjectAttributes in MyAttributes list.
        /// </summary>
        public bool HasAttributes
        {            
            get
            {
                bool hasAttributes = false;
                if ((myAttributes != null) && (myAttributes.Count > 0))
                {
                    hasAttributes = true;
                }
                return hasAttributes;
            }
        }        

        /// <summary>
        /// Gets the short description for this CIMObject.
        /// <para>Description contains type and identifier informations.</para>
        /// </summary>
        public StringBuilder ShortDescription
        {
            get
            {
                StringBuilder description = new StringBuilder(string.Empty);
                if (!string.IsNullOrEmpty(type))
                {
                    description.Append("Type : ");
                    description.Append(type);
                    description.AppendLine(";");
                }
                if (!string.IsNullOrEmpty(id))
                {
                    description.Append("ID : ");
                    description.Append(id);
                    description.AppendLine(";");
                }
                if (description.Length > 0)
                {
                    description.AppendLine("-----------------");
                }
                return description;
            }
        }

        /// <summary>
        /// Gets and sets the ID of parent object inside of which
        /// this object is embedded child (is defined inside of parent object).
        /// </summary>
        public string EmbeddedParentId
        {
            get
            {
                return embeddedParentId;
            }
            set
            {
                embeddedParentId = value;
            }
        }

        /// <summary>
        /// Gets the ProfileElement of which this CIMObject is instance.
        /// <para>Can be null if validation to profile wasn't called or it this object is not supported by curren profile.</para>
        /// </summary>
        public ProfileElement IsInstanceOfType
        {
            get
            {
                return isInstanceOfType;
            }
            set
            {
                isInstanceOfType = value;
            }
        }

		/// <summary>
		/// Gets the CIM model context instance of this object.
		/// </summary>
		public CIMModelContext ModelContext
		{
			get
			{
				return modelContext;
			}
		}

        /// <summary>
        /// Gets and sets the validation status, where:
        /// <para> 0 - validation process was not jet performed</para>
        /// <para> 1 - object is fully valid</para>
        /// <para>-1 - object is not valid</para>
        /// <para>See also: <see cref="ValidationStatusExtended"/></para>
        /// </summary>
        public int ValidationStatus
        {
            get
            {
                return validationStatus;
            }
            set
            {
                validationStatus = value;
            }
        }

        /// <summary>
        /// Gets the validation status for object and it's attributes
        /// i.e. validation status can't be 0 or 1 if some attribute of object has -1 validation status.
        /// <para>See also: <see cref="ValidationStatus"/></para>
        /// </summary>
		public int ValidationStatusExtended
		{
			get
			{
				int status = validationStatus;
				if (myAttributes != null)
				{
					foreach (int attrCode in myAttributes.Keys)
					{
						foreach (ObjectAttribute attribute in myAttributes[attrCode])
						{
							if (attribute.ValidationStatus == 2)
							{
								status = -1;
							}
						}
					}
				}
				return status;
			}
		}

        /// <summary>
        /// Gets the list of identifiers of validation messages attached to this object
        /// during validation process.
        /// </summary>
        public List<string> ValidationMessages
        {
            get
            {
                return validationMessages;
            }
        }

        #region public : Attributes methods
        /// <summary>
        /// Method adds new attribute into existing map of all attributes (ObjectAttributes) of this CIMObject.
        /// <para>If MyAttributes map is null, it will be initialized and then the attribute will be added.</para>
        /// <para>If attribute with given FullName property already exists in the MyAttributes map, it will be added into the
        /// MyDuplicateAttributes list.</para>
        /// </summary>
        /// <param name="attr">object attribute</param>
        /// <returns>true if attribute has been successfuly added, false if there was duplicate attribute</returns>
		public bool AddAttribute(ObjectAttribute attr)
		{
			bool success = false;
			if (myAttributes == null)
			{
				myAttributes = new SortedDictionary<int, List<ObjectAttribute>>();
			}

			List<ObjectAttribute> attrList = null;
			if (!myAttributes.ContainsKey(attr.Code))
			{
				attrList = new List<ObjectAttribute>();
				//// add attribute in list and in map
				attrList.Add(attr);
				myAttributes.Add(attr.Code, attrList);
				success = true;
			}
			else
			{
				myAttributes.TryGetValue(attr.Code, out attrList);
				attrList.Add(attr);
				success = true;
			}

			if (string.Compare(CIMConstants.AttributeNameIdentifiedObjectName, attr.FullName) == 0)
			{
				name = attr.Value;
			}
			return success;
		}

        /// <summary>
        /// Method returns object attribute of this CIMObject with given FullName value, 
        /// <para>or null if the requested fullName value isn't found as one of key values in the MyAttributes map.</para>
        /// </summary>
        /// <param fullName="fullName">value of FullName property of requested attribute from MyAttributes map of this CIM object</param>
        /// <param name="multipleAtts">in case that there are multiple attributes with same fullName, this is the list of duplicates (duplicates aren't always error - it depends on profile definition)</param>
        /// <returns>ObjectAttribute with given full fullName or null</returns>
        public ObjectAttribute GetAttribute(string fullName, out List<ObjectAttribute> multipleAtts)
        {
			int attributeCode = modelContext.ReadCodeOfAttribute(fullName);
			ObjectAttribute attribute = null;
			multipleAtts = null;
			if (myAttributes != null)
			{
				if (myAttributes.ContainsKey(attributeCode))
				{
					if ((myAttributes[attributeCode] != null) && (myAttributes[attributeCode].Count > 0))
					{
						attribute = myAttributes[attributeCode][0];
						if (myAttributes[attributeCode].Count > 1)
						{
							multipleAtts = myAttributes[attributeCode].GetRange(1, myAttributes[attributeCode].Count - 1);
						}
					}
				}
			}
			return attribute;
        }

        /// <summary>
        /// Method returns all object attribute of this CIMObject with given FullName value, 
        /// <para>or null if the requested fullName value isn't found as one of key values in the MyAttributes map.</para>
        /// </summary>
        /// <param fullName="fullName">value of FullName property of requested attribute(s) from MyAttributes map of this CIM object</param>        
        /// <returns>list of ObjectAttributes with given full fullName or null</returns>
        public List<ObjectAttribute> GetAttributeList(string fullName)
        {
			int attributeCode = modelContext.ReadCodeOfAttribute(fullName);
			List<ObjectAttribute> attributes = null;
			if (myAttributes != null)
			{
				if (myAttributes.ContainsKey(attributeCode))
				{
					if ((myAttributes[attributeCode] != null) && (myAttributes[attributeCode].Count > 0))
					{
						attributes = myAttributes[attributeCode];
					}
				}
			}
			return attributes;
        }

        /// <summary>
        /// Method removes the object attribute(s) with given FullName value from the MyAttributes list of
        /// this CIMObject.
        /// </summary>
        /// <param name="fullName">value of FullName property of attribute from MyAttributes map of this CIM object</param>
        public void RemoveAttributes(string fullName)
        {
			int attributeCode = modelContext.ReadCodeOfAttribute(fullName);
			if (myAttributes != null)
			{
				if (myAttributes.ContainsKey(attributeCode))
				{
					myAttributes.Remove(attributeCode);
					if (string.Compare(fullName, CIMConstants.AttributeNameIdentifiedObjectName) == 0)
					{
						name = string.Empty;
					}
				}
			}            
        }

        /// <summary>
        /// Method removes the object attribute with given FullName value from the MyAttributes list of
        /// this CIMObject.
        /// <para>In case if there are multiple attributes wth the same FullName value, parameter index determines which one
        /// will be removed.</para>
        /// </summary>
        /// <param name="fullName">value of FullName property of attribute from MyAttributes map of this CIM object</param>
        public void RemoveAttribute(string fullName, int index)
        {
			int attributeCode = modelContext.ReadCodeOfAttribute(fullName);
			if (myAttributes != null)
			{
				if (myAttributes.ContainsKey(attributeCode))
				{
					List<ObjectAttribute> attrList = null;
					myAttributes.TryGetValue(attributeCode, out attrList);
					if ((attrList != null) && (attrList.Count > 0))
					{
						if (attrList.Count == 1) /// remove entirely
						{
							RemoveAttributes(fullName);
						}
						else
						{
							if ((index >= 0) && (index < attrList.Count))
							{
								attrList.RemoveAt(index);
							}
						}
					}
				}
			}
        }

		/// <summary>
		/// Method removes the given object attribute from the MyAttributes list of
		/// this CIMObject.
		/// </summary>
		/// <param name="objectAttribute">attribute from MyAttributes map of this CIM object</param>
		public void RemoveAttribute(ObjectAttribute objectAttribute)
		{
			if ((myAttributes != null) && (objectAttribute != null))
			{
				if (myAttributes.ContainsKey(objectAttribute.Code))
				{
					List<ObjectAttribute> attrList = null;
					myAttributes.TryGetValue(objectAttribute.Code, out attrList);
					if ((attrList != null) && (attrList.Contains(objectAttribute)))
					{
						attrList.Remove(objectAttribute);
					}
				}
			}
		}

        /// <summary>
        /// Method checks if attribute with given fullName exists in MyAttributes map.
        /// </summary>
        /// <param fullName="fullName">value of FullName property of requested attribute</param>
        /// <returns>true if specified fullName exists as a key in MyAttributes map, otherwise false</returns>
        public bool AttributeExists(string fullName)
        {
			bool exists = false;
			if (myAttributes != null)
			{
				exists = myAttributes.ContainsKey(modelContext.ReadCodeOfAttribute(fullName));
			}
			return exists;
        }

        /// <summary>
        /// Method returns all attributes who have given value of ShortName property.
        /// </summary>
        /// <param name="shortName"></param>
        /// <returns>list of object's attributes</returns>
        public List<ObjectAttribute> FindAttributeByShortName(string shortName)
        {
			List<ObjectAttribute> attributes = null;
			if ((myAttributes != null) && !string.IsNullOrEmpty(shortName))
			{
				foreach (int attrCode in myAttributes.Keys)
				{
					foreach (ObjectAttribute attribute in myAttributes[attrCode])
					{
						if (string.Compare(attribute.ShortName, shortName) == 0)
						{
							if (attributes == null)
							{
								attributes = new List<ObjectAttribute>();
							}
							attributes.Add(attribute);
						}
					}
				}
			}
			return attributes;
        }

        /// <summary>
        /// Method prepares list of all attributes which has given value.
        /// </summary>
        /// <param name="value">string value of requested object atribute(s)</param>
        /// <param name="isReference">indicator whether or not requested object attribute(s) are references</param>
        /// <returns></returns>
        public List<ObjectAttribute> GetMyAttributesWithValue(string value, bool isReference)
        {
			List<ObjectAttribute> attributesWithValue = null;
			if (myAttributes != null)
			{
				foreach (int attrCode in myAttributes.Keys)
				{
					foreach (ObjectAttribute attribute in myAttributes[attrCode])
					{
						if (attribute.IsReference == isReference)
						{
							if ((!isReference && (string.Compare(attribute.Value, value) == 0))
								|| (isReference && (string.Compare(attribute.ValueOfReference, value) == 0)))
							{
								if (attributesWithValue == null)
								{
									attributesWithValue = new List<ObjectAttribute>();
								}
								attributesWithValue.Add(attribute);
							}
						}
					}
				}
			}
			return attributesWithValue;
        }
        #endregion public : Attributes methods


        #region public : Parents map methods

        /// <summary>
        /// Method sets map of all parent objects(theirs MRIDs) of this CIMObject.
        /// <remarks>
        /// <para>Parent object is an object that is referenced by this object (by one of it's attributes).</para>
        /// <para>Map architecture: Map[string ("type" of objects), List{string (ID of object)} ]. </para>
        /// </remarks>
        /// </summary>
        /// <param name="parentsMap">new value for parents map of this CIM object</param>
        public void SetParents(SortedDictionary<string, List<string>> parentsMap)
        {
            parents = parentsMap;
        }

        /// <summary>
        /// Method adds given list of CIMObject IDs to the parents map (<see cref="M:SetParents"/>)
        /// with the given type as key in map.
        /// </summary>
        /// <param name="type">Type of all CIMObjects whose IDs are in the IDList</param>
        /// <param name="IDList">list of IDs of parent objects</param>
        public void SetParentsOfType(string type, List<string> IDList)
        {
            if (!string.IsNullOrEmpty(type) && (IDList != null))
            {
                if (parents == null)
                {
                    parents = new SortedDictionary<string, List<string>>();
                }
                parents.Add(type, IDList);
            }
        }

        /// <summary>
        /// Method returns map of all parent objects(theirs IDs) of this CIMObject.
        /// <remarks>
        /// <para>Parent object is an object that is referenced by this object (by one of it's attributes).</para>
        /// <para>Map architecture: Map[string ("type" of objects), List{string (ID of object)} ]. </para>
        /// </remarks> 
        /// </summary>
        /// <returns>parents map</returns>
        public SortedDictionary<string, List<string>> GetParents()
        {
            return parents;
        }

        /// <summary>
        /// Method gets the ID(string) list of all paren objects of given type.
        /// <para>See also: <seealso cref="M:SetParentsOfType"/></para>
        /// </summary>
        /// <param name="type">Type of parent CIMObjects</param>
        /// <returns></returns>
        public List<string> GetParentsOfType(string type)
        {
            List<string> parentsOfType = null;
            if (parents.ContainsKey(type))
            {
                parentsOfType = parents[type];
            }
            return parentsOfType;
        }

        /// <summary>
        /// Method adds new parent object(it's ID) into existing map of all parents of this CIMObject.
        /// <para>Parent object must have not empty values for it's properties CIMType and ID, or it won't be added to the map.</para>
        /// <para>In the parents map, parent is added in the following way: </para>
        /// <para>value of CIMType property defines a key in the main map, and the value of ID property is being added in the List{string} which correspondes to that key.</para>
        /// </summary>
        /// <param name="cimObject">new parent object (CIMObject) which will be addeed into parents map of this CIM object</param>
        /// <returns>true if adding parent is successful, otherwise false</returns>
		public bool AddParent(CIMObject cimObject)
		{
			bool success = false;
			if ((!string.IsNullOrEmpty(cimObject.CIMType)) && (!string.IsNullOrEmpty(cimObject.ID)))
			{
				if (parents.ContainsKey(cimObject.CIMType))
				{
					parents[cimObject.CIMType].Add(cimObject.id);
				}
				else
				{
					List<string> temp = new List<string>();
					temp.Add(cimObject.id);
					parents.Add(cimObject.CIMType, temp);
				}

				success = true;
			}
			return success;
		}

        /// <summary>
        /// Method removes given parent object(it's ID) from map of all parents of this CIMObject.        
        /// </summary>
        /// <param name="cimObjectID">ID of CIMObject which will be removed from parents map of this CIM object</param>
        /// <returns>true if removing parent is successful, otherwise false</returns>
        public bool RemoveParent(string cimObjectID)
        {
            bool success = false;
            if ((parents != null) && !string.IsNullOrEmpty(cimObjectID))
            {
                string typeOfObject = string.Empty;
                foreach (string type in parents.Keys)
                {
                    if (parents[type].Contains(cimObjectID))
                    {
                        success = parents[type].Remove(cimObjectID);
                        typeOfObject = type;
                        break;
                    }
                }

                if (success && !string.IsNullOrEmpty(typeOfObject) && parents.ContainsKey(typeOfObject))
                {
                    if (parents[typeOfObject].Count == 0)
                    {
                        parents.Remove(typeOfObject);
                    }
                }
            }
            return success;
        }
        #endregion public : Parents map methods


        #region public : Children map methods

        /// <summary>
        /// Method sets map of all children objects(theirs IDs) of this CIMObject.
        /// <remarks>
        /// <para>Child object is an object that has at least one reference to this object.</para>
        /// <para>Map architecture: Map[string ("type" of objects), List{string (ID of object)}].</para>
        /// </remarks>
        /// </summary>
        /// <param name="childrenMap">new value for children map of this CIM object</param>
        public void SetChildren(SortedDictionary<string, List<string>> childrenMap)
        {
            children = childrenMap;
        }

        /// <summary>
        /// Method adds given list of CIMObject IDs to the children map (<see cref="M:SetChildren"/>)
        /// with the given type as key in map.
        /// </summary>
        /// <param name="type">Type of all CIMObjects whose IDs are in the IDList</param>
        /// <param name="IDList">list of IDs of children objects</param>
        public void SetChildrenOfType(string type, List<string> IDList) 
        {
            if (!string.IsNullOrEmpty(type) && (IDList != null))
            {
                if (children == null)
                {
                    children = new SortedDictionary<string, List<string>>();
                }
                if (children.ContainsKey(type))
                {
                    children[type] = IDList;
                }
                else
                {
                    children.Add(type, IDList);
                }
            }
        }

        /// <summary>
        /// Method gets map of all children objects(theirs IDs) of this CIMObject.
        /// <remarks>
        /// <para>Child object is an object that has at least one reference to this object.</para>
        /// <para>Map architecture: Map[string ("type" of objects), List{string (ID of object)}].</para>
        /// </remarks>
        /// </summary>
        /// <returns>children map</returns>
        public SortedDictionary<string, List<string>> GetChildren()
        {
            return children;
        }

        /// <summary>
        /// Method gets the ID(string) list of all children objects of given type.
        /// <para>See also: <seealso cref="M:SetChildrenOfType"/></para>
        /// </summary>
        /// <param name="type">Type of children CIMObjects</param>
        /// <returns></returns>
        public List<string> GetChildrenOfType(string type)
        {
            List<string> childrenOfType = new List<string>();
            if (children.ContainsKey(type))
            {
                childrenOfType = children[type];                
            }
            return childrenOfType;
        }
        
        /// <summary>
        /// Method adds new child object(it's ID) into existing map of all children of this CIMObject.
        /// <para>Child object must have not empty values for it's properties CIMType and ID, or it won't be added to the map.</para>
        /// <para>In the children map, child is added in the following way: </para>
        /// <para>value of CIMType property defines a key in the main map, and the value of ID property is being added in the List{string} which correspondes to that key.</para>
        /// </summary>
        /// <param name="cimObject">new child object (CIMObject) which will be addeed into children map of this CIM object</param>
        /// <returns>true if adding child is successful, otherwise false</returns>
		public bool AddChild(CIMObject cimObject)
		{
			bool success = false;
			if ((!string.IsNullOrEmpty(cimObject.CIMType)) && (!string.IsNullOrEmpty(cimObject.ID)))
			{
				if (children.ContainsKey(cimObject.CIMType))
				{
					children[cimObject.CIMType].Add(cimObject.id);
				}
				else
				{
					List<string> temp = new List<string>();
					temp.Add(cimObject.ID);
					children.Add(cimObject.CIMType, temp);
				}
				success = true;
			}
			return success;
		}

        /// <summary>
        /// Method removes given child object(it's ID) from map of all children of this CIMObject.        
        /// </summary>
        /// <param name="cimObjectID">ID of CIMObject which will be removed from children map of this CIM object</param>
        /// <returns>true if removing child is successful, otherwise false</returns>
        public bool RemoveChild(string cimObjectID)
        {
            bool success = false;
            if ((children != null) && !string.IsNullOrEmpty(cimObjectID))
            {
                string typeOfObject = string.Empty;
                foreach (string type in children.Keys)
                {
                    if (children[type].Contains(cimObjectID))
                    {
                        success = children[type].Remove(cimObjectID);
                        typeOfObject = type;
                        break;
                    }
                }

                if (success && !string.IsNullOrEmpty(typeOfObject) && children.ContainsKey(typeOfObject))
                {
                    if (children[typeOfObject].Count == 0)
                    {
                        children.Remove(typeOfObject);
                    }
                }
            }
            return success;
        }
        #endregion public : Children map methods


        #region public : Embedded children methods

        /// <summary>
        /// Method sets map of all embedded children objects(theirs IDs) of this CIMObject.
        /// <remarks>
        /// <para>Embedde child object is an object that is defined inside this object.</para>
        /// <para>Map architecture: Map[string (name of embedded category), List{string (ID of object)}].</para>
        /// </remarks>
        /// </summary>
        /// <param name="childrenMap">new value for embeded children map of this CIM object</param>
        public void SetEmbeddedChildren(SortedDictionary<string, List<string>> embeddedChildrenMap)
        {
            embeddedChildren = embeddedChildrenMap;
        }

        /// <summary>
        /// Method adds given list of CIMObject IDs to the embeded children map (<see cref="M:SetEmbeddedChildren"/>)
        /// with the given embeddedCategory name as key in map.
        /// </summary>
        /// <param name="embeddedCategory">name of embedded category list</param>
        /// <param name="IDList">list of IDs of embeded children objects</param>
        public void SetEmbededdChildrenOfCategory(string embeddedCategory, List<string> IDList)
        {
            if (!string.IsNullOrEmpty(embeddedCategory) && (IDList != null))
            {
                if (embeddedChildren == null)
                {
                    embeddedChildren = new SortedDictionary<string, List<string>>();
                }
                if (embeddedChildren.ContainsKey(embeddedCategory))
                {
                    embeddedChildren[embeddedCategory] = IDList;
                }
                else
                {
                    embeddedChildren.Add(embeddedCategory, IDList);
                }
            }
        }

        /// <summary>
        /// Method gets map of all embedded children objects(theirs IDs) of this CIMObject.
        /// <remarks>
        /// <para>Embedde child object is an object that is defined inside this object.</para>
        /// <para>Map architecture: Map[string (name of embedded category), List{string (ID of object)}].</para>
        /// </remarks>
        /// </summary>
        /// <returns>children map</returns>
        public SortedDictionary<string, List<string>> GetEmbeddedChildren()
        {
            return embeddedChildren;
        }

        /// <summary>
        /// Method gets the ID(string) list of all embedded children objects of given embedded category.
        /// <para>See also: <seealso cref="M:SetEmbededdChildrenOfCategory"/></para>
        /// </summary>
        /// <param name="embeddedCategory">name of embedded category list</param>
        /// <returns></returns>
        public List<string> GetEmbededdChildrenOfCategory(string embeddedCategory)
        {
            List<string> childrenOfType = new List<string>();
            if (embeddedChildren.ContainsKey(embeddedCategory))
            {
                childrenOfType = embeddedChildren[embeddedCategory];
            }
            return childrenOfType;
        }

        /// <summary>
        /// Method tries to find the name of embedded category inside of which is object with given id.
        /// </summary>
        /// <param name="embeddedChildId">ID of embedded child object</param>
        /// <returns>name of embedded category list inside of which is given child object or empty string</returns>
        public string GetEmbeddedCategoryNameOfEmbeddedChild(string embeddedChildId)
        {
            string embeddedCategory = string.Empty;
            if ((embeddedChildren != null) && !string.IsNullOrEmpty(embeddedChildId))
            {
                foreach (string ec in embeddedChildren.Keys)
                {
                    if (embeddedChildren[ec].Contains(embeddedChildId))
                    {
                        embeddedCategory = ec;
                        break;
                    }
                }
            }
            return embeddedCategory;
        }

        /// <summary>
        /// Method adds new embedded child object(it's ID) into existing map of all 
        /// embedded children of this CIMObject.
        /// <para>Child object must have not empty value for it's ID property, or it won't be added to the map.</para>        
        /// </summary>
        /// <param name="cimObject">new embedded child object (CIMObject)</param>
        /// <returns>true if adding embedded child is successful, otherwise false</returns>
		public bool AddEmbeddedChild(string embeddedCategory, CIMObject cimObject)
		{
			bool success = false;
			if ((!string.IsNullOrEmpty(embeddedCategory)) && (cimObject != null) && (!string.IsNullOrEmpty(cimObject.ID)))
			{
				if (embeddedChildren == null)
				{
					embeddedChildren = new SortedDictionary<string, List<string>>();
				}

				if (embeddedChildren.ContainsKey(embeddedCategory))
				{
					embeddedChildren[embeddedCategory].Add(cimObject.ID);
				}
				else
				{
					List<string> temp = new List<string>();
					temp.Add(cimObject.ID);
					embeddedChildren.Add(embeddedCategory, temp);
				}
				success = true;
			}
			return success;
		}

        #endregion public : Embedded children methods


        #region public : Validation data methods
        /// <summary>
        /// Method adds identifier of validation message which is attached to this object 
        /// during validation process.
        /// </summary>
        /// <param name="validationMessageId"></param>
        public void AddValidationMessage(string validationMessageId)
        {
            if (!string.IsNullOrEmpty(validationMessageId))
            {
                validationStatus = -1;
                if (validationMessages == null)
                {
                    validationMessages = new List<string>();
                }
                if (!validationMessages.Contains(validationMessageId))
                {
                    validationMessages.Add(validationMessageId);
                }
            }
        }

        /// <summary>
        /// Method resets the validation informations for this object.
        /// </summary>
        public void ClearValidationInformations()
        {
            isInstanceOfType = null;
            validationStatus = 0;
            validationMessages = null;
        }
        #endregion public : Validation data methods        


        public override bool Equals(object obj)
        {
            bool eq = false;
            if ((obj != null) && (obj is CIMObject))
            {
                eq = (string.Compare(this.ID, ((obj as CIMObject).ID)) == 0);
            }
            return eq;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public override string ToString()
        {
            string retVal = "Identified Object :  type = ";
            if (type != null)
                retVal += type + ", id = ";
            else
                retVal += "- , id = ";
            if (id != null)
                retVal += id + "\n";
            else
                retVal += "- \n";

            foreach (int attId in myAttributes.Keys)
            {
                retVal += myAttributes[attId].ToString() + "\n";
            }

            retVal += "children elements : \n";
            if (children.Count > 0)
                foreach (string childType in children.Keys)
                {
                    retVal += "\ttype = " + childType + "\n\tlist = ";
                    for (int i = 0; i < children[childType].Count; i++)
                        retVal += children[childType][i] + " , ";
                    retVal += "\n\t****\n";
                }
            else
                retVal += " - \n";

            retVal += "parent elements : \n";
            if (parents.Count > 0)
                foreach (string parentType in parents.Keys)
                {
                    retVal += "\ttype = " + parentType + "\n\tlist = ";
                    for (int i = 0; i < parents[parentType].Count; i++)
                        retVal += parents[parentType][i] + " , ";
                    retVal += "\n\t****\n";
                }
            else
                retVal += " - \n";

            return retVal;
        }               
    }
}
