using System;
using System.Collections.Generic;
using System.Linq;
using FTN.ESI.SIMES.CIM.Manager;
using FTN.ESI.SIMES.CIM.Model;
using FTN.ESI.SIMES.CIM.Model.Tools;

namespace FTN.ESI.SIMES.CIM.Parser.Handler
{
    class RDFSXMLReaderHandler : IHandler
    {
        #region CONST
        private const string documentError = "Processing aborted: document doesn't have a CIM-RDFS structure!";

        private const string rdfProfileElement = "rdf:Description";
        private const string rdfPropertyElement = "rdf:Property";
        private const string rdfId = "rdf:ID";
        private const string rdfType = "rdf:type";
        private const string rdfAbout = "rdf:about";
        private const string rdfResource = "rdf:resource";

        private const string rdfsNamespace = "rdfs:";
        private const string rdfsClassElement = "rdfs:Class";
        private const string rdfsLabel = "rdfs:label";     // text
        private const string rdfsComment = "rdfs:comment"; // text
        private const string rdfsRange = "rdfs:range";
        private const string rdfsDomain = "rdfs:domain";
        private const string rdfsSubClassOf = "rdfs:subClassOf";

        private const string cimsNamespace = "cims:";
        private const string cimsClassCategoryElement = "cims:ClassCategory";
        private const string cimsStereotype = "cims:stereotype";
        private const string cimsBelongsToCategory = "cims:belongsToCategory";
        private const string cimsDataType = "cims:dataType";
        private const string cimsInverseRoleName = "cims:inverseRoleName";
        private const string cimsMultiplicity = "cims:multiplicity";
        private const string cimsIsAggregate = "cims:isAggregate"; // text

        private const string xmlBase = "xml:base";

        private const string separator = StringManipulationManager.SeparatorSharp;
		#endregion CONST

		#region FIELDS
		private string content = string.Empty; //// text content of element
        private Profile profile;
        private SortedDictionary<ProfileElementTypes, List<ProfileElement>> allByType;
        //// helper map:  parent class uri,   properties
        ////   ie.        package uri,      classes
        private Dictionary<string, Stack<ProfileElement>> belongingMap;
		private static SortedList<string, string> details = new SortedList<string, string>();
        
        //// for checking if document can't be processed as CIM-RDFS
        private bool documentIdentifiedLikeRDFS = false;
        private int checkedElementsCount = 0;
        private bool abort = false;
		#endregion FIELDS

		/// <summary>
        /// Gets the Profile object which is the finall product of parsing RDFS document.
        /// </summary>
        public Profile Profile
        {
            get
            {
                return profile;
            }
        }
		

		#region IHandler Members
		public void StartDocument(string filePath)
        {
            profile = new Profile();
            profile.SourcePath = filePath;
            allByType = new SortedDictionary<ProfileElementTypes, List<ProfileElement>>();

            checkedElementsCount = 0;
            documentIdentifiedLikeRDFS = false;
            abort = false;
        }

        public void StartElement(string localName, string qName, SortedList<string, string> atts)
        {
            if (!abort)
            {
                if (atts.ContainsKey(xmlBase))
                {
                    profile.BaseNS = atts[xmlBase];
                    Console.WriteLine(profile.BaseNS);
                }
                else
                {
                    string ls;
                    details.TryGetValue(qName, out ls);

                    foreach (KeyValuePair<string, string> at in atts)
                    {
                        if (ls != null)
                        {
                            int i = 0;
                            do
                            {
                                ls = null; ;
                                details.TryGetValue(qName + (++i), out ls);
                            } while (ls != null);
                            ls = at.Value;
                            details.Add(qName + i, ls);
                        }
                        else
                        {
                            ls = at.Value;
                            details.Add(qName, ls);
                        }
                    }
                }

                checkedElementsCount++;
                if (qName.StartsWith(rdfsNamespace) || (qName.StartsWith(cimsNamespace)))
                {
                    documentIdentifiedLikeRDFS = true;
                }
                if ((!documentIdentifiedLikeRDFS) && (checkedElementsCount >= 70))
                {
                    this.profile = null;
                    //occurredError = new ExtendedParseError(new Exception(documentError));
                    abort = true;
                }
            }
        }

        public void EndElement(string localName, string qName)
        {
            if (!abort)
            {
                if (qName.Equals(rdfProfileElement) || qName.Equals(cimsClassCategoryElement)
                    || qName.Equals(rdfsClassElement) || qName.Equals(rdfPropertyElement)) //end of element    
                {
                    if (details != null)
                    {
                        string type;
                        details.TryGetValue(rdfType, out type);

                        if (string.Compare(ExtractSimpleNameFromResourceURI(type), "ClassCategory") == 0)
                        {
                            ClassCategory cs = new ClassCategory();
                            foreach (KeyValuePair<string, string> pp in details)
                            {
                                string str = pp.Value;
                                if ((pp.Key.Equals(cimsBelongsToCategory)) && (str != null))
                                {
                                    cs.BelongsToCategory = str;
                                    AddBelongingInformation(cs, cs.BelongsToCategory);
                                }
                                else if ((pp.Key.Equals(rdfsComment)) && (str != null))
                                {
                                    cs.Comment = str;
                                }
                                else if ((pp.Key.Equals(rdfsLabel)) && (str != null))
                                {
                                    cs.Label = str;
                                }
                                else if ((pp.Key.Equals(rdfType)) && (str != null))
                                {
                                    cs.Type = str;
                                }
                                else if ((pp.Key.Equals(rdfProfileElement)) && (str != null))
                                {
                                    cs.URI = str;
                                }
                                else if ((pp.Key.Equals(cimsMultiplicity)) && (str != null))
                                {
                                    cs.MultiplicityAsString = ExtractSimpleNameFromResourceURI(str);
                                }
                            }
                            AddProfileElement(ProfileElementTypes.ClassCategory, cs);
                        }
                        else if (string.Compare(ExtractSimpleNameFromResourceURI(type), "Class") == 0)
                        {
                            Class cs = new Class();
                            foreach (KeyValuePair<string, string> pp in details)
                            {
                                string str = pp.Value;
                                if ((pp.Key.Equals(cimsBelongsToCategory)) && (str != null))
                                {
                                    cs.BelongsToCategory = str;
                                    AddBelongingInformation(cs, cs.BelongsToCategory);
                                }
                                else if ((pp.Key.Equals(rdfsComment)) && (str != null))
                                {
                                    cs.Comment = str;
                                }
                                else if ((pp.Key.Equals(rdfsLabel)) && (str != null))
                                {
                                    cs.Label = str;
                                }
                                else if ((pp.Key.Equals(cimsMultiplicity)) && (str != null))
                                {
                                    cs.MultiplicityAsString = ExtractSimpleNameFromResourceURI(str);
                                }
                                else if ((pp.Key.Contains(cimsStereotype)) && (str != null))
                                {
                                    cs.AddStereotype(str);
                                }
                                else if ((pp.Key.Contains(rdfsSubClassOf)) && (str != null))
                                {
                                    cs.SubClassOf = str;
                                }
                                else if ((pp.Key.Equals(rdfType)) && (str != null))
                                {
                                    cs.Type = str;
                                }
                                else if ((pp.Key.Equals(rdfProfileElement)) && (str != null))
                                {
                                    cs.URI = str;
                                }
                            }
                            AddProfileElement(ProfileElementTypes.Class, cs);
                        }
                        else if (string.Compare(ExtractSimpleNameFromResourceURI(type), "Property") == 0)
                        {
                            Property pr = new Property();
                            foreach (KeyValuePair<string, string> pp in details)
                            {
                                string str = pp.Value;
                                if ((pp.Key.Equals(cimsDataType)) && (str != null))
                                {
                                    pr.DataType = str;
                                }
                                else if ((pp.Key.Equals(cimsMultiplicity)) && (str != null))
                                {
                                    pr.MultiplicityAsString = str;
                                }
                                else if ((pp.Key.Equals(rdfProfileElement)) && (str != null))
                                {
                                    pr.URI = str;
                                }
                                else if ((pp.Key.Equals(rdfType)) && (str != null))
                                {
                                    pr.Type = str;
                                }
                                else if ((pp.Key.Equals(rdfsDomain)) && (str != null))
                                {
                                    pr.Domain = str;
                                    AddBelongingInformation(pr, pr.Domain);
                                }
                                else if ((pp.Key.Contains(cimsStereotype)) && (str != null))
                                {
                                    pr.AddStereotype(str);
                                }
                                else if ((pp.Key.Contains(rdfsComment)) && (str != null))
                                {
                                    pr.Comment = str;
                                }
                                else if ((pp.Key.Equals(rdfsLabel)) && (str != null))
                                {
                                    pr.Label = str;
                                }
                                else if ((pp.Key.Equals(rdfsRange)) && (str != null))
                                {
                                    pr.Range = str;
                                }
                            }
                            AddProfileElement(ProfileElementTypes.Property, pr);
                        }
                        else
                        {
                            EnumMember en = new EnumMember();
                            foreach (KeyValuePair<string, string> pp in details)
                            {
                                string str = pp.Value;
                                if ((pp.Key.Equals(rdfsComment)) && (str != null))
                                {
                                    en.Comment = str;
                                }
                                else if ((pp.Key.Equals(rdfsLabel)) && (str != null))
                                {
                                    en.Label = str;
                                }
                                else if ((pp.Key.Equals(rdfType)) && (str != null))
                                {
                                    en.Type = str;
                                }
                                else if ((pp.Key.Equals(rdfProfileElement)) && (str != null))
                                {
                                    en.URI = str;
                                }
                                else if ((pp.Key.Equals(cimsMultiplicity)) && (str != null))
                                {
                                    en.MultiplicityAsString = ExtractSimpleNameFromResourceURI(str);
                                }
                            }
                            AddProfileElement(ProfileElementTypes.Unknown, en);
                        }
                        details.Clear();

                    }
                }
                else if (qName.Equals(rdfsLabel)) //// end of label subelement
                {
                    content = content.Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        string ls;
                        details.TryGetValue(qName, out ls);
                        if (ls == null)
                        {
                            ls = (string)content.Clone();
                            details.Add(qName, ls);
                        }
                        content = string.Empty;
                    }
                }
                else if (qName.Equals(rdfsComment)) //// end of comment subelement
                {
                    content = content.Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        string ls;
                        details.TryGetValue(qName, out ls);
                        if (ls == null)
                        {
                            ls = (string)content.Clone();
                            details.Add(qName, ls);
                        }
                        content = string.Empty;
                    }
                }
                else if (qName.Equals(cimsIsAggregate)) //// end of isAggregate subelement
                {
                    content = content.Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        bool paresedValue;

                        string ls;
                        details.TryGetValue(qName, out ls);
                        if (ls == null)
                        {
                            if (bool.TryParse((string)content.Clone(), out paresedValue))
                            {
                                ls = paresedValue.ToString();
                            }
                            details.Add(qName, ls);
                        }
                        content = string.Empty;
                    }
                }
            }
        }

        public void AddProfileElement(ProfileElementTypes tp, ProfileElement el)
        {
            List<ProfileElement> elementsOfSameType = null;
            if (allByType.ContainsKey(tp))
            {
                allByType.TryGetValue(tp, out elementsOfSameType);
            }
            if (elementsOfSameType == null)
            {
                elementsOfSameType = new List<ProfileElement>();
            }
            allByType.Remove(tp);
            elementsOfSameType.Add(el);
            allByType.Add(tp, elementsOfSameType);
        }

        public void StartPrefixMapping(string prefix, string uri)
        {
            throw new NotImplementedException();
        }

        public void Characters(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                content = text;
            }
            else
            {
                content = string.Empty;
            }
        }

        public void EndDocument()
        {
            if (profile != null)
            {
                profile.ProfileMap = allByType;
                ProcessProfile();
            }
        }
        #endregion


        #region Helper methods
        private string ExtractResourceAttributeFromElement(SortedList<string, string> atts)
        {
            string resourceAtt = string.Empty;
            if (atts.ContainsKey(rdfResource))
            {
                resourceAtt = atts[rdfResource];
            }
            return resourceAtt.Trim();
        }

        private string ExtractSimpleNameFromResourceURI(string resourceUri)
        {
            return StringManipulationManager.ExtractShortestName(resourceUri, separator);
        }

        //// Connect property of class with class and classes with packages.        
        private void AddBelongingInformation(ProfileElement currentElement, string classUri)
        {
            if (belongingMap == null)
            {
                belongingMap = new Dictionary<string, Stack<ProfileElement>>();
            }
            Stack<ProfileElement> stack;

            if (!belongingMap.ContainsKey(classUri))
            {
                stack = new Stack<ProfileElement>();
            }
            else
            {
                stack = belongingMap[classUri];
            }
            stack.Push(currentElement);

            belongingMap.Remove(classUri);
            belongingMap.Add(classUri, stack);
        }

        private void ProcessProfile()
        {
            if (profile.ProfileMap != null)
            {
                List<ProfileElement> moveFromUnknownToEnumElement = new List<ProfileElement>();
                foreach (ProfileElementTypes type in profile.ProfileMap.Keys)
                {
					switch (type)
					{
						case ProfileElementTypes.ClassCategory:
							{
								List<ClassCategory> list = profile.ProfileMap[type].Cast<ClassCategory>().ToList();
								foreach (ClassCategory element in list)
								{
									//// search for classes of class categories
									if ((belongingMap != null) && (belongingMap.ContainsKey(element.URI)))
									{
										Stack<ProfileElement> stack = belongingMap[element.URI];
										ProfileElement classInPackage;
										while (stack.Count > 0)
										{
											classInPackage = stack.Pop();
											if (ExtractSimpleNameFromResourceURI(classInPackage.Type).Equals("Class"))
											{
												Class cl = (Class)classInPackage;
												element.AddToMembersOfClassCategory(cl);
												cl.BelongsToCategoryAsObject = element;
											}
											else
											{
												ClassCategory cl = (ClassCategory)classInPackage;
												element.AddToMembersOfClassCategory(cl);
												cl.BelongsToCategoryAsObject = element;
											}
										}
									}
								}
								break;
							}
						case ProfileElementTypes.Class:
							{
								List<Class> list = profile.ProfileMap[type].Cast<Class>().ToList();
								foreach (Class element in list)
								{
									if (element.SubClassOf != null)
									{
										Class uppclass = (Class)profile.FindProfileElementByUri(element.SubClassOf);
										element.SubClassOfAsObject = uppclass;

										if (uppclass != null)
										{
											uppclass.AddToMySubclasses(element);
										}
									}

									//// search for attributes of class and classCategory of class
									if ((belongingMap != null) && (belongingMap.ContainsKey(element.URI)))
									{
										Stack<ProfileElement> stack = belongingMap[element.URI];
										Property property;
										while (stack.Count > 0)
										{
											property = (Property)stack.Pop();
											element.AddToMyProperties(property);
											property.DomainAsObject = element;
										}
									}
								}
								break;
							}
						case ProfileElementTypes.Property:
							{
								List<Property> list = profile.ProfileMap[type].Cast<Property>().ToList();
								foreach (Property element in list)
								{
									if (!element.IsPropertyDataTypeSimple)
									{
										element.DataTypeAsComplexObject = profile.FindProfileElementByUri(element.DataType);
									}
									if (!string.IsNullOrEmpty(element.Range))
									{
										element.RangeAsObject = profile.FindProfileElementByUri(element.Range);
									}
								}
								break;
							}
						case ProfileElementTypes.Unknown:
							{
								List<EnumMember> list = profile.ProfileMap[type].Cast<EnumMember>().ToList();
								foreach (EnumMember element in list)
								{
									Class enumElement = (Class)profile.FindProfileElementByUri(element.Type);
									if (enumElement != null)
									{
										element.EnumerationObject = enumElement;
										enumElement.AddToMyEnumerationMembers(element);
										moveFromUnknownToEnumElement.Add(element);
									}
								}
								break;
							}
					}
                }
                if (moveFromUnknownToEnumElement.Count > 0)
                {
                    List<ProfileElement> unknownsList = null;
                    List<ProfileElement> enumerationElementsList = null;
                    profile.ProfileMap.TryGetValue(ProfileElementTypes.Unknown, out unknownsList);
                    profile.ProfileMap.TryGetValue(ProfileElementTypes.EnumerationElement, out enumerationElementsList);
                    if (unknownsList != null)
                    {
                        if (enumerationElementsList == null)
                        {
                            enumerationElementsList = new List<ProfileElement>();
                        }

                        foreach (ProfileElement movingEl in moveFromUnknownToEnumElement)
                        {
                            unknownsList.Remove(movingEl);
                            enumerationElementsList.Add(movingEl);
                        }

                        profile.ProfileMap.Remove(ProfileElementTypes.Unknown);
                        if (unknownsList.Count > 0)
                        {
                            profile.ProfileMap.Add(ProfileElementTypes.Unknown, unknownsList);
                        }

                        profile.ProfileMap.Remove(ProfileElementTypes.EnumerationElement);
                        if (enumerationElementsList.Count > 0)
                        {
                            enumerationElementsList.Sort(CIMComparer.ProfileElementComparer);
                            profile.ProfileMap.Add(ProfileElementTypes.EnumerationElement, enumerationElementsList);
                        }
                    }
                }
            }
        }
        #endregion
    }
}