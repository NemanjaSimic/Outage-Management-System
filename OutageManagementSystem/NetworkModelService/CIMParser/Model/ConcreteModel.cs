using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CIM.Model
{
    /// <summary>
    /// ConcreteModel class holds the map containing CIM objects.
    /// </summary>
    public class ConcreteModel
    {
        #region FIELDS
        //             MAP<string type,MAP              <id    , object>>
        private SortedDictionary<string, SortedDictionary<string, object>> modelMap = new SortedDictionary<string, SortedDictionary<string, object>>();

        public SortedDictionary<string, SortedDictionary<string, object>> ModelMap
        {
            get
            {
                return modelMap;
            }
            set
            {
                if(value != null)
                {
                    modelMap = value;
                }
            }
        }

        #endregion

		public ConcreteModel()
		{
		}

		public ConcreteModel(SortedDictionary<string, SortedDictionary<string, object>> modelMap)
		{
			this.ModelMap = modelMap;
		}

        public string PrintModel()
        {
            StringBuilder result = new StringBuilder();

            foreach (KeyValuePair<string, SortedDictionary<string, object>> oneType in modelMap)
            {
                result.AppendLine(string.Format("{0} - Count: {1}", oneType.Key, oneType.Value.Count));
            }

            result.AppendLine();
            result.AppendLine();

            return result.ToString();
        }

        public string GetProperty(string mrId, string propertyName)
        { 
            object entity = GetObjectByID(mrId);
            string porpertyValue = entity.GetType().GetProperty(propertyName).GetValue(entity, null).ToString();
            return string.Format("Type: {0}, mrId: {1}, {2}: {3}", entity.GetType(), mrId, propertyName, porpertyValue);
        }

        #region ADD/GET OBJECT METHODS
        /// <summary>
        /// Method inserts given IDClass in Model.
        /// <para>Object will be inserted in submap acording to it's Type and ID property values. </para>
        /// </summary>
        /// <param fullName="IDObject">object for inserting (must not be null)</param>
        public string InsertObjectInModelMap(object IDObject, Type type)
        {
            if (type != null)
            {
                if (IDObject != null)
                {
                    string ID = type.GetProperty("ID").GetValue(IDObject, null).ToString();
                    if (!string.IsNullOrEmpty(ID))
                    {
                        if (modelMap == null)
                        {
                            modelMap = new SortedDictionary<string, SortedDictionary<string, object>>();
                        }

                        SortedDictionary<string, object> typeSubmap;
                        if (!modelMap.ContainsKey(type.ToString()))
                        {
                            typeSubmap = new SortedDictionary<string, object>();
                        }
                        else
                        {
                            typeSubmap = modelMap[type.ToString()];
                            if (typeSubmap == null)
                            {
                                typeSubmap = new SortedDictionary<string, object>();
                            }
                        }

                        if (!typeSubmap.ContainsKey(ID))
                        {
                            typeSubmap.Add(ID, IDObject);
                        }
                        else
                        {
                            return string.Format("Object with ID: {0} already exists in model. Existing instance in model will be kept.",ID);
                        }
                        modelMap.Remove(type.ToString());
                        modelMap.Add(type.ToString(), typeSubmap);
                        return string.Empty;
                    }
                    else
                    {
                        return "ID of object can not be null.";
                    }
                }
                else
                {
                    return "Object can not be null.";
                }
            }
            else
            {
                return "Type can not be null.";
            }
        }


        /// <summary>
        /// Gets the object of type <c>type</c> and with <c>ID</c> id
        /// </summary>
        /// <param name="type">type of object</param>
        /// <param name="ID">ID of object</param>
        /// <returns>found object or null if there is no object with that type and ID</returns>
        public object GetObjectByTypeAndID(Type type, string ID)
        {
            object foundObject = null;
            if(modelMap.ContainsKey(type.ToString()))
            {
                SortedDictionary<string, object> typeObjects = ModelMap[type.ToString()];
                if(typeObjects.ContainsKey(ID))
                {
                    foundObject = typeObjects[ID];
                }
            }
            return foundObject;
        }

		/// <summary>
		/// Removes the object of type <c>type</c> and with <c>ID</c> id
		/// </summary>
		/// <param name="type">type of object</param>
		/// <param name="ID">ID of object</param>
		/// <returns>true if object is removed, otherwise false</returns>
		public bool RemoveObjectByTypeAndID(Type type, string ID)
		{
			bool isRemoved = false;
			if (modelMap.ContainsKey(type.ToString()))
			{
				SortedDictionary<string, object> typeObjects = ModelMap[type.ToString()];
				if (typeObjects.ContainsKey(ID))
				{
					isRemoved = typeObjects.Remove(ID);
				}
			}
			return isRemoved;
		}

        /// <summary>
        /// Method gets map of all objects of requested type from this ConcreteModel.
        /// </summary>
        /// <param name="type">type classifier - must be given with the full namespace string, e.g. "RDF.ACLineSegment"</param>
        /// <returns>null if the result is empty, otherwise map of all matching objects</returns>
        public SortedDictionary<string, object> GetAllObjectsOfType(string type)
        {
            if(modelMap.ContainsKey(type))
            {
                return ModelMap[type];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Method searches and returns the object with given identificator.
        /// <para>If it doesn't find the object with given id, method returns null.</para>
        /// </summary>
        /// <param fullName="identifier">identifier (ID property value) of request object</param>
        /// <returns>requested object or null</returns>
        public object GetObjectByID(string identifier)
        {

            object objFound = null;
            if((modelMap != null) && (!string.IsNullOrEmpty(identifier)))
            {
                foreach(string type in modelMap.Keys)
                {
                    if(modelMap[type].ContainsKey(identifier))
                    {
                        objFound = (modelMap[type])[identifier];
                        break;
                    }
                }
            }
            return objFound;
        }


        /// <summary>
        /// Method gets map of all objects of requested type from this ConcreteModel
        /// that also have a given value of its property.
        /// </summary>
        /// <param name="type">type classifier - must be given with the full namespace string, e.g. "RDF.ACLineSegment"</param>
        /// <param name="property">name of the property that is defined for given type</param>
        /// <param name="value">value of the property (can be simple value or reference to an object) </param>
        /// <returns>null if the result is empty, otherwise map of all matching objects</returns>
        public SortedDictionary<string, object> GetAllObjectsOfType(string type, string property, object value)
        {
            SortedDictionary<string, object> map = null;
            if (value != null)
            {
                if (modelMap.ContainsKey(type))
                {
                    SortedDictionary<string, object> objectsOfTypeMap = ModelMap[type];
                    if (objectsOfTypeMap != null && objectsOfTypeMap.Count != 0)
                    {
                        map = new SortedDictionary<string, object>();
                        foreach (KeyValuePair<string, object> idObjectPair in objectsOfTypeMap)
                        {
                            PropertyInfo pInfo = idObjectPair.Value.GetType().GetProperty(property);
                            if ((pInfo != null) && (value.Equals(pInfo.GetValue(idObjectPair.Value, null))))
                            {
                                map.Add(idObjectPair.Key, idObjectPair.Value);
                            }
                        }
                    }
                }
            }
            return map;
        }

        #endregion
        
        /// <summary>
        /// Returns number of objects in model
        /// </summary>
        /// <returns>number of objects in model (not counting data types)</returns>
        public int ObjectsCount
        {
            get
            {
                int size = 0;
                foreach(string type in modelMap.Keys)
                {
                    size += modelMap[type].Count;
                }
                return size;
            }
        }

    }

}
