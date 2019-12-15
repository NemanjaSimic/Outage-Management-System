using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace CIMParser
{
    /// <summary>
	/// Class XMLParsingManager is incharge for preparing XML parsing environment 
    /// and performing parsing of the given file.  
    /// <para>@author: Stanislava Selena</para>
    /// </summary>
    static class XMLParsingManager
    {
        /// <summary>
        /// Enumeration of possible file types that can be parsed
        /// </summary>
        public enum ParsingFileTypes 
        {
            ConfigFile = 0, 
            CIMProjectFile, 
            ProfileFile, 
            CIMModelFile, 
            CIMObjectSnippet,
			CIMDifferenceFile
        };
		/// <summary> "xmlns" string </summary>
		public const string xmlnsPrefix = "xmlns";

        /// <summary>
        /// Main method of the class that perfroms XML parsing of file.
        /// </summary>
		/// <param name="handler">the specific XML handler incharge for parsing</param>
        /// <param name="parsingFileType">type of file that will be parsed</param>
        /// <param name="filePath">absolute file path to file that will be parsed</param>
        /// <param name="success">boolean indicator of parsing success (true if there were no errors)</param>
        /// <param name="durationOfParsing">duration of parsing process</param>
        /// <param name="fatalError">null if there was no loading or parsing errors or ValidationData with error message</param>
        /// <returns></returns>
		public static DefaultXMLHandler DoParse(DefaultXMLHandler handler, ParsingFileTypes parsingFileType, Stream stream, out bool success, out TimeSpan durationOfParsing) 
        {
            success = false;
            durationOfParsing = new TimeSpan(0);

			//// for measuring time needed for processing
            DateTime startTime;
            DateTime stopTime;

            if (handler != null)
            {
				startTime = DateTime.Now;
				handler.Stream = new MemoryStream();
				stream.CopyTo(handler.Stream);
				handler.Stream.Position = 0;
				stream.Position = 0;
				handler.StartDocument();

				try
				{	
					XmlReaderSettings settings = new XmlReaderSettings();
					settings.IgnoreComments = true;
					settings.IgnoreWhitespace = true;
					XmlTextReader readerTXT = new XmlTextReader(stream);
					XmlReader reader = XmlReader.Create(readerTXT, settings);

					string localName = string.Empty;
					string name = string.Empty;
					int lineNumber = 0;
					int columnNumber = 0;
					bool isEmptyElement = false;
					int endingPosition = 0;
					SortedList<string, string> atts = null;

					while (reader.Read())
					{
						reader.MoveToElement();
						switch (reader.NodeType)
						{
							case XmlNodeType.Element:
								{
									//// save name and local name, because reading of attributes will move the reader possition
									localName = reader.LocalName;
									name = reader.Name;
									lineNumber = (reader as IXmlLineInfo).LineNumber;
									columnNumber = ((reader as IXmlLineInfo).LinePosition - 2 >= 0) ? ((reader as IXmlLineInfo).LinePosition - 2) : 0;
									isEmptyElement = reader.IsEmptyElement;
									ReadAndPrepareAttributes(handler, reader, ref atts, out endingPosition);
									////START ELEMENT
									handler.StartElement(localName, name, atts, lineNumber, columnNumber);

									if (isEmptyElement)
									{	
										////END ELEMENT
										handler.EndElement(localName, name, (reader as IXmlLineInfo).LineNumber, endingPosition);
									}
									break;
								}
							case XmlNodeType.EndElement:
								{
									////END ELEMENT
									handler.EndElement(reader.LocalName, reader.Name, (reader as IXmlLineInfo).LineNumber, (((reader as IXmlLineInfo).LinePosition - 3 >= 0) ? ((reader as IXmlLineInfo).LinePosition - 3) : 0));
									break;
								}
							case XmlNodeType.Text:
								{
									handler.Characters(reader.Value.ToString());
									break;
								}
						}
					}
					handler.EndDocument();

					success = true;

						
				}
				catch (TargetInvocationException tex)
				{
					Console.WriteLine(tex.StackTrace);
				}
				catch (XmlException xmlEx)
				{
					handler.Error(xmlEx);
					Console.WriteLine(xmlEx.StackTrace);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.StackTrace);
				}
				stopTime = DateTime.Now;
				durationOfParsing = stopTime - startTime;
            }
            return handler;
        }


		public static DefaultXMLHandler DoParseText(DefaultXMLHandler handler, ParsingFileTypes parsingFileType, string text, out bool success, out TimeSpan durationOfParsing)
		{
			success = false;
			durationOfParsing = new TimeSpan(0);
			//// for measuring time needed for processing
			DateTime startTime;
			DateTime stopTime;

			if ((handler != null) && !string.IsNullOrEmpty(text))
			{
				startTime = DateTime.Now;
				handler.StartDocument();
				try
				{
					StringReader sr = new StringReader(text);

					XmlReaderSettings settings = new XmlReaderSettings();
					settings.IgnoreComments = true;
					settings.IgnoreWhitespace = true;
					settings.CloseInput = true;
					XmlTextReader readerTXT = new XmlTextReader(sr);
					XmlReader reader = XmlReader.Create(readerTXT, settings);
					
					string localName = string.Empty;
					string name = string.Empty;
					int lineNumber = 0;
					int columnNumber = 0;
					bool isEmptyElement = false;
					int endingPosition = 0;
					SortedList<string, string> atts = null;

					while (reader.Read())
					{
						reader.MoveToElement();
						switch (reader.NodeType)
						{
							case XmlNodeType.Element:
								{
									//// save name and local name, because reading of attributes will move the reader possition
									localName = reader.LocalName;
									name = reader.Name;
									lineNumber = (reader as IXmlLineInfo).LineNumber;
									columnNumber = ((reader as IXmlLineInfo).LinePosition - 2 >= 0) ? ((reader as IXmlLineInfo).LinePosition - 2) : 0;
									isEmptyElement = reader.IsEmptyElement;
									ReadAndPrepareAttributes(handler, reader, ref atts, out endingPosition);
									handler.StartElement(localName, name, atts, lineNumber, columnNumber);
									if (isEmptyElement)
									{
										handler.EndElement(localName, name, (reader as IXmlLineInfo).LineNumber, endingPosition);
									}
									break;
								}
							case XmlNodeType.EndElement:
								{
									handler.EndElement(reader.LocalName, reader.Name, (reader as IXmlLineInfo).LineNumber, (((reader as IXmlLineInfo).LinePosition - 3 >= 0) ? ((reader as IXmlLineInfo).LinePosition - 3) : 0));
									break;
								}
							case XmlNodeType.Text:
								{
									handler.Characters(reader.Value.ToString());
									break;
								}
						}
					}
					handler.EndDocument();

					success = true;


				}
				catch (TargetInvocationException tex)
				{
					Console.WriteLine(tex.StackTrace);
				}
				catch (XmlException xmlEx)
				{
					handler.Error(xmlEx);
					Console.WriteLine(xmlEx.StackTrace);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.StackTrace);
				}
				stopTime = DateTime.Now;
				durationOfParsing = stopTime - startTime;
			}

			return handler;
		}




		/// <summary>
		/// Method extracts the list of attributes from given XML reader node.
		/// <para>Attributes are given as list of pairs [attName, attvalue].</para>
		/// <para>In case when a XML namespace definition is found, the handler.StartPrefixMapping() method is called.</para>
		/// </summary>
		/// <param name="handler">the XML handler</param>
		/// <param name="reader">curren node of XML reader</param>
		/// <param name="attributes">returns null if reader(node) has no attributes, or list of pairs [attName, attValue]</param>
		/// <param name="currentLinePosition">line position of reader where attributes reading has ended (end of last read attribute)</param>
		private static void ReadAndPrepareAttributes(DefaultXMLHandler handler, XmlReader reader, ref SortedList<string, string> attributes, out int currentLinePosition)
		{
			if (attributes != null)
			{
				attributes.Clear();
			}
			attributes = null;
			currentLinePosition = 0;
			if ((handler != null) && (reader != null))
			{
				currentLinePosition = ((reader as IXmlLineInfo).LinePosition - 2 >= 0) ? (reader as IXmlLineInfo).LinePosition - 2 : 0;
				currentLinePosition += (reader.Name.Length);
				if (reader.HasAttributes)
				{
					attributes = new SortedList<string, string>();
					reader.MoveToFirstAttribute();
					do
					{
						currentLinePosition = ((reader as IXmlLineInfo).LinePosition - 1 >= 0) ? (reader as IXmlLineInfo).LinePosition - 1 : 0;
						currentLinePosition += (reader.Name.Length + reader.Value.Length + 3);
						if (!attributes.ContainsKey(reader.Name))
						{
							attributes.Add(reader.Name, reader.Value.ToString());

							if (string.Compare(reader.Prefix, xmlnsPrefix) == 0)
							{
								handler.StartPrefixMapping(reader.LocalName, reader.Value.ToString());
							}
						}
					} while (reader.MoveToNextAttribute());
				}
			}
		}
	}
}
