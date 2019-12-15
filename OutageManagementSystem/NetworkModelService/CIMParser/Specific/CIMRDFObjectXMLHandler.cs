using System;
using System.Collections.Generic;
using CIM.Model;
using CIMParser;
using CIM.Manager;

namespace CIM.Specific
{
	class CIMRDFObjectXMLHandler : DefaultXMLHandler
	{
		internal enum Level
		{
			Root = 0,
			Element,
			Property
		};

		internal const string rdfRoot = "rdf";
		internal const string rdfID = "rdf:ID";
		internal const string rdfid = "rdf:id";
		internal const string rdfId = "rdf:Id";
		internal const string rdfResource = "rdf:resource";
		

		//// current level in xml document
		private Level inLevel = Level.Root;

		private CIMModelContext cimModelContext;
		private CIMObject currentElement = null; //// current element                
		private ObjectAttribute currentAttribute; //// current subelement (attribute of CIM object)
		
		private string content = string.Empty; //// text content of subelement        


		/// <summary>
		/// Gets the CIMModel object which is finall product of parsing RDF document.
		/// </summary>
		public CIMObject Object
		{
			get
			{
				return currentElement;
			}
		}


		public CIMModelContext CimModelContext
		{
			get
			{
				return cimModelContext;
			}
			set
			{
				cimModelContext = value;
			}
		}


		#region public: XML Handler methods
		public override void StartDocument()
		{
			currentElement = null;
			currentAttribute = null;
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

			if (string.Compare(rdfRoot, type, true) != 0)
			{
				string id = TryReadRDFIdAttribute(attributes);
				//// start of new main element
				if (currentElement == null)
				{
					if (id != null)
					{
						inLevel = Level.Element;
						currentElement = new CIMObject(cimModelContext, type);
						currentElement.ID = id;
						currentElement.NamespaceString = namespaceName;
						currentElement.SourceStartLine = lineNumber;
						currentElement.SourceStartColumn = columnNumber;
					}
				}
				//// start of new subelement (attribute or embedded element)
				else
				{
					//// new attribute
					inLevel = Level.Property;
					currentAttribute = new ObjectAttribute(cimModelContext, type);
					currentAttribute.NamespaceString = namespaceName;

					ReadAndProcessAttributeValue(currentAttribute, attributes);
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
					}
				}
				//// end of an attribute (property)
				else if (inLevel == Level.Property)
				{
					inLevel = Level.Element;
				
					content = content.Trim();
					if (!string.IsNullOrEmpty(content))
					{
						if (currentAttribute != null)
						{
							currentAttribute.Value = content;
						}
						content = string.Empty;
					}
					AddAttributeToCurrentElement();
				}
			}
		}

		public override void StartPrefixMapping(string prefix, string uri)
		{
			
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
					objectAttribute.Value = atts[rdfResource];
					objectAttribute.IsReference = true;
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
					currentElement.AddAttribute(currentAttribute);
				}
			}
		}
		#endregion private: Helper methods

	}
}
