using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CIM.ModelCompare;
using CIMParser;
using CIM.Manager;
using CIM.Model;


namespace CIM.Specific
{
	/// <summary>
	/// CIMRDFComparerXMLHandler class is specific implementation of DefaultXMLHandler.
	/// <para>Class implements handler methods for parsing CIM data in RDF format.</para>
	/// <para>Outcome of processing file can be red from the Model property.</para>
	/// <para>@author: Stanislava Selena</para>
	/// </summary>
	class CIMRDFComparerXMLHandler : DefaultXMLHandler
	{
		internal enum Level
		{
			Root = 0,
			Element,
			Property
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
		private CIMModelSets model; //// object CIM model which content is being loaded during parsing process
		private CIMEntity currentElement; //// current element	
		private Dictionary<string, string> namespaces = new Dictionary<string, string>();
		
		private string content = string.Empty; //// text content of subelement
		private StreamReader reader;
		private long lastLine = 1;
		private StringBuilder sourceText = new StringBuilder();

		private MD5 md5 = new MD5CryptoServiceProvider();
		private string xmlBegining = string.Empty;
		private bool x = false;

		public Dictionary<string, string> Namespaces
		{
			get
			{
				return namespaces;
			}
		}


		/// <summary>
		/// Gets the CIMModel object which is finall product of parsing RDF document.
		/// </summary>
		public CIMModelSets Model
		{
			get
			{
				return model;
			}
		}

		public string Start
		{
			get
			{
				return xmlBegining;
			}
		}


		#region public: XML Handler methods
		public override void StartDocument()
		{
			currentElement = null;
			if (model == null)
			{
				model = new CIMModelSets();
				model.SourcePath = this.filePath;
			}

			if (stream != null)
			{
				reader = new StreamReader(stream);
			}
			else
			{
				reader = new StreamReader(model.SourcePath);
			}
			
			inLevel = Level.Root;
		}

		public override void StartElement(string localName, string qName, SortedList<string, string> attributes, int lineNumber = 0, int columnNumber = 0)
		{
			string type = localName;
			string namespaceName = string.Empty;
			if(qName.Contains(StringManipulationManager.SeparatorColon))
			{
				namespaceName = qName.Substring(0, qName.IndexOf(StringManipulationManager.SeparatorColon));
			}

			if(string.Compare(qName, rdfRootQ, true) == 0)
			{
				//// rdf:RDF element - extract attributes
				if((attributes != null) && (attributes.Count > 0))
				{
					foreach(string attrName in attributes.Keys)
					{
						if(!attrName.StartsWith(xmlnsPrefix, true, null))
						{
							namespaces.Add(attrName, attributes[attrName]);
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
						sourceText.Clear();
						currentElement = new CIMEntity();
						currentElement.RdfID = StringManipulationManager.ExtractAllAfterSeparator(id,StringManipulationManager.SeparatorSharp);
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

			if (string.Compare(rdfRoot, type, true) != 0)
			{
				//// end of main element
				if (inLevel == Level.Element)
				{
					inLevel = Level.Root;
					if (currentElement != null)
					{
						currentElement.EndLine = lineNumber;
						currentElement.EndColumn = columnNumber + qName.Length + 3;

						ReadToPosition(currentElement.EndLine, currentElement.EndColumn);
						currentElement.Source = sourceText.ToString();
						currentElement.Hash = GetMd5Sum(currentElement.Source);


						if (!string.IsNullOrEmpty(currentElement.MRID))
						{
							model.AddToModelMRID(currentElement);
						}
						else
						{
							model.AddToModelRDFID(currentElement);
						}
						model.AddToModelRDFIDFull(currentElement);
						currentElement = null;
					}
				}
				//// end of an attribute (property)
				else if (inLevel == Level.Property)
				{
					inLevel = Level.Element;

					content = content.Trim();
					if (string.Compare(CIMConstants.AttributeNameIdentifiedObjectMRID, localName) == 0)
					{
						currentElement.MRID = content;
					}
					content = string.Empty;
				}
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
			if (reader != null)
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
		#endregion public: XML Handler methods


		#region private: Helper methods
		private void GoToPosistion(long startLine, int startColumn)
		{
			startColumn = (startColumn > 0) ? startColumn - 1 : 0;
			if (startLine >= lastLine)
			{
				long i = lastLine;
				while (reader.Peek() >= 0)
				{
					string line = reader.ReadLine();
					if (!x && (i != startLine))
					{
						xmlBegining += line;
					}
					lastLine++;
					if (i >= startLine)
					{
						if (!string.IsNullOrEmpty(line))
						{
							if (i == startLine)
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
			if (endLine >= lastLine)
			{
				long i = lastLine;
				while ((reader.Peek() >= 0) && (i <= endLine))
				{
					string line = reader.ReadLine();
					lastLine++;
					if (i == endLine)
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


		private string GetMd5Sum(string str)
		{
			// First we need to convert the string into bytes, which means using a text encoder.
			Encoder enc = System.Text.Encoding.Unicode.GetEncoder();

			// Create a buffer large enough to hold the string
			byte[] unicodeText = new byte[str.Length * 2];
			enc.GetBytes(str.ToCharArray(), 0, str.Length, unicodeText, 0, true);
			
			// Now that we have a byte array we can ask the CSP to hash it
			byte[] result = md5.ComputeHash(unicodeText);

			// Build the final string by converting each byte into hex and appending it to a StringBuilder
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < result.Length; i++)
			{
				sb.Append(result[i].ToString("X2"));
			}

			return sb.ToString();
		}


		#endregion private: Helper methods

	}
}
