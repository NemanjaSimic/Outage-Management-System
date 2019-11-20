using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CIM.ModelCompare;

namespace CIM.ModelDifference
{
	public class CIMDifference
	{
		#region constants
		/// <summary> "http://iec.ch/TC57/2008/CIM-schema-cim13#" </summary>
		public const string CIM13 = "http://iec.ch/TC57/2008/CIM-schema-cim13#";
		/// <summary> "http://iec.ch/TC57/2009/CIM-schema-cim14#" </summary>
		public const string CIM14 = "http://iec.ch/TC57/2009/CIM-schema-cim14#";
		/// <summary> "http://iec.ch/TC57/2010/CIM-schema-cim15#" </summary>
		public const string CIM15 = "http://iec.ch/TC57/2010/CIM-schema-cim15#";

		private const string singleTab = "\t";
		private const string doubleTab = "\t\t";
		private const string tripleTab = "\t\t\t";
		/// <summary> "&lt;" </summary>
		private const string tagOpen = "<";
		/// <summary> "&gt;" </summary>
		private const string tagClose = ">";
		/// <summary> "&lt;/" </summary>
		private const string closingTagOpen = "</";
		/// <summary> "/&gt;" </summary>
		private const string closingTagClose = "/>";
		/// <summary> "\"" </summary>
		private const string quote = "\"";
		/// <summary> "\"&gt;" </summary>
		private const string quoteAndCloseTag = "\">";
		/// <summary> "=" </summary>
		private const string equalAndQuote = "=\"";
		/// <summary> " rdf:resource=\"" </summary>
		private const string rdfResource = " rdf:resource=\"";
		/// <summary> " rdf:ID=\"" </summary>
		private const string rdfID = " rdf:ID=\"";
		/// <summary> "&lt;!-- " </summary>
		private const string commentBegin = "<!-- ";
		/// <summary> " --&gt;" </summary>
		private const string commentEnd = " -->";
		private const byte dummy = 0;
		#endregion constants

		private string extractDescription = string.Empty;
        private Stream extract = null;
        private Stream difference = null;
		private CIMModelSets model = new CIMModelSets();
		private List<CIMEntity> added = new List<CIMEntity>();
		private List<CIMEntity> removed = new List<CIMEntity>();
		private Dictionary<string, string> namespaces = new Dictionary<string, string>();

        public Stream Extract 
        {
            get 
            {
                return extract;
            }
            set 
            {
                extract = value;
            }
        }

        public Stream Difference
        {
            get
            {
                return difference;
            }
            set
            {
                difference = value;
            }
        }

		public Dictionary<string, string> Namespaces
		{
			get
			{
				return namespaces;
			}
			set
			{
				namespaces = value;
			}
		}

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

		internal CIMModelSets Model
		{
			get
			{
				return model;
			}
			set
			{
				model = value;
			}
		}

		public string ExtractDescription
		{
			get
			{
				return extractDescription;
			}
			set
			{
				extractDescription = value;
			}
		}

		public Stream ApplyDifference(ref Stream stream)
		{
			StringBuilder duplicateErrors = new StringBuilder();
			foreach(CIMEntity entity in removed)
			{
                if (model.ModelRDFIDFull.ContainsKey(entity.RdfID))
                {
                    // Removing whole entity from map
                    if (!entity.Source.Contains("rdf:Description"))
                    {
                        model.ModelRDFIDFull.Remove(entity.RdfID);
                    }
                    else
                    {
                        // Removing property from entity
                        CIMEntity changed = model.ModelRDFIDFull[entity.RdfID];
                        string[] parts = entity.Source.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string part in parts)
                        {
                            if (changed.Source.Contains(part))
                            {
                                int start = changed.Source.IndexOf(part);
                                changed.Source = changed.Source.Remove(start, part.Length);
                            }
                        }
                    }
                }
			}
			foreach(CIMEntity entity in added)
			{
                
				// Add whole entity to map
				if(!entity.Source.Contains("rdf:Description"))
				{
					if (!model.ModelRDFIDFull.ContainsKey(entity.RdfID))
					{
						model.ModelRDFIDFull.Add(entity.RdfID, entity);
					}
					else
					{
						
						if (duplicateErrors.Length == 0)
						{
							duplicateErrors.Append("Patch adds element with ID that is already in model.");
						}

						duplicateErrors.AppendLine("Duplicate rdf ID: " + entity.RdfID);
					}
				}
				else
				{
					// Add only a property
					if(model.ModelRDFIDFull.ContainsKey(entity.RdfID))
					{
						CIMEntity changed = model.ModelRDFIDFull[entity.RdfID];

						string[] parts = entity.Source.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
						string[] finalParts = changed.Source.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
						string lastLine = finalParts[finalParts.Length - 1];
						StringBuilder newSource = new StringBuilder();
						for(int i = 0; i < finalParts.Length - 1; i++)
						{
							newSource.AppendLine(finalParts[i]);
						}
						foreach(string part in parts)
						{
							if(!part.Contains("rdf:Description"))
							{
								newSource.AppendLine(part);
							}
						}
						newSource.AppendLine(lastLine);
						changed.Source = newSource.ToString();
					}
				}
			}

			if (duplicateErrors.Length > 0)
			{
				throw new Exception(duplicateErrors.ToString());
			}

			SaveDifference(ref stream);
			return stream;
		}

		private void SaveDifference(ref Stream stream)
		{
			StringBuilder text = new StringBuilder();
			StreamWriter streamWriter = new StreamWriter(stream);
			
			try
			{
				streamWriter.Write(BuildDefaultCIMRDFDocumentHeaderSnippet());
				streamWriter.Write(BuildXMLComment("generated by: 'CIM Importer' application"));

				foreach(KeyValuePair<string,CIMEntity> pair in model.ModelRDFIDFull)
				{
					if(!string.IsNullOrEmpty(pair.Key))
					{
						streamWriter.WriteLine("\r\n" + pair.Value.Source.Trim());
					}
				}
				streamWriter.Write(BuildCIMRDFDocumentFooter());
			}
			catch(Exception)
			{
				throw;
			}
			finally
			{
				streamWriter.Close();
			}
		}

        public CIMDifference(string extractDescription, Stream extract, Stream difference)
        {
            this.extractDescription = extractDescription;
            this.extract = extract;
            this.difference = difference;
        }

		#region public static: Prepare XML text for print
		
		public string BuildDefaultCIMRDFDocumentHeaderSnippet()
		{
			StringBuilder headerBuilder = new StringBuilder();
			headerBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			headerBuilder.Append("<rdf:RDF");
			foreach(KeyValuePair<string, string> pair in Namespaces)
			{
				headerBuilder.AppendFormat("\n\txmlns:{0}=\"{1}\"", pair.Key, pair.Value);
			}
			headerBuilder.AppendLine(">");
			return headerBuilder.ToString();
		}

		/// <summary>
		/// Method returns footer for CIM/RDF document i.e. closing rdf tag.
		/// </summary>
		/// <returns>footer of RDF document, ie. "/rdf:RDF"</returns>
		public static string BuildCIMRDFDocumentFooter()
		{
			return "</rdf:RDF>";
		}

		//// Method returns xml comment paragraph.        
		//// returns : string - xml comment paragraph with given text
		private static string BuildXMLComment(string comment)
		{
			StringBuilder commentBuilder = new StringBuilder();
			commentBuilder.Append(commentBegin);

			string subString = comment;
			int maxLength = 150;
			while(subString.Length > maxLength)
			{
				subString = subString.Substring(0, maxLength);
				commentBuilder.AppendLine(subString);
			}
			commentBuilder.Append(subString);
			commentBuilder.AppendLine(commentEnd);
			return commentBuilder.ToString();
		}

		#endregion public static: Prepare XML text for print

		internal void AddRDFNamespaces(Dictionary<string, string> namespaces)
		{
			foreach (KeyValuePair<string, string> pair in namespaces)
			{
				string prefixAndNamespace = pair.Key;
				string uri = pair.Value;
				if (!string.IsNullOrEmpty(prefixAndNamespace) && !string.IsNullOrEmpty(uri))
				{
					if(!Namespaces.ContainsKey(prefixAndNamespace))
					{
						Namespaces.Add(prefixAndNamespace, uri);
					}
					else
					{
						Namespaces[prefixAndNamespace] = uri;
					}
				}
			}
		}
	}
}
