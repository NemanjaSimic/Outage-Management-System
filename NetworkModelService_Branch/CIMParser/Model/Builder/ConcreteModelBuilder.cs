using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Globalization;
using CIM.Manager;

namespace CIM.Model
{
	public class ConcreteModelBuilder
	{
        private enum MessageLevel
        {
            INFO,
            ERROR,
            WARNING
        }
        
        private ConcreteModelBuildingResult result;
        private string activeNamespace;
        private Dictionary<string, List<string>> missingValuesMap = new Dictionary<string, List<string>>();
        
		#region Public methods

        public Dictionary<string, List<string>> GetMissingValuesMap()
        {
            return missingValuesMap;
        }

        public string ActiveNamespace
        {
            get { return activeNamespace; }
        }

		/// <summary>
		/// Creates ConcreteModel from CIMModel using classes from assembly
		/// </summary>
		/// <param name="CIM_Model">model of cim/xml that is used to create ConcreteModel</param>
		/// <param name="assembly">contains classes for model</param>
		public ConcreteModelBuildingResult GenerateModel(CIMModel CIM_Model, Assembly assembly, string type, ref ConcreteModel model)
		{
            //model = new ConcreteModel();
            result = new ConcreteModelBuildingResult();
            activeNamespace = type;
 
			if(CIM_Model != null && assembly != null)
			{
                try
                {
                    //two iterations 
                    //- first - only simple value/enumerations and instantiate the class
                    //- second - create connections
                    InstantiateClassesWithSimpleValues(CIM_Model, assembly, ref model);

                    //second iteration for setting up references (excluding dataTypes)GOES HERE
                    ConnectModelElements(CIM_Model, assembly, ref model);

                    result.MissingValues = GenerateMissingMandatoryValuesReport(model, assembly);
                }
                catch(Exception ex)
                {
                    OnMessage(ex.Message, MessageLevel.ERROR);
                }
			}
			else
			{
				OnMessage("Model or assembly are not selected.", MessageLevel.ERROR);
			}

			return result;
		}
        #endregion

        #region MissingMandatoryValues
        private StringBuilder GenerateMissingMandatoryValuesReport(ConcreteModel model, Assembly assembly)
        {
            StringBuilder report = new StringBuilder();
            //extract names
            Dictionary<string, string> names = GetNames(model.ModelMap, assembly);

            foreach (KeyValuePair<string, SortedDictionary<string, object>> kv in model.ModelMap)
            {
                Type type = assembly.GetType(kv.Key);
                List<PropertyInfo> pInfoList = GetIsMandatoryProperties(type);

                foreach(PropertyInfo pInfo in pInfoList)
                {
                    string propertyName = pInfo.Name.Substring("is".Length);
                    propertyName = propertyName.Substring(0,propertyName.Length - "mandatory".Length);
                    PropertyInfo hasValueProp = type.GetProperty(propertyName + "HasValue");

                    bool firstOccurance = true;
                    foreach (KeyValuePair<string, object> idObjectPair in kv.Value) 
                    {
                        if (idObjectPair.Value != null) 
                        {
                            if (!(bool)hasValueProp.GetValue(idObjectPair.Value, null)) 
                            {
                                //no value so add to report
                                if (firstOccurance) 
                                {
                                    report.AppendLine("[WARNING] Missing mandatory property " + type.Name + "." + propertyName + " on following entities:");
                                    if (this.missingValuesMap.ContainsKey(idObjectPair.Key))
                                    {
                                        if (!this.missingValuesMap[idObjectPair.Key].Contains(propertyName))
                                        {
                                            this.missingValuesMap[idObjectPair.Key].Add(propertyName);
                                        }
                                    }
                                    else
                                    {
                                        this.missingValuesMap.Add(idObjectPair.Key, new List<string>() { propertyName });
                                    }

                                    firstOccurance = false;
                                }

                                if (this.missingValuesMap.ContainsKey(idObjectPair.Key))
                                {
                                    if (!this.missingValuesMap[idObjectPair.Key].Contains(propertyName))
                                    {
                                        this.missingValuesMap[idObjectPair.Key].Add(propertyName);
                                    }
                                }
                                else
                                {
                                    this.missingValuesMap.Add(idObjectPair.Key, new List<string>() { propertyName });
                                }

                                report.AppendLine("\t - ID: " + idObjectPair.Key + ";" + GetName(names, idObjectPair.Key));
                            }
                        }
                    }
                }
            }
            return report;
        }

        private Dictionary<string, string> GetNames(SortedDictionary<string, SortedDictionary<string, object>> modelMap, Assembly assembly)
        {
            Dictionary<string, string> idToNames = new Dictionary<string, string>();
            SortedDictionary<string, object> names;
            Type nameType = assembly.GetType(ActiveNamespace + ".Name");
            if (nameType != null)
            {
                Type idClassType = assembly.GetType(ActiveNamespace + ".IDClass");
                PropertyInfo namePI = nameType.GetProperty("NameP");
                PropertyInfo nameIO = nameType.GetProperty("IdentifiedObject");
                PropertyInfo idPI = idClassType.GetProperty("ID");
                if (modelMap.ContainsKey(ActiveNamespace + ".Name"))
                {
                    names = modelMap[ActiveNamespace + ".Name"];
                    foreach (KeyValuePair<string, object> namePair in names)
                    {
                        object name = namePair.Value;
                        string nameValue = namePI.GetValue(name, null).ToString();
                        object referenced = nameIO.GetValue(name, null);
                        if (!string.IsNullOrWhiteSpace(nameValue) && referenced != null)
                        {
                            string refID = idPI.GetValue(referenced, null).ToString();
                            if (!string.IsNullOrWhiteSpace(refID))
                            {
								if (!idToNames.ContainsKey(refID))
								{
									idToNames.Add(refID, nameValue);
								}
                            }
                        }
                    }
                }
            }
            return idToNames;
        }

        private List<PropertyInfo> GetIsMandatoryProperties(Type type)
        {
            List<PropertyInfo> pList = new List<PropertyInfo>();
            List<PropertyInfo> pResultList = new List<PropertyInfo>();
            Type tempType = type;
            while (tempType.BaseType != null) 
            {
                foreach (PropertyInfo info in tempType.GetProperties()) 
                {
                    if (info.Name.StartsWith("Is") && info.Name.EndsWith("Mandatory") && string.Compare(info.Name,"IsIDMandatory")!=0) 
                    {
                        pList.Add(info);
                    }
                }
                tempType = tempType.BaseType;
            }


            foreach (PropertyInfo pInfo in pList)
            {
                if (pInfo.Name.StartsWith("Is") && pInfo.Name.EndsWith("Mandatory"))
                {
                    if ((bool)pInfo.GetValue(null, null))
                    {
                        pResultList.Add(pInfo);
                    }
                }
            }

            return pResultList;
        }

        private string GetName(Dictionary<string, string> idToNames, string id)
        {
            if (idToNames.ContainsKey(id))
            {
                return   "Tag ID: " + idToNames[id];
            }
            return string.Empty;
        }

        private List<PropertyInfo> GetHasValueProperties(Type type)
        {
            List<PropertyInfo> pList = new List<PropertyInfo>();
            //pList.AddRange(type.GetProperties());
            foreach (PropertyInfo pInfo in type.GetProperties())
            {
                if (pInfo.Name.EndsWith("HasValue"))
                {
                    pList.Add(pInfo);
                }
            }
            return pList;
        }

		#endregion

		#region Support Methods

        private void OnMessage(string message, MessageLevel level)
        {
            if (level == MessageLevel.WARNING)
            {
                result.WarrningCount++;
				result.Report.AppendLine("WARNING: " + message);
            }
            if (level == MessageLevel.ERROR)
            {
                result.ErrorCount++;
				result.Report.AppendLine("ERROR: " + message);
            }
			if (level == MessageLevel.INFO) 
			{
				result.Report.AppendLine(message);
			}
        }

		private void ConnectModelElements(CIMModel CIM_Model, Assembly assembly, ref ConcreteModel concreteModel)
		{
			foreach(string type in CIM_Model.ModelMap.Keys)
			{
				SortedDictionary<string, CIMObject> objects = CIM_Model.ModelMap[type];
				foreach(string objID in objects.Keys)
				{
					CIMObject element = objects[objID];
					Type classType;
					classType = assembly.GetType(ActiveNamespace + "." + type);
					if(classType == null)
					{
						OnMessage("Element (" + element.ID + ") not found in assembly:" + ActiveNamespace + "."
                            + type + "  - validation of document failed!", MessageLevel.ERROR);
						continue;
					}

					////aquire object from concrete model
					object currentObject = concreteModel.GetObjectByTypeAndID(classType, objID);
					if(currentObject != null)
					{
						foreach (int attKey in element.MyAttributes.Keys)
						{
							string propertyName = StringManipulationManager.ExtractShortestName(CIM_Model.ModelContext.ReadAttributeWithCode(attKey), StringManipulationManager.SeparatorDot);
							propertyName = StringManipulationManager.CreateHungarianNotation(propertyName);

							if(propertyName.Equals(type))
							{
								propertyName = propertyName + "P";
							}

							PropertyInfo prop = classType.GetProperty(propertyName);
							if(prop == null)
							{
                                OnMessage("Property " + propertyName + " not found in class "
                                    + element.CIMType + ", elements ID:" + element.ID + "  - validation of document failed!", MessageLevel.ERROR);
								continue;
							}
							////if it is a list or collection of references
							if(prop.PropertyType.IsGenericType)
							{
								Type propertyListType = prop.PropertyType.GetGenericArguments()[0];
								List<ObjectAttribute> attList = element.MyAttributes[attKey];
								////get the property as IList
								IList list = (IList)prop.GetValue(currentObject, null);
								foreach(ObjectAttribute att in attList)
								{
									////this part should add a reference to IList
									if(!IsSimpleValue(propertyListType) && !propertyListType.IsEnum)
									{
										AddReferenceToList(element,ref list, att.Value, prop, concreteModel);
									}
								}
							}
							else
							{
								List<ObjectAttribute> attList = element.MyAttributes[attKey];
								////if its not a list...
								ObjectAttribute att = attList.ElementAt(0);

								if(null != prop && prop.CanWrite)
								{
									if(!IsSimpleValue(prop.PropertyType) && !prop.PropertyType.IsEnum)
									{
										SetReferenceToProperty(element, currentObject, att.Value, prop, concreteModel);
									}
								}
							}
						}
						////embeded elements - lists
						if(element.GetEmbeddedChildren() != null)
						{
							foreach(string attKey in element.GetEmbeddedChildren().Keys)
							{
								////first is the name of property
								string propertyName = StringManipulationManager.ExtractShortestName(attKey, StringManipulationManager.SeparatorDot);
								propertyName = StringManipulationManager.CreateHungarianNotation(propertyName);

								if(propertyName.Equals(type))
								{
									propertyName = propertyName + "P";
								}
								PropertyInfo prop = classType.GetProperty(propertyName);
								if(prop != null && prop.PropertyType.IsGenericType)
								{
									Type propertyListType = prop.PropertyType.GetGenericArguments()[0];
									List<string> attList = element.GetEmbeddedChildren()[attKey];
									////get the property as IList
									IList list = (IList)prop.GetValue(currentObject, null);
									foreach(string att in attList)
									{
										////this part should add a reference to IList
										if(!IsSimpleValue(propertyListType) && !propertyListType.IsEnum)
										{
											AddReferenceToList(element,ref list, att, prop, concreteModel);
										}
									}
								}
								else
								{
									List<string> attList = element.GetEmbeddedChildren()[attKey];
									////if its not a list...
									string att = attList.ElementAt(0);

									if(prop != null && prop.CanWrite)
									{
										if(!IsSimpleValue(prop.PropertyType) && !prop.PropertyType.IsEnum)
										{
											SetReferenceToProperty(element, currentObject, att, prop, concreteModel);
										}
									}
								}
							}
						}
					}
					else
					{
						OnMessage("Object of class:" + classType + ", with ID:" + objID + " not found in model! Unable to create concrete model."
							, MessageLevel.ERROR);
					}
				}
			}
		}

		private void CheckMissingValues(CIMModel CIM_Model, Assembly assembly)
		{
			////FOREACH TYPE IN MODEL
			foreach(string type in CIM_Model.ModelMap.Keys)
			{
				SortedDictionary<string, CIMObject> objects = CIM_Model.ModelMap[type];
				////FOREACH OBJECT WITH ID THAT BELONGS TO TYPE
				foreach(string objID in objects.Keys)
				{
					////GET THE OBJECT
					CIMObject element = objects[objID];
					Type classType;
					////AQUIRE TYPE FROM ASSEMBLY
					classType = assembly.GetType(ActiveNamespace + "." + type);
					if(classType == null)
					{
						OnMessage("Element (" + element.ID + ") not found in assembly:" + ActiveNamespace + "." + type + "  - validation of document failed!"
							, MessageLevel.ERROR);
						continue;
					}
				}
			}
		}

		/// <summary>
		/// Method instantiates all classes from <c>CIM_Model</c> using class definitions from <c>assembly</c>
		/// All instances will have ID, and properties, that are not references, set and will be stored in model
		/// </summary>
		/// <param name="CIM_Model">model representing cim/xml</param>
		/// <param name="assembly">assembly that contains classes used in model</param>
		private void InstantiateClassesWithSimpleValues(CIMModel CIM_Model, Assembly assembly, ref ConcreteModel concreteModel)
		{
			////FOREACH TYPE IN MODEL
			foreach(string type in CIM_Model.ModelMap.Keys)
			{
                
				SortedDictionary<string, CIMObject> objects = CIM_Model.ModelMap[type];
				////FOREACH OBJECT WITH ID THAT BELONGS TO TYPE
				foreach(string objID in objects.Keys)
				{                    
					////GET THE OBJECT
					CIMObject element = objects[objID];
					Type classType;
					////AQUIRE TYPE FROM ASSEMBLY
					classType = assembly.GetType(ActiveNamespace + "." + type);
					if(classType == null)
					{
						OnMessage("Class for element (" + element.ID + ") not found in assembly:" + ActiveNamespace + "." + type + "  - validation of document failed!"
							, MessageLevel.ERROR);
						continue;
					}

					////WITH TYPE MAKE AN INSTANCE
					object instance = Activator.CreateInstance(classType);

					////AND SET ID TO THAT INSTANCE
					PropertyInfo propID = classType.GetProperty("ID");
					propID.SetValue(instance, objID, null);

					////SET SIMPLE VALUE ATTRIBUTES
					ProcessAttributes(assembly, element, classType, instance);

					////SAVE OBJECT IN MODEL
                    string insertEl = concreteModel.InsertObjectInModelMap(instance, classType);
                    if (!string.IsNullOrEmpty(insertEl)) 
                    {

                        OnMessage(string.Format("Inserting in model error on element ID {0} . Message: {1}", objID, insertEl),MessageLevel.WARNING);
                    }
				}
			}
		}

		/// <summary>
		/// Reads all the "simple" values (not references) from attribute list and sets properties to the 
		/// specified value 
		/// </summary>
		/// <param name="assembly">assembly that contains class definitions</param>
		/// <param name="element">element being processes</param>
		/// <param name="classType">type of the instance</param>
		/// <param name="instance">instance of classType that will have properties set</param>
		private void ProcessAttributes(Assembly assembly, CIMObject element, Type classType, object instance)
		{
			foreach(int attKey in element.MyAttributes.Keys)
			{
				////all properties have capital letters used for naming them
				string propertyName = StringManipulationManager.ExtractShortestName(element.ModelContext.ReadAttributeWithCode(attKey), StringManipulationManager.SeparatorDot);
				propertyName = StringManipulationManager.CreateHungarianNotation(propertyName);

				if(propertyName.Equals(element.CIMType))
				{
					propertyName = propertyName + "P";
				}
				PropertyInfo prop = classType.GetProperty(propertyName);
				if(prop == null)
				{
					OnMessage("Property " + propertyName + " not found in class "
                        + element.CIMType + " (element ID:" + element.ID + ")" + "  - validation of document failed!" 
						, MessageLevel.ERROR);
					continue;
				}
				////if it is a list or collection - though it always has to be a list
				if(prop.PropertyType.IsGenericType)
				{
					////gets the type of the items in list
					Type propertyListType = prop.PropertyType.GetGenericArguments()[0];
					////get all the values for this property
					List<ObjectAttribute> attList = element.MyAttributes[attKey];
					////get the property as IList
					IList list = (IList)prop.GetValue(instance, null);
					foreach(ObjectAttribute att in attList)
					{
						////Only add a simple value to IList, enumerations and references are not needed
						if(IsSimpleValue(propertyListType))
						{
							AddSimpleValueToList(element,ref list, att, propertyListType);
						}
					}
					//prop.SetValue(instance, list, null);
				}
				else
				{
					////if property is not a list...
					List<ObjectAttribute> attList = element.MyAttributes[attKey];
					////it only has one attribute value in list then
					if(attList.Count <= 1)
					{
						ObjectAttribute att = attList.ElementAt(0);

						if(null != prop)
						{
							if(IsSimpleValue(prop.PropertyType))
							{
								SetSimpleValue(element, instance, att, prop);
							}
							else
							{
								if(prop.PropertyType.IsEnum)
								{
									SetEnumerationProperty(element, assembly, instance, prop, att);
								}
								else
								{
									////if it was not found up until now it has to be reference or data type
									////if it is not empty and it is not any of the cases checked already it is DataType
									////make instance of dataType and set value
									if(!IsSimpleValue(prop.PropertyType) && !string.IsNullOrEmpty(att.Value))
									{
										SetDataTypeProperty(element, assembly, instance, prop, att);
									}
								}
							}
						}
					}
					else
					{
						OnMessage("Multiple values for attribute with multiplicity 1 on element with ID:" + element.ID + ". ATTRIBUTE: " + classType + "." + prop.Name
							, MessageLevel.WARNING);
					}
				}
			}
		}

		#endregion

		#region Property and Value/Reference manipulation methods

		private void SetReferenceToProperty(CIMObject element, object something, string attValue, PropertyInfo prop, ConcreteModel model)
		{
			if(!string.IsNullOrEmpty(attValue))
			{

				Type referencedType = prop.PropertyType;
				string referencedID = StringManipulationManager.ExtractAllAfterSeparator(attValue, StringManipulationManager.SeparatorSharp);


				object referencedObject = model.GetObjectByID(referencedID);
				if(referencedObject == null)
				{

					if(!(referencedType.GetProperty("Value") != null &&
						referencedType.GetProperty("Multiplier") != null &&
						referencedType.GetProperty("Unit") != null) && referencedType.Name!="AbsoluteDate")
					{



						OnMessage("Referenced object on property: " + prop.DeclaringType + "." + prop.Name + ", elements ID:" + element.ID + " not in model. Referenced: "
							+ referencedType.ToString() + ", ID:" + referencedID
							, MessageLevel.WARNING);
                        string propertyType = StringManipulationManager.ExtractAllAfterSeparator(referencedType.FullName, StringManipulationManager.SeparatorDot);
					}
					////otherwise its DataType and its already set
				}
				else
				{
                    if (referencedObject.GetType().Equals(referencedType) || referencedObject.GetType().IsSubclassOf(referencedType))
                    {                        
                        prop.SetValue(something, referencedObject, null);
                    }
                    else
                    {
                        string referencedObjectType = StringManipulationManager.ExtractAllAfterSeparator(referencedObject.GetType().FullName, StringManipulationManager.SeparatorDot);
                        string propertyType = StringManipulationManager.ExtractAllAfterSeparator(referencedType.FullName, StringManipulationManager.SeparatorDot);
                    }
				}
			}
		}

		/// <summary>
		/// Method adds reference to list on property prop
		/// </summary>
		/// <param name="list">list that reference will be added to</param>
		/// <param name="att">attribute containing information about reference</param>
		/// <param name="prop">property containing list</param>
		private void AddReferenceToList(CIMObject element,ref IList list, string attValue, PropertyInfo prop, ConcreteModel model)
		{
			if(!string.IsNullOrEmpty(attValue))
			{
				Type referencedType = prop.PropertyType;
				string referencedID = StringManipulationManager.ExtractAllAfterSeparator(attValue, StringManipulationManager.SeparatorSharp);
                
                string refType = referencedType.ToString();
                if (referencedType.IsGenericType) 
                {
                    refType = referencedType.GetGenericArguments()[0].ToString();
                }

                //DataTypes? what about them if in list - should be a way to determine 
				object referencedObject = model.GetObjectByID(referencedID);

				if(referencedObject != null)
				{
					try
					{
						list.Add(referencedObject);
					}
					catch
					{
                       
						OnMessage("Unsuccessful adding item to list on property: " + prop.DeclaringType + "." + prop.Name
							+ ", elements ID:" + element.ID + " Referenced: " + refType + ", ID: " + referencedID
							, MessageLevel.ERROR);
                        string referencedTypeWithoutPrefix = StringManipulationManager.ExtractAllAfterSeparator(refType, StringManipulationManager.SeparatorDot);
					}
				}
				else
				{
					OnMessage("Referenced object on property: " + prop.DeclaringType + "." + prop.Name + ", elements ID:" + element.ID + " not in model. Referenced: "
                            + refType + ", ID:" + referencedID
						, MessageLevel.WARNING);
                    string referencedTypeWithoutPrefix = StringManipulationManager.ExtractAllAfterSeparator(refType, StringManipulationManager.SeparatorDot);
				}
			}
		}

		/// <summary>
		/// Sets value of enumeration to property <c>prop</c> of object <c>instance</c>
		/// </summary>
		/// <param name="assembly">contains enumeration definition</param>
		/// <param name="instance">object that contains property</param>
		/// <param name="prop">property that value will be set to</param>
		/// <param name="att">attribute from model that cointains value and name of enumeration</param>
		private void SetEnumerationProperty(CIMObject element, Assembly assembly, object instance, PropertyInfo prop, ObjectAttribute att)
		{
			if(!string.IsNullOrEmpty(att.Value))
			{
				////get type, that is, enumeration member that has name equal to the string value of attribute
				////get the field with name casted into int and set itatt.Value
				Type enumType = assembly.GetType(prop.PropertyType.ToString());
				FieldInfo enumVal = enumType.GetField(StringManipulationManager.ReplaceInvalidEnumerationCharacters(att.Value));
				if(enumVal == null)
				{
					OnMessage("Error occured while trying to set value to field " +
						prop.DeclaringType.Name + "." +
						prop.Name + ", enumeration value <" +
						prop.PropertyType.ToString() + "." + StringManipulationManager.ReplaceInvalidEnumerationCharacters(att.Value) + "> not found in assembly!"
						+ " elements ID: " + element.ID, MessageLevel.WARNING);
				}
				else
				{
					object newEnumValue = enumVal.GetValue(enumType);
					prop.SetValue(instance, newEnumValue, null);
				}
			}
		}

		/// <summary>
		/// Sets value as data type, because in cim/xml model there is no data type as class
		/// only its value
		/// </summary>
		/// <param name="assembly">contains data type class</param>
		/// <param name="instance">object that needs property set</param>
		/// <param name="prop">property of instace that data type will be set to</param>
		/// <param name="att">attribute from model that contains value and name od data type</param>
		private void SetDataTypeProperty(CIMObject element, Assembly assembly, object instance, PropertyInfo prop, ObjectAttribute att)
		{
			if(!string.IsNullOrEmpty(att.Value))
			{
				////get Type from assembly
				Type dataType = assembly.GetType(prop.PropertyType.ToString());
				if(dataType != null)
				{
					////it should be only value field that can be set by value
					PropertyInfo dataTypePropertyInfo = dataType.GetProperty("Value");
					if(dataTypePropertyInfo != null)
					{
						object dataTypeInstance = Activator.CreateInstance(dataType);
						prop.SetValue(instance, dataTypeInstance, null);
						SetSimpleValue(element, dataTypeInstance, att, dataTypePropertyInfo);
					}
				}
				else
				{
					OnMessage("Error occured while trying to set value to field " +
						prop.DeclaringType.Name + "." +
						prop.Name + " DataType " +
						prop.PropertyType.ToString() + " not found in assembly! Elements ID: " + element.ID
						, MessageLevel.WARNING);
				}
			}
		}

		private void AddSimpleValueToList(CIMObject element,ref IList list, ObjectAttribute att, Type type)
		{
			if(!string.IsNullOrEmpty(att.Value))
			{
				////DATE AND TIME are specific - have to take care of them separately
				if(type.Equals(typeof(System.DateTime)))
				{
					try
					{
						////Temporary solution 24h => 23:59:59h so that we know this is 00-24h
						////this is because 24:00:00 is not by the standard but still may appear in rdf
						string value = att.Value;
						if(att.Value.Equals("24:00:00"))
						{
							value = "23:59:59";
						}
						DateTime timeVal = DateTime.Parse(value);
						list.Add(timeVal);
					}
					catch
					{
                        OnMessage("Invalid value format in CIM/XML of attribute (DateTime type) to be added to list "
                            + att.FullName + ", value: " + att.Value + " elements ID: " + element.ID
                            , MessageLevel.WARNING);
					}
				}////Everything else goes the same way
				else
				{
					try
					{
						list.Add(Convert.ChangeType(att.Value, type, new CultureInfo("en-US")));
					}
					catch
					{
						OnMessage("Invalid value format in CIM/XML of attribute to be added to list "
							+ att.FullName + ", value: " + att.Value + " elements ID: " + element.ID
							, MessageLevel.ERROR);
					}
				}
			}
		}

		private void SetSimpleValue(CIMObject element, object something, ObjectAttribute att, PropertyInfo prop)
		{
			if(!string.IsNullOrEmpty(att.Value))
			{

				////DATE AND TIME are specific - have to take care of them separately
				if(prop.PropertyType.Equals(typeof(System.DateTime)))
				{
					try
					{
						////Temporary solution 24h => 23:59:59h so that we know this is 00-24h
						////this is because 24:00:00 is not by the standard but still may appear in rdf
						string value = att.Value;
						if(att.Value.Equals("24:00:00"))
						{
							value = "23:59:59";
						}
						DateTime timeVal = DateTime.Parse(value);
						prop.SetValue(something, timeVal, null);
					}
					catch
					{
						OnMessage("Invalid format in CIM/XML of attribute (DateTime type)"
							+ att.FullName + ", value: " + att.Value + ", elements ID: " + element.ID
							, MessageLevel.WARNING);
					}
				}////Everything else goes the same way
				else
				{
					try
					{
                        prop.SetValue(something, Convert.ChangeType(att.Value, prop.PropertyType, new CultureInfo("en-US")), null);
					}
					catch
					{
						OnMessage("Invalid format in CIM/XML of attribute (simple value)" + att.FullName + " elements ID: " + element.ID, MessageLevel.WARNING);
					}
				}
			}
		}

		private bool IsSimpleValue(Type type)
		{
			if(type.Equals(typeof(System.DateTime)) || type.Equals(typeof(System.String)) || type.Equals(typeof(System.Int32)) || type.Equals(typeof(System.Single)) || type.Equals(typeof(System.Double)) || type.Equals(typeof(System.Boolean))
				|| type.Equals(typeof(System.DateTime?)) || type.Equals(typeof(System.Int32?)) || type.Equals(typeof(System.Single?)) || type.Equals(typeof(System.Double?)) || type.Equals(typeof(System.Boolean?))
				)
			{
				return true;
			}
			return false;
		}

		#endregion
	}
}
