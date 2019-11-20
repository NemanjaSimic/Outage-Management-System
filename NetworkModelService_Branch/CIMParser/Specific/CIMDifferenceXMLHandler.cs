using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CIM.ModelCompare;
using CIMParser;
using CIM.Model;
using CIM.Manager;

namespace CIM.Specific
{
	class CIMDifferenceXMLHandler : DefaultXMLHandler
	{

		internal enum Level
		{
			Root = 0,
			Element,
			Property
		};

		internal enum ChangeType
		{
			None = 0,
			Forward,
			Reverse
		};

		internal const string rdfRootQ = "rdf:RDF";
		internal const string rdfRoot = "rdf";
		internal const string rdfID = "rdf:ID";
		internal const string rdfid = "rdf:id";
		internal const string rdfId = "rdf:Id";
		internal const string rdfAbout = "rdf:about";
		internal const string rdfResource = "rdf:resource";
		internal const string xmlnsPrefix = "xmlns";
		internal const string forwardDiffQ = "dm:forwardDifferences";
		internal const string reverseDiffQ = "dm:reverseDifferences";

		//// current level in xml document
		private Level inLevel = Level.Root;
		private ChangeType changeType = ChangeType.None;
		private CIMEntity currentElement; //// current element		

		private string content = string.Empty; //// text content of subelement
		private StreamReader reader;
		private long lastLine = 1;
		private StringBuilder sourceText = new StringBuilder();
		private string xmlBegining = string.Empty;
		private bool x = false;

		private string start = string.Empty;
		private List<CIMEntity> added = new List<CIMEntity>();
		private List<CIMEntity> removed = new List<CIMEntity>();
		Dictionary<string, string> namespaces = new Dictionary<string, string>();

		internal List<CIMEntity> Added
		{
			get
			{
				return added;
			}
			set
			{
				added = value;
			}
		}

		internal List<CIMEntity> Removed
		{
			get
			{
				return removed;
			}
			set
			{
				removed = value;
			}
		}

		public string Start
		{
			get
			{
				return start;
			}
			set
			{
				start = value;
			}
		}

		public Dictionary<string, string> Namespaces
		{
			get
			{
				return namespaces;
			}
		}

		public override void StartDocument()
		{
			currentElement = null;
			
			if (stream != null)
			{
				reader = new StreamReader(stream);
			}
			else
			{
				reader = new StreamReader(this.filePath);
			}

			inLevel = Level.Root;
			changeType = ChangeType.None;
		}

		public override void StartElement(string localName, string qName, SortedList<string, string> attributes, int lineNumber = 0, int columnNumber = 0)
		{
			if(string.Compare(qName, forwardDiffQ, true) == 0)
			{
				changeType = ChangeType.Forward;
			}
			if(string.Compare(qName, reverseDiffQ, true) == 0)
			{
				changeType = ChangeType.Reverse;
			}
			string type = localName;
			if(string.Compare(rdfRoot, type, true) != 0)
			{
				string id = TryReadRDFIdAttribute(attributes);
				//// start of new main element
				if(currentElement == null)
				{
					if(id != null)
					{
						inLevel = Level.Element;
						sourceText.Clear();
						currentElement = new CIMEntity();
						currentElement.RdfID = StringManipulationManager.ExtractAllAfterSeparator(id, StringManipulationManager.SeparatorSharp);
						currentElement.StartLine = lineNumber - 1;
						currentElement.StartColumn = columnNumber;
						GoToPosistion(lineNumber, columnNumber);
						x = true;
					}
				}
				//// start of new subelement (attribute or embedded element)
				else
				{
					//// new attribute
					inLevel = Level.Property;
				}
			}
		}

		public override void EndElement(string localName, string qName, int lineNumber = 0, int columnNumber = 0)
		{
			string type = localName;

			if(string.Compare(rdfRoot, type, true) != 0)
			{
				//// end of main element
				if(inLevel == Level.Element)
				{
					inLevel = Level.Root;
					if(currentElement != null)
					{
						currentElement.EndLine = lineNumber;
						currentElement.EndColumn = columnNumber + qName.Length + 3;

						ReadToPosition(currentElement.EndLine, currentElement.EndColumn);
						currentElement.Source = sourceText.ToString();

						////add it to forward or reverse
						if(changeType == ChangeType.Forward)
						{
							added.Add(currentElement);
						}
						if(changeType == ChangeType.Reverse)
						{
							removed.Add(currentElement);
						}
						
						currentElement = null;
					}
				}
				//// end of an attribute (property)
				else if(inLevel == Level.Property)
				{
					inLevel = Level.Element;

					content = content.Trim();
					if(string.Compare(CIMConstants.AttributeNameIdentifiedObjectMRID, localName) == 0)
					{
						currentElement.MRID = content;
					}
					content = string.Empty;
				}
			}
		}

		public override void Characters(string text)
		{
			if(!string.IsNullOrEmpty(text))
			{
				content = text;
			}
			else
			{
				content = string.Empty;
			}
		}

		public override void StartPrefixMapping(string prefix, string uri)
		{
			if(string.IsNullOrEmpty(prefix))
			{
				prefix = string.Empty;
			}
			namespaces.Add(prefix, uri);
		}

		public override void EndDocument()
		{
			if(reader != null)
			{
				reader.Close();
				reader.Dispose();
			}
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

		#region private: Helper methods
		private void GoToPosistion(long startLine, int startColumn)
		{
			startColumn = (startColumn > 0) ? startColumn - 1 : 0;
			if(startLine >= lastLine)
			{
				long i = lastLine;
				while(reader.Peek() >= 0)
				{
					string line = reader.ReadLine();
					if(!x && (i != startLine))
					{
						xmlBegining += line;
					}
					lastLine++;
					if(i >= startLine)
					{
						if(!string.IsNullOrEmpty(line))
						{
							if(i == startLine)
							{
								sourceText.AppendLine((line.Substring(startColumn).Trim()));
								break;
							}
						}
					}
					i++;
				}
			}
		}

		private void ReadToPosition(int endLine, int endColumn)
		{
			if(endLine >= lastLine)
			{
				long i = lastLine;
				while((reader.Peek() >= 0) && (i <= endLine))
				{
					string line = reader.ReadLine();
					lastLine++;
					if(i == endLine)
					{
						sourceText.AppendLine((line.Substring(0, endColumn)).Trim());
					}
					else
					{
						sourceText.AppendLine(line.Trim());
					}
					i++;
				}
			}
		}


		//// Method tries to read the "rdf:ID" ettribute from list of attributes
		//// and returns it's value or null value if such attribute can't be found.
		private string TryReadRDFIdAttribute(SortedList<string, string> atts)
		{
			string rdfIdentificator = null;
			if(atts != null)
			{
				if(atts.ContainsKey(rdfID))
				{
					rdfIdentificator = atts[rdfID];
				}
				else if(atts.ContainsKey(rdfid))
				{
					rdfIdentificator = atts[rdfid];
				}
				else if(atts.ContainsKey(rdfId))
				{
					rdfIdentificator = atts[rdfId];
				}
				else if(atts.ContainsKey(rdfAbout))
				{
					rdfIdentificator = atts[rdfAbout];
				}
			}
			return rdfIdentificator;
		}

		#endregion private: Helper methods
	}
}
