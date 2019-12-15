using System;
using System.Collections.Generic;
using System.Text;
using CIM.Manager;

namespace CIM.Model
{    
    /// <summary>
    /// Class CIMModel represents an object view to the CIM data loaded from source file.
    /// <para>(see also <seealso cref="T:CIMDocumentHandler.cs"/>)</para>
    /// <para>It contains map of objects packed in submaps by type of objects, 
    /// and, in each submap, by value of ID property.</para>
    /// <para>Also, it contains two maps for representing connectivity model: </para>
    /// <para>they contains informations of how elements are connected to each other by connectivity nodes.</para>
    /// <remarks>
    /// <para>Special case of connectivity is relationship between PowerTransformer and it's TransformerWindings.</para>
    /// </remarks>
    /// <para>@author: Stanislava Selena</para>
    /// </summary>
    public class CIMModel
    {
        //        CIMType          id      object
        //HashMap<string, Hashmap<string, CIMObject>>
        private SortedDictionary<string, SortedDictionary<string, CIMObject>> modelMap;
		//// structure that keeps all found CIM types, attributes...
		private CIMModelContext modelContext = new CIMModelContext();

        // maps for representing connectivity model:
        //      pairs(connectivityNode_ID, List<ID_of_connected_element>)
        private Dictionary<string, List<string>> nodeToElementMap;
        //      pairs(element_ID, List<connectivityNode_ID>)
        private Dictionary<string, List<string>> elementToNodeMap;

        //// RDF attributes and namespaces
        private Dictionary<string, string> rdfNamespaces;
        private Dictionary<string, string> rdfAttributes;

        //// helper structures - voltage informations :
        // map contains identifiers of all PowerTransformers grouped by their voltage level (HV, HV/MV, LV)
        private Dictionary<CIMConstants.VoltageLevel, List<string>> voltageLevelMap;
        // list of all TransformerWindings (IDs) with volatge in MV range
        private List<string> mvTransformerWindings;
        // list of all TransformerWindings (IDs) with volatge in HV range
        private List<string> hvTransformerWindings;
        // map key is identifier of PowerTransformer and value is map-list of it's windings and their type
        private Dictionary<string, Dictionary<CIMObject, CIMConstants.WindingType>> analyzedPowerTransformers;

        private string sourcePath;
        private string fileName;
        private double fileSizeMB = 0;
        private DateTime? lastModificationTime;


        public CIMModel()
        {
            modelMap = new SortedDictionary<string, SortedDictionary<string, CIMObject>>();
            nodeToElementMap = new Dictionary<string, List<string>>();
            elementToNodeMap = new Dictionary<string, List<string>>();
            rdfAttributes = new Dictionary<string, string>();
            rdfNamespaces = new Dictionary<string, string>();
        }

        public CIMModel(string filePath)
        {
            modelMap = new SortedDictionary<string, SortedDictionary<string, CIMObject>>();
            nodeToElementMap = new Dictionary<string, List<string>>();
            elementToNodeMap = new Dictionary<string, List<string>>();
            rdfAttributes = new Dictionary<string, string>();
            rdfNamespaces = new Dictionary<string, string>();
            SourcePath = filePath;
        }


        /// <summary>
        /// Gets the CIM data in form of map of objects. 
        /// <para>Architecture of map: </para>
        /// <para>HashMap {<c>string</c> (object type), Hashmap {<c>string</c> (object ID), <c>CIMObject</c> (whole object)}}</para>
        /// </summary>
        public SortedDictionary<string, SortedDictionary<string, CIMObject>> ModelMap
        {
            get
            {
                return modelMap;
            }
        }
		
		/// <summary>
		/// Gets the CIM model context instance of this model.
		/// </summary>
		public CIMModelContext ModelContext
		{
			get
			{
				return modelContext;
			}
		}

        /// <summary>
        /// Gets and sets full file path to the source file of CIM document.
        /// </summary>
        public string SourcePath
        {
            set
            {
                sourcePath = value;
                fileName = string.Empty;
                fileSizeMB = 0;
                if (sourcePath != null)
                {
                    fileName = System.IO.Path.GetFileName(sourcePath);
                    lastModificationTime = FileManager.ReadLastModificationTimeForFile(sourcePath);
                    fileSizeMB = FileManager.ReadFileSizeInMBForFile(sourcePath);
                }
            }
            get
            {
                return sourcePath;
            }
        }

        /// <summary>
        /// Gets the file name of CIM data source file (with extension).
        /// </summary>
        public string FileName
        {
            get
            {
                return fileName;
            }
        }

        /// <summary>
        /// Gets the display name for this CIMModel. Display name is equals to FileName property,
        /// but it may have the "*" sufix if content of CIMModel was modified by user.
        /// </summary>
        public string DisplayName
        {
            get
            {
                string displayName = fileName;
                return displayName;
            }
        }

        /// <summary>
        /// Gets the size of CIM data source file (in MB).
        /// </summary>
        public double FileSizeMB
        {
            get
            {
                return fileSizeMB;
            }
        }

        /// <summary>
        /// Gets the current number of all CIMObjects in Model.
        /// </summary> 
        public int CountObjectsInModelMap
        {
            get
            {
                int size = 0;
                foreach (string type in modelMap.Keys)
                {
                    size += modelMap[type].Count;
                }
                return size;
            }
        }

        /// <summary>
        /// Gets one of the maps for representing connectivity model.
        /// <para>Architecture of map: </para>
        /// <para>pair{connectivityNode_ID, List{ID_of_connected_element}}</para>
        /// </summary>
        public Dictionary<string, List<string>> NodeToElementMap
        {
            get
            {
                return nodeToElementMap;
            }
        }

        /// <summary>
        /// Gets one of the maps for representing connectivity model.
        /// <para>Architecture of map: </para>
        /// <para>pair{element_ID, List{connectivityNode_ID}}</para>
        /// </summary>
        public Dictionary<string, List<string>> ElementToNodeMap
        {
            get
            {
                return elementToNodeMap;
            }
        }
                
        /// <summary>
        /// Gets the map with identifiers of all PowerTransformer CIMObjects 
        /// grouped by their voltage level (HV, HV/MV, LV).
        /// </summary>
        public Dictionary<CIMConstants.VoltageLevel, List<string>> VoltageLevelMap
        {
            get
            {
                return voltageLevelMap;
            }
        }

        /// <summary>
        /// Gets the indication of whether or not this CIMModel can be displayed as diagram.
        /// <para>If ModelMap doesn't contain any CIMObjects of CIMType "TransformerWinding", than this value is false.</para>
        /// <para>In special case, if there are no "TransformerWinding" objects, this value is true only if 
        /// there is at least one physical connection among model objects.</para>
        /// </summary>
        public bool IsModelDisplayable
        {
            get
            {
                bool isDisplayable = false;
                if (modelMap != null)
                {
                    if (modelMap.ContainsKey(CIMConstants.TypeNameTransformerWinding))
                    {
                        int count = 0;
                        if (modelMap[CIMConstants.TypeNameTransformerWinding] != null)
                        {
                            count = modelMap[CIMConstants.TypeNameTransformerWinding].Count;
                        }
                        if (count > 0)
                        {
                            isDisplayable = true;
                        }
                    }
                    else if (((NodeToElementMap != null) && (NodeToElementMap.Count > 0)) || ((ElementToNodeMap != null) && (ElementToNodeMap.Count > 0)))
                    {
                        isDisplayable = true;
                    }
                }                
                return isDisplayable;
            }
        }

        #region Simple methods for ModelMap

        /// <summary>
        /// Method <b>must</b> be called after all objects are added to ModelMap.
        /// <para>Method loads the nodeToElementMap and elementToNodeMap maps which represents connectivity in model.</para>
        /// <para>Method also process important informations about windings and transformers (voltage related info).</para>
        /// </summary>
        public void FinishModelMap()
        {
            nodeToElementMap = new Dictionary<string, List<string>>();
            elementToNodeMap = new Dictionary<string, List<string>>();

            //SortTransformerWindingsOfModel();
            ProcessConnectivityByNodes();
            ProcessConnectivityByPowerTransformers();   
        }

        /// <summary>
        /// Method returns the submap with elements of given type.
        /// <para>Submap is extracted from Model property.</para>
        /// </summary>
        /// <param fullName="type">type of IdentifedObject</param>
        /// <returns>map of CIMObjects of given type or null if given type doesn't exist as key in Model</returns>
        public SortedDictionary<string, CIMObject> GetAllObjectsOfType(string type)
        {
            SortedDictionary<string, CIMObject> mapAllObjectsOfType = null;
            if ((modelMap != null) && (!string.IsNullOrEmpty(type)) && modelMap.ContainsKey(type))
            {
               mapAllObjectsOfType = modelMap[type];
            }
            return mapAllObjectsOfType;
        }

        /// <summary>
        /// Method counts all elements of given type inside the ModelMap.        
        /// </summary>
        /// <param fullName="type">type of IdentifedObject</param>
        /// <returns>number of all elements of given type</returns>
        public int CountAllObjectsOfType(string type)
        {
            int count = 0;            
            if ((modelMap != null) && (!string.IsNullOrEmpty(type)) && modelMap.ContainsKey(type))
            {
                count = modelMap[type].Count;
            }
            return count;
        }

        /// <summary>
        /// Method searches and returns the CIMObject with given identificator.
        /// <para>If it doesn't find the CIMObject with given id, method returns null.</para>
        /// </summary>
        /// <param fullName="identifier">identifier (ID property value) of request CIMObject</param>
        /// <returns>request CIMObject or null</returns>
        public CIMObject GetObjectByID(string identifier)
        {
            CIMObject objFound = null;
            if ((modelMap != null) && (!string.IsNullOrEmpty(identifier)))
            {
                foreach (string type in modelMap.Keys)
                {
                    if (modelMap[type].ContainsKey(identifier))
                    {
                        objFound = (modelMap[type])[identifier];
                        break;
                    }
                }
            }
            return objFound;
        }

        /// <summary>
        /// Method searches and returns the CIMObject of given type and identificator.
        /// <para>If it doesn't find the CIMObject of given type with given id, method returns null.</para>
        /// </summary>
        /// <param fullName="type">type (CIMType property value) of request CIMObject</param>
        /// <param fullName="identifier">identifier (ID property value) of request CIMObject</param>
        /// <returns></returns>
        public CIMObject GetObjectByTypeAndID(string type, string identifier)
        {
            CIMObject objFound = null;
            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(identifier))
            {
                if ((modelMap != null) && modelMap.ContainsKey(type))
                {
                    if (modelMap[type].ContainsKey(identifier))
                    {
                        objFound = (modelMap[type])[identifier];
                    }
                }
            }
            return objFound;
        }
      
        /// <summary>
        /// Method inserts given CIMObject in Model.
        /// <para>Object will be inserted in submap acording to it's CIMType and ID property values. </para>
        /// </summary>
        /// <param fullName="cimObject">object for inserting (must not be null)</param>
		public void InsertObjectInModelMap(CIMObject cimObject)
		{
			if ((cimObject != null) && !string.IsNullOrEmpty(cimObject.ID) && !string.IsNullOrEmpty(cimObject.CIMType))
			{
				if (modelMap == null)
				{
					modelMap = new SortedDictionary<string, SortedDictionary<string, CIMObject>>();
				}

				if (!modelMap.ContainsKey(cimObject.CIMType))
				{
					SortedDictionary<string, CIMObject> typeSubmap = new SortedDictionary<string, CIMObject>();
					typeSubmap.Add(cimObject.ID, cimObject);
					modelMap.Add(cimObject.CIMType, typeSubmap);
				}
				else
				{
					if (modelMap[cimObject.CIMType] == null)
					{
						modelMap[cimObject.CIMType] = new SortedDictionary<string, CIMObject>();
					}
                    modelMap[cimObject.CIMType].Add(cimObject.ID, cimObject);                       
				}
			}
		}   
        #endregion Simple methods for ModelMap

        #region Get : parents

        /// <summary>
        /// Method loads and returns the map of all parents (objects refernced by it's attributes) of given object.
        /// <para>All parents are packed in submaps by their type.</para>
        /// </summary>
        /// <param fullName="cimObject"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, CIMObject>> GetParentObjectsOf(CIMObject cimObject)
        {
            Dictionary<string, Dictionary<string, CIMObject>> parents = new Dictionary<string, Dictionary<string, CIMObject>>();
            if (cimObject != null)
            {
                SortedDictionary<string, List<string>> parentsIdentifiers = cimObject.GetParents();
                if ((parentsIdentifiers != null) && (parentsIdentifiers.Count > 0))
                {
                    foreach (string type in parentsIdentifiers.Keys)
                    {
                        Dictionary<string, CIMObject> submap = new Dictionary<string, CIMObject>();
                        foreach (string id in parentsIdentifiers[type])
                        {
                            CIMObject parentObj = GetObjectByTypeAndID(type, id);
                            if (parentObj != null)
                            {
                                submap.Add(id, parentObj);
                            }
                        }
                        parents.Add(type, submap);
                    }
                }
            }
            return parents;
        }

        /// <summary>
        /// Method loads and returns the map of all parents (objects refernced by it's attributes) of given object.
        /// <para>All parents are NOT packed in submaps by their type - they are all in one map with ID value for key.</para>
        /// </summary>
        /// <param fullName="cimObject"></param>
        /// <returns></returns>
        public Dictionary<string, CIMObject> GetAllParentObjectsMapOf(CIMObject cimObject)
        {
            Dictionary<string, CIMObject> parents = new Dictionary<string, CIMObject>();
            if (cimObject != null)
            {
                SortedDictionary<string, List<string>> parentsIdentifiers = cimObject.GetParents();
                if ((parentsIdentifiers != null) && (parentsIdentifiers.Count > 0))
                {
                    foreach (string type in parentsIdentifiers.Keys)
                    {
                        foreach (string id in parentsIdentifiers[type])
                        {
                            CIMObject parentObj = GetObjectByTypeAndID(type, id);
                            if (parentObj != null)
                            {
                                parents.Add(id, parentObj);
                            }
                        }
                    }
                }
            }
            return parents;
        }

        public List<CIMObject> GetAllParentObjectsListOf(CIMObject cimObject)
        {
            List<CIMObject> parentObjects = new List<CIMObject>();
            if (cimObject != null)
            {
                SortedDictionary<string, List<string>> allParentsMap = cimObject.GetParents();
                foreach (string type in allParentsMap.Keys)
                {
                    List<string> parentsOfType = allParentsMap[type];
                    if ((parentsOfType != null) && (parentsOfType.Count > 0))
                    {
                        foreach (string parentId in parentsOfType)
                        {
                            CIMObject parentObject = GetObjectByTypeAndID(type, parentId);
                            if (parentObject != null)
                            {
                                parentObjects.Add(parentObject);
                            }
                        }
                    }
                }
            }
            return parentObjects;
        }
        #endregion Get : parents

        #region Get : children

        /// <summary>
        /// Method loads and returns the map of all children (objects who refernces given 
        /// object with their attributes) of given object.
        /// <para>All children are packed in submaps by their type.</para>
        /// </summary>
        /// <param fullName="cimObject"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, CIMObject>> GetChildObjectsOf(CIMObject cimObject)
        {
            Dictionary<string, Dictionary<string, CIMObject>> children = new Dictionary<string, Dictionary<string, CIMObject>>();
            if (cimObject != null)
            {
                SortedDictionary<string, List<string>> childrenIdentifiers = cimObject.GetChildren();
                if ((childrenIdentifiers != null) && (childrenIdentifiers.Count > 0))
                {
                    foreach (string type in childrenIdentifiers.Keys)
                    {
                        Dictionary<string, CIMObject> submap = new Dictionary<string, CIMObject>();
                        foreach (string id in childrenIdentifiers[type])
                        {
                            CIMObject childObj = GetObjectByTypeAndID(type, id);
                            if (childObj != null)
                            {
                                submap.Add(id, childObj);
                            }
                        }
                        children.Add(type, submap);
                    }
                }
            }
            return children;
        }

        /// <summary>
        /// Method loads and returns the map of all children (objects who refernces given 
        /// object with their attributes) of given object.
        /// <para>All children are NOT packed in submaps by their type - they are all in one map with ID value for key.</para>
        /// </summary>
        /// <param fullName="cimObject"></param>
        /// <returns></returns>
        public Dictionary<string, CIMObject> GetAllChildObjectsMapOf(CIMObject cimObject)
        {
            Dictionary<string, CIMObject> children = new Dictionary<string, CIMObject>();
            if (cimObject != null)
            {
                SortedDictionary<string, List<string>> childrenIdentifiers = cimObject.GetChildren();
                if ((childrenIdentifiers != null) && (childrenIdentifiers.Count > 0))
                {
                    foreach (string type in childrenIdentifiers.Keys)
                    {
                        foreach (string id in childrenIdentifiers[type])
                        {
                            CIMObject childObj = GetObjectByTypeAndID(type, id);
                            if (childObj != null)
                            {
                                children.Add(id, childObj);
                            }
                        }
                    }
                }
            }
            return children;
        }

        public List<CIMObject> GetAllChildObjectsListOf(CIMObject cimObject)
        {
            List<CIMObject> childObjects = new List<CIMObject>();
            if (cimObject != null)
            {
                SortedDictionary<string, List<string>> allChildrenMap = cimObject.GetChildren();
                foreach (string type in allChildrenMap.Keys)
                {
                    List<string> childrenOfType = allChildrenMap[type];
                    if ((childrenOfType != null) && (childrenOfType.Count > 0))
                    {
                        foreach (string childId in childrenOfType)
                        {
                            CIMObject childObject = GetObjectByTypeAndID(type, childId);
                            if (childObject != null)
                            {
                                childObjects.Add(childObject);
                            }
                        }
                    }
                }
            }
            return childObjects;
        }

        #endregion Get : children

        #region Get : RDF attributes / namespaces

        public Dictionary<string, string> GetRDFAttributes()
        {
            return rdfAttributes;
        }

        public void AddRDFAttribute(string rdfAttributeName, string rdfAttributeValue)
        {
            if (!string.IsNullOrEmpty(rdfAttributeName) && !string.IsNullOrEmpty(rdfAttributeValue))
            {
                if (!rdfAttributes.ContainsKey(rdfAttributeName))
                {
                    rdfAttributes.Add(rdfAttributeName, rdfAttributeValue);
                }
                else
                {
                    rdfAttributes[rdfAttributeName] = rdfAttributeValue;
                }
            }
        }

        public Dictionary<string, string> GetRDFNamespaces()
        {
            return rdfNamespaces;
        }

        public void AddRDFNamespace(string prefixAndNamespace, string uri)
        {
            if (!string.IsNullOrEmpty(prefixAndNamespace) && !string.IsNullOrEmpty(uri))
            {
                if (!rdfNamespaces.ContainsKey(prefixAndNamespace))
                {
                    rdfNamespaces.Add(prefixAndNamespace, uri);
                }
                else
                {
                    rdfNamespaces[prefixAndNamespace] = uri;
                }
            }
        }
        #endregion Get : RDF attributes / namespaces

        #region Get: Voltage related informations

        /// <summary>
        /// Method finds all objects of type TransformerWinding which are main Medium Voltage windings.
        /// </summary>
        /// <returns>list of main medium voltage windings</returns>
        public List<string> FindAllEnergySources()
        {
            List<string> energySources = new List<string>(); // main transformer windings
            List<string> processedIds = new List<string>(); // remember processed windings

            if ((modelMap != null) && (modelMap.ContainsKey(CIMConstants.TypeNameTransformerWinding)))
            {
                // get all TransformerWindings
                SortedDictionary<string, CIMObject> allTransformerWindings = null;
                modelMap.TryGetValue(CIMConstants.TypeNameTransformerWinding, out allTransformerWindings);

                if ((allTransformerWindings != null) && (allTransformerWindings.Count > 0))
                {
                    // extract all secondary windings as energy sources
                    foreach (CIMObject tWinding in allTransformerWindings.Values)
                    {
                        if (!processedIds.Contains(tWinding.ID))
                        {
                            processedIds.Add(tWinding.ID);
                            // try to read winding voltage
                            double voltage = ReadTransformerWindingVoltage(tWinding);

                            if ((voltage >= CIMConstants.VoltageRangeLowerKV) && (voltage <= CIMConstants.VoltageRangeUpperKV))
                            {
                                string type = ReadTransformerWindingType(tWinding);
                                bool isSecondary = false;

                                if (!string.IsNullOrEmpty(type))
                                {
                                    // if windingType attribute exists, check if it is secondary
                                    type = type.ToLower();
                                    if (type.Contains("secondary"))
                                    {
                                        isSecondary = true;
                                    }
                                }
                                else
                                {
                                    // else, rely on voltage check
                                    double ptVoltageValue;
                                    // check if this winding is secondary inside it's PowerTransformer
                                    List<string> pt = tWinding.GetParentsOfType(CIMConstants.TypeNamePowerTransformer);
                                    if ((pt != null) && (pt.Count > 0))
                                    {
                                        CIMObject ptObject = GetObjectByTypeAndID(CIMConstants.TypeNamePowerTransformer, pt[0]);
                                        if (ptObject != null)
                                        {
                                            Dictionary<CIMObject, CIMConstants.WindingType> analizedWindings = AnalyzeWindingsOfPowerTransformer(ptObject, out ptVoltageValue);
                                            if ((analizedWindings != null) && analizedWindings.ContainsKey(tWinding))
                                            {
                                                if ((analizedWindings[tWinding] == CIMConstants.WindingType.Secondary)
                                                    || ((analizedWindings[tWinding] == CIMConstants.WindingType.Other) && (analizedWindings.Count == 2)))
                                                {
                                                    isSecondary = true;
                                                    foreach (CIMObject analizedTW in analizedWindings.Keys)
                                                    {
                                                        if (!processedIds.Contains(analizedTW.ID))
                                                        {
                                                            processedIds.Add(analizedTW.ID);
                                                        }
                                                    }
                                                }                                              
                                            }                                            
                                        }
                                    }
                                }

                                if (isSecondary && (!energySources.Contains(tWinding.ID)))
                                {
                                    energySources.Add(tWinding.ID);
                                }
                            }
                        }                        
                    }                    
                }

            }
            return energySources;
        }

        /// <summary>
        /// Method tries to read the voltage value of given TransformerWinding object.
        /// <para>Value is searched in: ratedKV, ratedU attribute or from reference to a BaseVoltage.</para>
        /// <para>If none of this informations couldn't be found, the returned voltage value will be -1.</para>
        /// </summary>
        /// <param name="transformerWinding">CIMObject of TransformerWinding type</param>
        /// <returns>read voltage value (in kV) or -1 if voltage information couldn't be found</returns>
        public double ReadTransformerWindingVoltage(CIMObject transformerWinding)
        {
            double voltage = -1;

            double voltageFromBaseVoltage = ReadTransformerWindingBaseVoltageNominalVoltage(transformerWinding);
            double voltageKV = ReadTransformerWindingVoltageKV(transformerWinding);
            double voltageU = ReadTransformerWindingVoltageU(transformerWinding);            
       
            if ((voltageU > -1) && (voltageFromBaseVoltage > -1) 
                && (double.Equals(voltageU, voltageFromBaseVoltage) || (Math.Abs(voltageU - voltageFromBaseVoltage) < 1)))
            {
                voltageU *= 1000;
            }
           
            if (voltageKV > -1)
            {
                voltage = voltageKV;
            }
            else if (voltageU > -1)
            {
                voltage = voltageU;
            }    
            else if (voltageFromBaseVoltage > -1)
            {
                voltage = voltageFromBaseVoltage;
            }

            return voltage;
        }

        /// <summary>
        /// Method uses the VoltageLevelMap property and returns the previously determined VoltageLevel 
        /// of given PowerTransformer CIM object.
        /// <para>This information is determined during process of analyzing transformer windings.</para>
        /// </summary>
        /// <param name="powerTransformer"></param>
        /// <returns>voltage level of power transformer</returns>
        public CIMConstants.VoltageLevel GetVoltageLevelOfPowerTransformer(CIMObject powerTransformer)
        {
            CIMConstants.VoltageLevel voltageLevel = CIMConstants.VoltageLevel.None;
            if ((powerTransformer != null) && (string.Compare(CIMConstants.TypeNamePowerTransformer, powerTransformer.CIMType) == 0))
            {
                if (voltageLevelMap != null)
                {
                    foreach (CIMConstants.VoltageLevel voltageL in voltageLevelMap.Keys)
                    {
                        List<string> ptIdentifiers = null;
                        voltageLevelMap.TryGetValue(voltageL, out ptIdentifiers);
                        if ((ptIdentifiers != null) && ptIdentifiers.Contains(powerTransformer.ID))
                        {
                            voltageLevel = voltageL;
                            break;
                        }
                    }
                }
            }
            return voltageLevel;
        }

        /// <summary>
        /// Method returns true if given CIMObject is of TransformerWindining type and has defined
        /// voltage value in the HV range.
        /// </summary>
        /// <param name="transformerWinding"></param>
        /// <returns>true if transformer winding has been determined as HV</returns>
        public bool IsTransformerWindingHV(CIMObject transformerWinding)
        {
            bool isHV = false;
            if ((transformerWinding != null) && (string.Compare(CIMConstants.TypeNameTransformerWinding, transformerWinding.CIMType) == 0))
            {
                if (hvTransformerWindings != null)
                {
                    if (hvTransformerWindings.Contains(transformerWinding.ID))
                    {
                        isHV = true;
                    }
                }
            }
            return isHV;
        }

        /// <summary>
        /// Method returns true if given CIMObject is of TransformerWindining type and has defined
        /// voltage value in the MV range.
        /// </summary>
        /// <param name="transformerWinding"></param>
        /// <returns>true if transformer winding has been determined as MV</returns>
        public bool IsTransformerWindingMV(CIMObject transformerWinding)
        {
            bool isMV = false;
            if ((transformerWinding != null) && (string.Compare(CIMConstants.TypeNameTransformerWinding, transformerWinding.CIMType) == 0))
            {
                if (mvTransformerWindings != null)
                {
                    if (mvTransformerWindings.Contains(transformerWinding.ID))
                    {
                        isMV = true;
                    }
                }
            }
            return isMV;
        }

        /// <summary>
        /// Method goes throught all TransformerWinding objects of given PowerTransformer object
        /// and determines type of each winding.
        /// </summary>
        /// <param name="powerTransformerObject"></param>
        /// <param name="voltageOnSecondar">voltage(kV) on secondary winding or -1 if that value can not be read or secondary winding is not explicitly found</param>
        /// <returns></returns>
        public Dictionary<CIMObject, CIMConstants.WindingType> AnalyzeWindingsOfPowerTransformer(CIMObject powerTransformerObject, out double voltageOnSecondar)
        {
            Dictionary<CIMObject, CIMConstants.WindingType> analyzed = null;
            voltageOnSecondar = -1;

            if ((powerTransformerObject != null) && (string.Compare(CIMConstants.TypeNamePowerTransformer, powerTransformerObject.CIMType) == 0))
            {
                if (analyzedPowerTransformers == null)
                {
                    analyzedPowerTransformers = new Dictionary<string, Dictionary<CIMObject, CIMConstants.WindingType>>();
                }

                if (analyzedPowerTransformers.ContainsKey(powerTransformerObject.ID))
                {
                    analyzedPowerTransformers.TryGetValue(powerTransformerObject.ID, out analyzed);
                    if (analyzed != null)
                    {
                        if (analyzed.ContainsValue(CIMConstants.WindingType.Secondary))
                        {
                            foreach (CIMObject tw in analyzed.Keys)
                            {
                                if (analyzed[tw] == CIMConstants.WindingType.Secondary)
                                {
                                    voltageOnSecondar = ReadTransformerWindingVoltage(tw);
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    analyzed = new Dictionary<CIMObject, CIMConstants.WindingType>();                    

                    List<string> windingIds = powerTransformerObject.GetChildrenOfType(CIMConstants.TypeNameTransformerWinding);
                    windingIds.Sort();
                    if (windingIds != null)
                    {
                        List<CIMObject> windings = new List<CIMObject>();
                        double maxVoltage = -1;
                        string primary = string.Empty;
                        foreach (string windingId in windingIds)
                        {
                            CIMObject winding = GetObjectByTypeAndID(CIMConstants.TypeNameTransformerWinding, windingId);
                            if (winding != null)
                            {
                                double myVoltage = ReadTransformerWindingVoltage(winding);
                                if (myVoltage > maxVoltage)
                                {
                                    maxVoltage = myVoltage;
                                    primary = winding.ID;
                                }
                                windings.Add(winding);

                                if (myVoltage != -1)
                                {
                                    if ((myVoltage > CIMConstants.VoltageRangeUpperKV) && !hvTransformerWindings.Contains(winding.ID))
                                    {
                                        hvTransformerWindings.Add(winding.ID);
                                    }
                                    else if ((myVoltage >= CIMConstants.VoltageRangeLowerKV) && (myVoltage <= CIMConstants.VoltageRangeUpperKV) && !mvTransformerWindings.Contains(winding.ID))
                                    {
                                        mvTransformerWindings.Add(winding.ID);
                                    }
                                }
                            }
                        }

                        foreach (CIMObject winding in windings)
                        {
                            CIMConstants.WindingType windingType = CIMConstants.WindingType.Other;
                            string type = ReadTransformerWindingType(winding);
                            if (string.IsNullOrEmpty(type))
                            {
                                if (string.Compare(primary, winding.ID) == 0)
                                {
                                    windingType = CIMConstants.WindingType.Primary;
                                }
                            }
                            else
                            {
                                type = type.ToLower();
                                if (type.Contains(CIMConstants.TransformerWindingTypePrimary))
                                {
                                    windingType = CIMConstants.WindingType.Primary;
                                }
                                else if (type.Contains(CIMConstants.TransformerWindingTypeSecondary))
                                {
                                    windingType = CIMConstants.WindingType.Secondary;
                                    voltageOnSecondar = ReadTransformerWindingVoltage(winding);
                                }
                                else if (type.Contains(CIMConstants.TransformerWindingTypeTertiary))
                                {
                                    windingType = CIMConstants.WindingType.Tertiary;
                                }
                                else if (type.Contains(CIMConstants.TransformerWindingTypeQuaternary))
                                {
                                    windingType = CIMConstants.WindingType.Quaternary;
                                }
                            }

                            analyzed.Add(winding, windingType);
                        }
                    }

                    analyzedPowerTransformers.Add(powerTransformerObject.ID, analyzed);
                }
            }        
            return analyzed;
        }
        #endregion Get: Voltage related informations

        #region Get : Connectivity informations

        /// <summary>
        /// Method returns all direct connections between object with
        /// given identifier and other CIM objects.
        /// <para>This information is retrieved from the NodeToElementMap and ElementToNodeMap properties.</para>
        /// </summary>
        /// <param name="cimObjectId">CIM object's identifier</param>
        /// <returns>list of directly connected objets</returns>
        public List<string> GetAllDirectConnectionsOfObject(string cimObjectId)
        {
            List<string> directConnections = null;            
            if (!string.IsNullOrEmpty(cimObjectId))
            {
                List<string> connections = null;
                if (nodeToElementMap != null)
                {
                    nodeToElementMap.TryGetValue(cimObjectId, out connections);
                    if (connections != null)
                    {
                        if (directConnections == null)
                        {
                            directConnections = new List<string>();
                        }
                        directConnections.AddRange(connections);
                    }
                }
                if (elementToNodeMap != null)
                {
                    connections = null;
                    elementToNodeMap.TryGetValue(cimObjectId, out connections);
                    if (connections != null)
                    {
                        if (directConnections == null)
                        {
                            directConnections = new List<string>();
                        }
                        directConnections.AddRange(connections);
                    }
                }
            }
            return directConnections;
        } 

        /// <summary>
        /// Method returns number of direct connections between object with
        /// given identifier and other CIM objects.
        /// <para>This information is retrieved from the NodeToElementMap and ElementToNodeMap properties.</para>
        /// </summary>
        /// <param name="cimObjectId">CIM object's identifier</param>
        /// <returns>number of direct connections</returns>
        public int GetNumberOfDirectConnectionOfObject(string cimObjectId)
        {
            int directConnections = 0;
            if (!string.IsNullOrEmpty(cimObjectId))
            {
                List<string> connections = null;   
                if (nodeToElementMap != null)
                {                                 
                    nodeToElementMap.TryGetValue(cimObjectId, out connections);
                    if (connections != null)
                    {
                        directConnections += connections.Count;
                    }
                }
                if (elementToNodeMap != null)
                {
                    connections = null;
                    elementToNodeMap.TryGetValue(cimObjectId, out connections);
                    if (connections != null)
                    {
                        directConnections += connections.Count;
                    }
                }
            }
            return directConnections;
        }
        #endregion Get : Connectivity informations

        /// <summary>
        /// Method retrieves the CIM/RDF text snippet with definition of given CIMObject
        /// or empty string if such information is not available.
        /// </summary>
        /// <param name="cimObject"></param>
        /// <returns>CIM/RDF snippet with object definition</returns>
        public string ReadDefinitionSnippetFromSourceFile(CIMObject cimObject)
        {
            string sourceSnippet = string.Empty;
            if (cimObject != null)
            {
				sourceSnippet = CIM.Manager.FileManager.ReadTextSegmentFromFile(SourcePath, cimObject.SourceStartLine, (int)cimObject.SourceStartColumn, cimObject.SourceEndLine, (int)cimObject.SourceEndColumn);
            }
            return sourceSnippet;
        }

        /// <summary>
        /// Method should be called when current CIM profile for this CIM model is changed
        /// and before new validation process.
        /// <para>Method resets validation informations for every CIMObject of this model.</para>
        /// </summary>
        public void CleanValidationMessagesForEntireModelElements()
        {
			if (ModelMap != null)
			{
				foreach (SortedDictionary<string, CIMObject> submap in modelMap.Values)
				{
					foreach (CIMObject modelObject in submap.Values)
					{
						if (modelObject != null)
						{
							modelObject.ClearValidationInformations();
							if (modelObject.MyAttributes != null)
							{
								foreach (int attributeCode in modelObject.MyAttributes.Keys)
								{
									foreach (ObjectAttribute attribute in modelObject.MyAttributes[attributeCode])
									{
										attribute.ClearValidationInformations();
									}
								}
							}
						}
					}
				}
			}
        }

        /// <summary>
        /// Method returns string with short description of Model (element count for each type..)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        { 
            StringBuilder messageBuilder = new StringBuilder();
            int count = 0;
            messageBuilder.Append("Diferent element types count : ");
            messageBuilder.AppendLine(modelMap.Count.ToString());

            foreach (string type in modelMap.Keys)
            {
                messageBuilder.AppendLine();
                messageBuilder.Append("Type ");
                messageBuilder.Append(type);
                messageBuilder.Append(" count : ");
                messageBuilder.AppendLine(modelMap[type].Count.ToString());
                messageBuilder.AppendLine();
                count += modelMap[type].Count;
            }
            messageBuilder.AppendLine();
            messageBuilder.Append("----------------------- summary count = ");
            messageBuilder.AppendLine(count.ToString());
            return messageBuilder.ToString();
        }


        #region Private: Helper methods
        // Helper method
        private void ProcessConnectivityByNodes()
        {
            SortedDictionary<string, CIMObject> allTerminals = GetAllObjectsOfType(CIMConstants.TypeNameTerminal);
            if (allTerminals != null)
            {
                foreach (string terminalId in allTerminals.Keys)
                {
                    CIMObject terminal = allTerminals[terminalId];

                    if (terminal != null)
                    {
                        SortedDictionary<string, List<string>> parentsOfTerminal = terminal.GetParents();
                        if (parentsOfTerminal != null)
                        {
                            string connectivityNodeId = null;
                            string elementId = null;
                            foreach (string parentType in parentsOfTerminal.Keys)
                            {
                                List<string> parents = parentsOfTerminal[parentType];
                                if ((parents != null) && (parents.Count > 0))
                                {
                                    if (parentType == CIMConstants.TypeNameConnectivityNode)
                                    {
                                        connectivityNodeId = parents[0];
                                    }
                                    else
                                    {
                                        elementId = parents[0];
                                    }
                                }
                            }

                            if ((connectivityNodeId != null) && (elementId != null))
                            {
                                AddToNodeToElementMap(connectivityNodeId, elementId);
                                AddToElementToNodeMap(elementId, connectivityNodeId);
                            }
                        }
                    }
                }
            }
        }

        // Helper method
        private void ProcessConnectivityByPowerTransformers()
        {
            SortedDictionary<string, CIMObject> allTransformerWindings = GetAllObjectsOfType(CIMConstants.TypeNameTransformerWinding);
            if (allTransformerWindings != null)
            {
                foreach (string transfWindingId in allTransformerWindings.Keys)
                {
                    CIMObject transfWinding = allTransformerWindings[transfWindingId];
                    if (transfWinding != null)
                    {
                        List<string> transformers = transfWinding.GetParentsOfType(CIMConstants.TypeNamePowerTransformer);
                        if (transformers != null)
                        {
                            foreach (string transformerId in transformers)
                            {
                                AddToNodeToElementMap(transformerId, transfWindingId);
                                AddToElementToNodeMap(transfWindingId, transformerId);
                            }
                        }
                    }
                }
            }
        }

        // Helper method
		private void AddToNodeToElementMap(string connectivityNodeId, string elementId)
		{
			if (nodeToElementMap == null)
			{
				nodeToElementMap = new Dictionary<string, List<string>>();
			}

			List<string> nToE = new List<string>();
			nodeToElementMap.TryGetValue(connectivityNodeId, out nToE);
			if (nToE == null)
			{
				nToE = new List<string>();
				nToE.Add(elementId);
				nodeToElementMap.Add(connectivityNodeId, nToE);
			}
			else
			{
				nodeToElementMap[connectivityNodeId].Add(elementId);
			}
		}

        // Helper method
		private void AddToElementToNodeMap(string elementId, string connectivityNodeId)
		{
			if (elementToNodeMap == null)
			{
				elementToNodeMap = new Dictionary<string, List<string>>();
			}

			List<string> eToN = null;
			elementToNodeMap.TryGetValue(elementId, out eToN);
			if (eToN == null)
			{
				eToN = new List<string>();
				eToN.Add(connectivityNodeId);
				elementToNodeMap.Add(elementId, eToN);
			}
			else
			{
				elementToNodeMap[elementId].Add(connectivityNodeId);
			}
		}
        #endregion Private: Helper methods

        #region Private: TransformerWinding & PowerTransformer Related

        private void SortTransformerWindingsOfModel()
        {
            if (modelMap != null)
            {
                voltageLevelMap = new Dictionary<CIMConstants.VoltageLevel, List<string>>();
                mvTransformerWindings = new List<string>();
                hvTransformerWindings = new List<string>();

                SortedDictionary<string, CIMObject> allPowerTransformers = GetAllObjectsOfType(CIMConstants.TypeNamePowerTransformer);
                if (allPowerTransformers != null)
                {
                    double ptVoltageValue;
                    foreach (CIMObject pt in allPowerTransformers.Values)
                    {
                        Dictionary<CIMObject, CIMConstants.WindingType> analized = AnalyzeWindingsOfPowerTransformer(pt, out ptVoltageValue);
                        if (analized.Count > 0)
                        {
                            SortedDictionary<int, string> orderedMap = new SortedDictionary<int, string>();
                            int count = 0;
                            foreach (CIMObject winding in analized.Keys)
                            {
                                count++;
                                switch (analized[winding])
                                {
                                    case CIMConstants.WindingType.Primary:
                                    {
                                        orderedMap.Add(1, winding.ID);
                                        break;
                                    }
                                    case CIMConstants.WindingType.Secondary:
                                    {
                                        orderedMap.Add(2, winding.ID);                                        
                                        break;
                                    }
                                    case CIMConstants.WindingType.Tertiary:
                                    {
                                        orderedMap.Add(3, winding.ID);
                                        break;
                                    }
                                    case CIMConstants.WindingType.Quaternary:
                                    {
                                        orderedMap.Add(4, winding.ID);
                                        break;
                                    }
                                    default:
                                    {
                                        orderedMap.Add(4 + count, winding.ID);
                                        break;
                                    }
                                }
                                if ((count == 2) && (ptVoltageValue == -1))
                                {
                                    ptVoltageValue = ReadTransformerWindingVoltage(winding);
                                }
                            }

                            List<string> orderedWindingChildren = new List<string>();
                            foreach (int i in orderedMap.Keys)
                            {
                                orderedWindingChildren.Add(orderedMap[i]);
                            }
                            pt.SetChildrenOfType(CIMConstants.TypeNameTransformerWinding, orderedWindingChildren);
                        }

                        AddPowerTransformerToVoltageLevelMap(pt.ID, ptVoltageValue);
                    }
                }
            }
        }

        private void AddPowerTransformerToVoltageLevelMap(string powerTransformerId, double voltageValue)
        {
            if (!string.IsNullOrEmpty(powerTransformerId))
            {
                CIMConstants.VoltageLevel level = CIMConstants.VoltageLevel.None;
                if (voltageValue != -1)
                {                    
                    if (voltageValue > CIMConstants.VoltageRangeUpperKV)
                    {
                        level = CIMConstants.VoltageLevel.HV;
                    }
                    else if ((voltageValue >= CIMConstants.VoltageRangeLowerKV) && (voltageValue <= CIMConstants.VoltageRangeUpperKV))
                    {
                        level = CIMConstants.VoltageLevel.HVMV;
                    }
                    else if (voltageValue < CIMConstants.VoltageRangeLowerKV)
                    {
                        level = CIMConstants.VoltageLevel.LV;
                    }
                }

                if (voltageLevelMap.ContainsKey(level))
                {
                    voltageLevelMap[level].Add(powerTransformerId);
                }
                else
                {
                    List<string> vLevelList = new List<string>();
                    vLevelList.Add(powerTransformerId);
                    voltageLevelMap.Add(level, vLevelList);
                }
            }
        }

        private double ReadTransformerWindingVoltageKV(CIMObject transformerWinding)
        {
            double voltage = -1;
            if ((transformerWinding != null) && (transformerWinding.CIMType.Equals(CIMConstants.TypeNameTransformerWinding)))
            {
                List<ObjectAttribute> voltageAttributes = transformerWinding.FindAttributeByShortName(CIMConstants.AttributeNameTransformerWindingVoltageKV);

                // try to read ratedKV attribute
                if ((voltageAttributes != null) && (voltageAttributes.Count > 0))
                {
                    ObjectAttribute voltageAttribute = null;
                    if (voltageAttributes.Count > 1)
                    {
                        foreach (ObjectAttribute att in voltageAttributes)
                        {
                            if (att.FullName.StartsWith(CIMConstants.TypeNameTransformerWinding))
                            {
                                voltageAttribute = att;
                                break;
                            }
                        }
                    }
                    if (voltageAttribute == null)
                    {
                        voltageAttribute = voltageAttributes[0];
                    }

                    try
                    {
                        // read value
                        voltage = Double.Parse(voltageAttribute.Value);
                    }
                    catch (Exception)
                    {
                        voltage = -1;
                    }
                }
            }
            return voltage;
        }

        private double ReadTransformerWindingVoltageU(CIMObject transformerWinding)
        {
            double voltage = -1;
            if ((transformerWinding != null) && (transformerWinding.CIMType.Equals(CIMConstants.TypeNameTransformerWinding)))
            {
                List<ObjectAttribute> voltageAttributes = transformerWinding.FindAttributeByShortName(CIMConstants.AttributeNameTransformerWindingVoltageU);

                // try to read ratedU attribute
                if ((voltageAttributes != null) && (voltageAttributes.Count > 0))
                {
                    ObjectAttribute voltageAttribute = null;
                    if (voltageAttributes.Count > 1)
                    {
                        foreach (ObjectAttribute att in voltageAttributes)
                        {
                            if (att.FullName.StartsWith(CIMConstants.TypeNameTransformerWinding))
                            {
                                voltageAttribute = att;
                                break;
                            }
                        }
                    }
                    if (voltageAttribute == null)
                    {
                        voltageAttribute = voltageAttributes[0];
                    }

                    try
                    {
                        // read value
                        voltage = Double.Parse(voltageAttribute.Value);
                    }
                    catch (Exception)
                    {
                        voltage = -1;
                    }
                }
            }
            if (voltage > -1)
            {
                voltage = voltage / 1000.0;
            }
            return voltage;
        }

        private string ReadTransformerWindingType(CIMObject transformerWinding)
        {
            string type = null;
            if ((transformerWinding != null) && (transformerWinding.CIMType.Equals(CIMConstants.TypeNameTransformerWinding)))
            {
                List<ObjectAttribute> typeAttributes = transformerWinding.FindAttributeByShortName(CIMConstants.AttributeNameTransformerWindingType);

                // try to read windingType attribute
                if ((typeAttributes != null) && (typeAttributes.Count > 0))
                {
                    ObjectAttribute typeAttribute = null;
                    if (typeAttributes.Count > 1)
                    {
                        foreach (ObjectAttribute att in typeAttributes)
                        {
                            if (att.FullName.StartsWith(CIMConstants.TypeNameTransformerWinding))
                            {
                                typeAttribute = att;
                                break;
                            }
                        }
                    }
                    if (typeAttribute == null)
                    {
                        typeAttribute = typeAttributes[0];
                    }

                    if (typeAttribute.IsReference)
                    {
						type = CIM.Manager.StringManipulationManager.ExtractShortestName(typeAttribute.ValueOfReference, CIM.Manager.StringManipulationManager.SeparatorSharp);
                    }
                    else
                    {
                        type = typeAttribute.Value;
                    }

                }
            }

            return type;
        }

        private double ReadTransformerWindingBaseVoltageNominalVoltage(CIMObject transformerWinding)
        {
            double voltage = -1;
            if ((transformerWinding != null) && (transformerWinding.CIMType.Equals(CIMConstants.TypeNameTransformerWinding)))
            {
                List<string> baseVoltages = transformerWinding.GetParentsOfType(CIMConstants.TypeNameBaseVoltage);
                if ((baseVoltages != null) && (baseVoltages.Count > 0))
                {
                    CIMObject baseVoltage = GetObjectByTypeAndID(CIMConstants.TypeNameBaseVoltage, baseVoltages[0]);
                    if (baseVoltage != null)
                    {
                        List<ObjectAttribute> voltageAttributes = baseVoltage.FindAttributeByShortName(CIMConstants.AttributeNameBaseVoltageNominalVoltage);

                        // try to read nominalVoltage attribute
                        if ((voltageAttributes != null) && (voltageAttributes.Count > 0))
                        {
                            ObjectAttribute voltageAttribute = null;
                            if (voltageAttributes.Count > 1)
                            {
                                foreach (ObjectAttribute att in voltageAttributes)
                                {
                                    if (att.FullName.StartsWith(CIMConstants.TypeNameBaseVoltage))
                                    {
                                        voltageAttribute = att;
                                        break;
                                    }
                                }
                            }
                            if (voltageAttribute == null)
                            {
                                voltageAttribute = voltageAttributes[0];
                            }

                            try
                            {
                                // read value
                                voltage = Double.Parse(voltageAttribute.Value);
                            }
                            catch (Exception)
                            {
                                voltage = -1;
                            }
                        }
                    }
                }                
            }

            if (voltage > -1)
            {
                voltage = voltage / 1000.0;
            }
            return voltage;
        }

        #endregion Private: TransformerWinding & PowerTransformer Related

    }
}
