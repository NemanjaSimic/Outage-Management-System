using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CIM.Manager;
using CIM.Model;

namespace CIM.Handler
{
	class RDFSXMLReaderHandler:IHandler
	{
		#region fields

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

		private string content = string.Empty; //// text content of element
		private Profile profile;
		private SortedDictionary<ProfileElementTypes, List<ProfileElement>> allByType;
		//// helper map:  parent class uri,   properties
		////   ie.        package uri,      classes
		private Dictionary<string, Stack<ProfileElement>> belongingMap;
		private ProfileElement currentElement;

		//// for checking if document can't be processed as CIM-RDFS
		private bool documentIdentifiedLikeRDFS = false;
		private int checkedElementsCount = 0;
		private bool abort = false;

		/// <summary>
		/// Gets the Profile object which is finall product of parsing RDFS document.
		/// </summary>
		public Profile Profile
		{
			get
			{
				return profile;
			}
		}

		#endregion

		#region IHandler Members

		public void StartDocument(string filePath)
		{
			currentElement = null;
			profile = new Profile();
			profile.SourcePath = filePath;
			allByType = new SortedDictionary<ProfileElementTypes, List<ProfileElement>>();

			checkedElementsCount = 0;
			documentIdentifiedLikeRDFS = false;
			abort = false;
		}

		public void StartElement(string localName, string qName, SortedList<string, string> atts)
		{
			if(!abort)
			{
				/**
				 * Deo neophodan za proveru ako postoji xml:base jer tada elementi, bar vecina nema nista pre #
				 */
				if(atts.ContainsKey(xmlBase))
				{
					profile.BaseNS = atts[xmlBase];
					Console.WriteLine(profile.BaseNS);
				}

				checkedElementsCount++;
				if(qName.StartsWith(rdfsNamespace) || (qName.StartsWith(cimsNamespace)))
				{
					documentIdentifiedLikeRDFS = true;
				}
				if((!documentIdentifiedLikeRDFS) && (checkedElementsCount >= 70))
				{
					this.profile = null;
					//occurredError = new ExtendedParseError(new Exception(documentError));
					abort = true;
				}

				if(qName.Equals(rdfProfileElement) || qName.Equals(cimsClassCategoryElement)
					|| qName.Equals(rdfsClassElement) || qName.Equals(rdfPropertyElement)) //start of new profile element
				{
					currentElement = new ProfileElement();

					if(atts.ContainsKey(rdfAbout))
					{
						currentElement.URI = atts[rdfAbout];
					}
					else if(atts.ContainsKey(rdfId)) // some elements can have rdf:Id instead rdf:about
					{
						currentElement.URI = atts[rdfId];
					}

					switch(qName)
					{
						case cimsClassCategoryElement:
							{
								currentElement.Type = ProfileElement.TypeClassCategoryString;
								break;
							}
						case rdfsClassElement:
							{
								currentElement.Type = ProfileElement.TypeClassString;
								break;
							}
						case rdfPropertyElement:
							{
								currentElement.Type = ProfileElement.TypePropertyString;
								break;
							}
					}
				}
				else if(qName.Equals(rdfType))
				{
					if(currentElement != null)
					{
						currentElement.Type = ExtractResourceAttributeFromElement(atts);
					}
				}
				else if(qName.Equals(rdfsSubClassOf))
				{
					if(currentElement != null)
					{
						currentElement.SubClassOf = ExtractResourceAttributeFromElement(atts);
					}
				}
				else if(qName.Equals(rdfsDomain))
				{
					if(currentElement != null)
					{
						string domainOfProperty = ExtractResourceAttributeFromElement(atts);
						////if (domainOfProperty.StartsWith(StringManipulationManager.SeparatorSharp))
						////{
						////    domainOfProperty = domainOfProperty.Substring(1);
						////}
						currentElement.Domain = domainOfProperty;

						AddBelongingInformation(currentElement.Domain);
					}
				}
				else if(qName.Equals(rdfsRange))
				{
					if(currentElement != null)
					{
						currentElement.Range = ExtractResourceAttributeFromElement(atts);
					}
				}
				else if(qName.Equals(cimsBelongsToCategory))
				{
					if(currentElement != null)
					{
						string belongToCategory = ExtractResourceAttributeFromElement(atts);
						////if (belongToCategory.StartsWith(StringManipulationManager.SeparatorSharp))
						////{
						////    belongToCategory = belongToCategory.Substring(1);
						////}
						currentElement.BelongsToCategory = belongToCategory;

						AddBelongingInformation(currentElement.BelongsToCategory);
					}
				}
				else if(qName.Equals(cimsStereotype))
				{
					if(currentElement != null)
					{
						string stereotype = ExtractResourceAttributeFromElement(atts);
						currentElement.AddStereotype(stereotype);
					}
				}
				else if(qName.Equals(cimsDataType))
				{
					if(currentElement != null)
					{
						currentElement.DataType = ExtractResourceAttributeFromElement(atts);
					}
				}
				else if(qName.Equals(cimsInverseRoleName))
				{
					if(currentElement != null)
					{
						currentElement.InverseRoleName = ExtractResourceAttributeFromElement(atts);
					}
				}
				else if(qName.Equals(cimsMultiplicity))
				{
					if(currentElement != null)
					{
						currentElement.MultiplicityAsString = ExtractSimpleNameFromResourceURI(ExtractResourceAttributeFromElement(atts));
					}
				}
			}
		}

		public void EndElement(string localName, string qName)
		{
			if(!abort)
			{
				if(qName.Equals(rdfProfileElement) || qName.Equals(cimsClassCategoryElement)
					|| qName.Equals(rdfsClassElement) || qName.Equals(rdfPropertyElement)) //end of element    
				{
					if(currentElement != null)
					{
						List<ProfileElement> elementsOfSameType = null;
						if(allByType.ContainsKey(currentElement.TypeAsEnumValue))
							allByType.TryGetValue(currentElement.TypeAsEnumValue, out elementsOfSameType);

						if(elementsOfSameType == null)
						{
							elementsOfSameType = new List<ProfileElement>();
						}

						allByType.Remove(currentElement.TypeAsEnumValue);

						elementsOfSameType.Add(currentElement);
						allByType.Add(currentElement.TypeAsEnumValue, elementsOfSameType);

						currentElement = null;
					}
				}
				else if(qName.Equals(rdfsLabel)) //// end of label subelement
				{
					content = content.Trim();
					if(!string.IsNullOrEmpty(content))
					{
						if(currentElement != null)
						{
							currentElement.Label = (string)content.Clone();
						}
						content = string.Empty;
					}
				}
				else if(qName.Equals(rdfsComment)) //// end of comment subelement
				{
					content = content.Trim();
					if(!string.IsNullOrEmpty(content))
					{
						if(currentElement != null)
						{
							currentElement.Comment = (string)content.Clone();
						}
						content = string.Empty;
					}
				}
				else if(qName.Equals(cimsIsAggregate)) //// end of isAggregate subelement
				{
					content = content.Trim();
					if(!string.IsNullOrEmpty(content))
					{
						bool paresedValue;
						if(currentElement != null)
						{
							if(bool.TryParse((string)content.Clone(), out paresedValue))
							{
								currentElement.IsAggregate = paresedValue;
							}
						}
						content = string.Empty;
					}
				}
			}
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
			if(profile != null)
			{
				profile.ProfileMap = allByType;
				ProcessProfile();
			}
		}

		public Model.CIMModel GetModel()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Helper methods
		private string ExtractResourceAttributeFromElement(SortedList<string, string> atts)
		{
			string resourceAtt = string.Empty;
			if(atts.ContainsKey(rdfResource))
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
		private void AddBelongingInformation(string classUri)
		{
			if(belongingMap == null)
			{
				belongingMap = new Dictionary<string, Stack<ProfileElement>>();
			}
			Stack<ProfileElement> stack;

			if(!belongingMap.ContainsKey(classUri))
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
			if(profile.ProfileMap != null)
			{
				List<ProfileElement> moveFromUnknownToEnumElement = new List<ProfileElement>();
				foreach(ProfileElementTypes type in profile.ProfileMap.Keys)
				{
					List<ProfileElement> list = profile.ProfileMap[type];

					foreach(ProfileElement element in list)
					{
						switch(type)
						{
							case ProfileElementTypes.ClassCategory:
								{
									//// search for classes of class categories
									if((belongingMap != null) && (belongingMap.ContainsKey(element.URI)))
									{
										Stack<ProfileElement> stack = belongingMap[element.URI];
										ProfileElement classInPackage;
										while(stack.Count > 0)
										{
											classInPackage = stack.Pop();
											element.AddToMembersOfClassCategory(classInPackage);
											classInPackage.BelongsToCategoryAsObject = element;
										}
									}
									break;
								}
							case ProfileElementTypes.Class:
								{
									if(element.SubClassOf != null)
									{
										ProfileElement uppclass = profile.FindProfileElementByUri(element.SubClassOf);
										element.SubClassOfAsObject = uppclass;

										if(uppclass != null)
										{
											uppclass.AddToMySubclasses(element);
										}
									}

									//// search for attributes of class and classCategory of class
									if((belongingMap != null) && (belongingMap.ContainsKey(element.URI)))
									{
										Stack<ProfileElement> stack = belongingMap[element.URI];
										ProfileElement property;
										while(stack.Count > 0)
										{
											property = stack.Pop();
											element.AddToMyProperties(property);
											property.DomainAsObject = element;
										}
									}
									break;
								}
							case ProfileElementTypes.Property:
								{
									if(!element.IsPropertyDataTypeSimple)
									{
										element.DataTypeAsComplexObject = profile.FindProfileElementByUri(element.DataType);
									}
									if(!string.IsNullOrEmpty(element.Range))
									{
										element.RangeAsObject = profile.FindProfileElementByUri(element.Range);
									}
									if(!string.IsNullOrEmpty(element.InverseRoleName))
									{
										element.InverseRoleAsObject = profile.FindProfileElementByUri(element.InverseRoleName);
									}

									if(!string.IsNullOrEmpty(element.Name) && (Char.IsUpper(element.Name[0]))
										&& (!element.HasStereotype(ProfileElementStereotype.StereotypeByReference)))
									{
										element.IsExpectedToContainLocalClass = true;
										if(element.RangeAsObject != null)
										{
											element.RangeAsObject.IsExpectedAsLocal = true;
										}
									}
									break;
								}
							case ProfileElementTypes.Unknown:
								{
									ProfileElement enumElement = profile.FindProfileElementByUri(element.Type);
									if(enumElement != null)
									{
										element.EnumerationObject = enumElement;
										element.TypeAsEnumValue = ProfileElementTypes.EnumerationElement;
										enumElement.AddToMyEnumerationMembers(element);
										moveFromUnknownToEnumElement.Add(element);
									}
									break;
								}
						}
					}
				}
				if(moveFromUnknownToEnumElement.Count > 0)
				{
					List<ProfileElement> unknownsList = null;
					List<ProfileElement> enumerationElementsList = null;
					profile.ProfileMap.TryGetValue(ProfileElementTypes.Unknown, out unknownsList);
					profile.ProfileMap.TryGetValue(ProfileElementTypes.EnumerationElement, out enumerationElementsList);
					if(unknownsList != null)
					{
						if(enumerationElementsList == null)
						{
							enumerationElementsList = new List<ProfileElement>();
						}

						foreach(ProfileElement movingEl in moveFromUnknownToEnumElement)
						{
							unknownsList.Remove(movingEl);
							enumerationElementsList.Add(movingEl);
						}

						profile.ProfileMap.Remove(ProfileElementTypes.Unknown);
						if(unknownsList.Count > 0)
						{
							profile.ProfileMap.Add(ProfileElementTypes.Unknown, unknownsList);
						}

						profile.ProfileMap.Remove(ProfileElementTypes.EnumerationElement);
						if(enumerationElementsList.Count > 0)
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
