using System;
using System.Collections.Generic;
using CIM.Model;
using CIMParser;
using CIM.Manager;


namespace CIM.Specific
{
	/// <summary>
	/// CIMRDFDocumentXMLHandler class is specific implementation of DefaultXMLHandler.
	/// <para>Class implements handler methods for parsing CIM data in RDF format.</para>
	/// <para>Outcome of processing file can be red from the Model property.</para>
	/// <para>@author: Stanislava Selena</para>
	/// </summary>
	class CIMRDFDocumentXMLHandler : DefaultXMLHandler
	{
		internal enum Level
		{
			Root = 0,
			Element,
			Property,
			EmbeddedList,
			EmbeddedElement
		};

		internal const string rdfRootQ = "rdf:RDF";
		internal const string rdfRoot = "rdf";
		internal const string rdfID = "rdf:ID";
		internal const string rdfid = "rdf:id";
		internal const string rdfId = "rdf:Id";
		internal const string rdfResource = "rdf:resource";
		internal const string xmlnsPrefix = "xmlns";

		//// current level in xml document
		private Level inLevel = Level.Root;

		private CIMModel model; //// object CIM model which content is being loaded during parsing process

		private CIMObject currentElement; //// current element                
		private ObjectAttribute currentAttribute; //// current subelement (attribute of CIM object)
		private string embeddedListName = string.Empty; //// current embedded list name
		private CIMObject embeddedElement; //// current embedded element

		private string content = string.Empty; //// text content of subelement        

		//// helper map:   parent id      children
		private Dictionary<string, Stack<CIMObject>> childrenOf;


		/// <summary>
		/// Gets the CIMModel object which is finall product of parsing RDF document.
		/// </summary>
		public CIMModel Model
		{
			get
			{
				return model;
			}
		}

		#region public: XML Handler methods
		public override void StartDocument()
		{
			currentElement = null;
			currentAttribute = null;
			embeddedElement = null;
			if (model == null)
			{
				model = new CIMModel();
				model.SourcePath = this.filePath;
			}
			childrenOf = new Dictionary<string, Stack<CIMObject>>();
			inLevel = Level.Root;
		}

		public override void StartElement(string localName, string qName, SortedList<string, string> attributes, int lineNumber = 0, int columnNumber = 0)
		{
			string type = localName;
			string namespaceName = string.Empty;
			if (qName.Contains(StringManipulationManager.SeparatorColon))
			{
				namespaceName = qName.Substring(0, qName.IndexOf(StringManipulationManager.SeparatorColon));
			}

			if (string.Compare(qName, rdfRootQ, true) == 0)
			{
				//// rdf:RDF element - extract attributes
				if ((attributes != null) && (attributes.Count > 0))
				{
					foreach (string attrName in attributes.Keys)
					{
						if (!attrName.StartsWith(xmlnsPrefix, true, null))
						{
							model.AddRDFAttribute(attrName, attributes[attrName]);
						}
					}
				}
			}

			if (string.Compare(rdfRoot, type, true) != 0)
			{
				string id = TryReadRDFIdAttribute(attributes);
				//// start of new main element
				if (currentElement == null)
				{	
					if (id != null)
					{
						inLevel = Level.Element;
						currentElement = new CIMObject(model.ModelContext, type);
						currentElement.ID = id;
						currentElement.NamespaceString = namespaceName;
						currentElement.SourceStartLine = lineNumber;
						currentElement.SourceStartColumn = columnNumber;
					}
				}
				//// start of new subelement (attribute or embedded element)
				else
				{
					//// if rdf:ID is defined -> it is new embedded element
					if (id != null)
					{
						inLevel = Level.EmbeddedElement;
						embeddedElement = new CIMObject(model.ModelContext, type);
						embeddedElement.ID = id;
						embeddedElement.NamespaceString = namespaceName;
						embeddedElement.SourceStartLine = lineNumber;
						embeddedElement.SourceStartColumn = columnNumber;
						embeddedElement.IsDefinedAsEmbedded = true;

						if (string.IsNullOrEmpty(embeddedListName))
						{
							embeddedListName = currentAttribute.FullName;
							if (!string.IsNullOrEmpty(currentAttribute.NamespaceString))
							{
								embeddedListName = currentAttribute.NamespacePrintString + embeddedListName;
							}
						}
						currentAttribute = null;
						AddEmbeddedChildToCurrentElement();
					}
					//// new attribute
					else
					{
						inLevel = Level.Property;
						currentAttribute = new ObjectAttribute(model.ModelContext, type);
						currentAttribute.NamespaceString = namespaceName;

						ReadAndProcessAttributeValue(currentAttribute, attributes);
					}
				}
			}
		}

		public override void EndElement(string localName, string qName, int lineNumber = 0, int columnNumber = 0)
		{
			string type = localName;

			if (string.Compare(rdfRoot, type, true) != 0)
			{
				//// end of main element
				if (inLevel == Level.Element)
				{
					inLevel = Level.Root;
					if (currentElement != null)
					{
						currentElement.SourceEndLine = lineNumber;
						currentElement.SourceEndColumn = columnNumber + qName.Length + 3;
						if (model.GetObjectByID(currentElement.ID) == null)
						{
							model.InsertObjectInModelMap(currentElement);
						}
						else
						{
							AddValidationMessageAboutDuplicateRdfId();
						}
						currentElement = null;
					}

					
				}
				//// end of embededList of elements
				else if (inLevel == Level.EmbeddedList)
				{
					inLevel = Level.Element;
					embeddedListName = string.Empty;
				}
				//// end of embeded element
				else if (inLevel == Level.EmbeddedElement)
				{
					inLevel = Level.EmbeddedList;
					if (embeddedElement != null)
					{
						embeddedElement.SourceEndLine = lineNumber;
						embeddedElement.SourceEndColumn = columnNumber + qName.Length + 3;
						if (model.GetObjectByID(embeddedElement.ID) == null)
						{
							model.InsertObjectInModelMap(embeddedElement);
						}
						else
						{
							AddValidationMessageAboutDuplicateRdfId();
						}
						embeddedElement = null;
					}
					
				}
				//// end of an attribute (property)
				else if (inLevel == Level.Property)
				{
					if (embeddedElement == null)
					{
						inLevel = Level.Element;
					}
					else
					{
						inLevel = Level.EmbeddedElement;
					}

					content = content.Trim();
					if (!string.IsNullOrEmpty(content))
					{
						if (currentAttribute != null)
						{
							//// if attribute value wasn't set as the reference, we read the text content as set it in value
							if (string.IsNullOrEmpty(currentAttribute.Value))
							{
								currentAttribute.Value = content;
								if (string.IsNullOrEmpty(currentAttribute.Value))
								{
									AddValidationMessageAboutEmptyValueOfProperty();
								}
							}
						}
						content = string.Empty;
					}

					AddAttributeToCurrentElement();
				}
			}
		}

		public override void StartPrefixMapping(string prefix, string uri)
		{
			if (string.IsNullOrEmpty(prefix))
			{
				prefix = string.Empty;
			}
			model.AddRDFNamespace(prefix, uri);
		}

		public override void Characters(string text)
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

		public override void EndDocument()
		{
			JoinReferences();
			model.FinishModelMap();
		}

		public override void FatalError(Exception error)
		{
			occurredError = new XMLParseError(error);
		}

		public override void Error(Exception error)
		{
			occurredError = new XMLParseError(error);
		}

		public override void Warning(Exception error)
		{
			occurredError = new XMLParseError(error);
		}
		#endregion public: XML Handler methods


		#region private: Helper methods
		//// Method tries to read the "rdf:ID" ettribute from list of attributes
		//// and returns it's value or null value if such attribute can't be found.
		private string TryReadRDFIdAttribute(SortedList<string, string> atts)
		{
			string rdfIdentificator = null;
			if (atts != null)
			{
				if (atts.ContainsKey(rdfID))
				{
					rdfIdentificator = atts[rdfID];
				}
				else if (atts.ContainsKey(rdfid))
				{
					rdfIdentificator = atts[rdfid];
				}
				else if (atts.ContainsKey(rdfId))
				{
					rdfIdentificator = atts[rdfId];
				}
			}
			return rdfIdentificator;
		}

		//// Method tries to read the "rdf:resource" attribute and if it is found,
		//// it connects the referenced object (parent) with child object.
		private void ReadAndProcessAttributeValue(ObjectAttribute objectAttribute, SortedList<string, string> atts)
		{
			if ((objectAttribute != null) && (atts != null))
			{
				//// if object attribute contains reference to another element, 
				//// it is being read and put on stack of that element (parent element)                
				if (atts.ContainsKey(rdfResource))
				{
					string parentId = atts[rdfResource];
					objectAttribute.Value = parentId;
					objectAttribute.IsReference = true;

					if (parentId.StartsWith(StringManipulationManager.SeparatorSharp))
					{
						parentId = StringManipulationManager.ExtractAllAfterSeparator(parentId, StringManipulationManager.SeparatorSharp);
					}
					AddChildElementToReferencedParentElement(parentId);
				}
			}
		}

		//// Method adds the currentAttribute to currentElement if both of them exists,
		//// ie. adds currentAttribute to embeddedElement.
		private void AddAttributeToCurrentElement()
		{
			if (currentAttribute != null)
			{
				if ((inLevel == Level.Element) && (currentElement != null))
				{
					if (!currentElement.AddAttribute(currentAttribute))
					{
						AddValidationMessageAboutDuplicatePropertyForElementCount();
					}
				}
				else if ((inLevel == Level.EmbeddedElement) && (embeddedElement != null))
				{
					if (!embeddedElement.AddAttribute(currentAttribute))
					{
						AddValidationMessageAboutDuplicatePropertyForElementCount();
					}
				}
			}
		}

		//// Method adds value of ID property of current element on the stack that corespondes to parentId.
		//// If that stack doesn't exist jet, it is being created and current's element ID pushed on it.
		//// Created stacks will be processed on the end of parsing process.
		private void AddChildElementToReferencedParentElement(string parentId)
		{
			if (!string.IsNullOrEmpty(parentId))
			{
				if (childrenOf == null)
				{
					childrenOf = new Dictionary<string, Stack<CIMObject>>();
				}

				Stack<CIMObject> stack;
				if (!childrenOf.ContainsKey(parentId))
				{
					stack = new Stack<CIMObject>();
				}
				else
				{
					stack = childrenOf[parentId];
				}

				if (embeddedElement != null)
				{
					stack.Push(embeddedElement);
				}
				else
				{
					stack.Push(currentElement);
				}
				childrenOf.Remove(parentId);
				childrenOf.Add(parentId, stack);
			}
		}

		private void AddEmbeddedChildToCurrentElement()
		{
			if ((embeddedElement != null) && (currentElement != null))
			{
				if (!string.IsNullOrEmpty(currentElement.ID))
				{
					if (childrenOf == null)
					{
						childrenOf = new Dictionary<string, Stack<CIMObject>>();
					}

					Stack<CIMObject> stack;
					if (!childrenOf.ContainsKey(currentElement.ID))
					{
						stack = new Stack<CIMObject>();
					}
					else
					{
						stack = childrenOf[currentElement.ID];
					}

					stack.Push(embeddedElement);

					childrenOf.Remove(currentElement.ID);
					childrenOf.Add(currentElement.ID, stack);
					currentElement.AddEmbeddedChild(embeddedListName, embeddedElement);
					embeddedElement.EmbeddedParentId = currentElement.ID;
				}
			}
		}

		//// Method uses helper structure childrenOf (map of stacks) to connect every 
		//// child object with it's parent objects.
		//// (spajanje parenta sa decom i dece sa parentom)
		private void JoinReferences()
		{
			if (childrenOf != null)
			{
				CIMObject parent;
				foreach (string id in childrenOf.Keys)
				{
					//// try to find referenced parent object in model
					parent = model.GetObjectByID(id);

					if (parent != null)
					{
						Stack<CIMObject> stack = childrenOf[id];
						CIMObject child;
						while (stack.Count > 0)
						{
							child = stack.Pop();
							parent.AddChild(child);
							child.AddParent(parent);
						}
					}
					else
					{   //// if the CIMObject with given "id" wasn't found in model,
						//// for every child object that references it, 
						//// the "ReferenceToUnexistingElement" validation message
						//// should be reported
						Stack<CIMObject> stack = childrenOf[id];
						CIMObject child;
						while (stack.Count > 0)
						{
							child = stack.Pop();
							List<ObjectAttribute> badAttributes = child.GetMyAttributesWithValue(id, true);
							if (badAttributes != null)
							{
								foreach (ObjectAttribute attribute in badAttributes)
								{
									AddValidationMessageAboutReferenceToUnexistingElement(child, attribute, id);
								}
							}
						}
					}
				}
			}
		}

		//// If is there has been the EndElement event on currentAttribute, but no value was set for it,
		//// this method will add the proper validation message to model.
		private void AddValidationMessageAboutEmptyValueOfProperty()
		{
			//if (model.ValidationDataFromParsing == null)
			//{
			//    model.ValidationDataFromParsing = new ValidationData(model);
			//    model.ValidationDataFromParsing.StartTime = DateTime.Now;
			//}

			//if ((currentElement != null) && (currentAttribute != null) && string.IsNullOrEmpty(currentAttribute.Value))
			//{
			//    //// add validation message to model and attache it to the problematic attribute                
			//    currentAttribute.AddValidationMessage((model.ValidationDataFromParsing.AddOrUpdateMessage(ValidationMessageType.EmptyValueOfProperty, this.FilePath,
			//                                   currentElement.CIMType, currentAttribute.FullName, null, currentElement.ID)).ID);
			//}
			//model.ValidationDataFromParsing.EndTime = DateTime.Now;
		}

		//// If some object in model references the unexisting parent object by some 
		//// of it's properties, the "ReferenceToUnexistingElement" validation message is reported.
		private void AddValidationMessageAboutReferenceToUnexistingElement(CIMObject childObject, ObjectAttribute invalidProperty, string unexistingParentId)
		{
			//if (model.ValidationDataFromParsing == null)
			//{
			//    model.ValidationDataFromParsing = new ValidationData(model);
			//    model.ValidationDataFromParsing.StartTime = DateTime.Now;
			//}

			//if (childObject != null)
			//{
			//    //// add validation message to model and attache it to the problematic attribute
			//    invalidProperty.AddValidationMessage((model.ValidationDataFromParsing.AddOrUpdateMessage(ValidationMessageType.ReferenceToUnexistingElement, this.FilePath,
			//                                            childObject.CIMType, invalidProperty.FullName, unexistingParentId, childObject.ID)).ID);
			//}
			//model.ValidationDataFromParsing.EndTime = DateTime.Now;
		}

		//// If duplicate rdf:Id has been found before currentElement was included in model map,
		//// this element will be ignored and method will add the proper validation message to model.
		private void AddValidationMessageAboutDuplicateRdfId()
		{
			//if (model.ValidationDataFromParsing == null)
			//{
			//    model.ValidationDataFromParsing = new ValidationData(model);
			//    model.ValidationDataFromParsing.StartTime = DateTime.Now;
			//}

			//if ((currentElement != null) && !string.IsNullOrEmpty(currentElement.ID))
			//{
			//    model.ValidationDataFromParsing.AddOrUpdateMessage(ValidationMessageType.DuplicateRdfId, this.FilePath,
			//                                   currentElement.CIMType, null, currentElement.ID, currentElement.ID);
			//}
			//model.ValidationDataFromParsing.EndTime = DateTime.Now;
		}

		//// If duplicate attrbute has been found for currentElement,
		//// this attribute will marked as duplicate and method will add the proper validation message to model.
		private void AddValidationMessageAboutDuplicatePropertyForElementCount()
		{
			//if (model.ValidationDataFromParsing == null)
			//{
			//    model.ValidationDataFromParsing = new ValidationData(model);
			//    model.ValidationDataFromParsing.StartTime = DateTime.Now;
			//}

			//if ((currentElement != null) && (currentAttribute != null) && !string.IsNullOrEmpty(currentElement.ID))
			//{
			//    currentAttribute.AddValidationMessage((model.ValidationDataFromParsing.AddOrUpdateMessage(ValidationMessageType.DuplicatePropertyForElement, this.FilePath,
			//                                   currentElement.CIMType, currentAttribute.FullName, currentAttribute.Value, currentElement.ID)).ID);
			//}
			//model.ValidationDataFromParsing.EndTime = DateTime.Now;
		}
		#endregion private: Helper methods

	}
}
