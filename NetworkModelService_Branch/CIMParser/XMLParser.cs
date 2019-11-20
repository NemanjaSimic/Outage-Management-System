using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using CIM.Handler;

namespace CIMParser
{
	class XMLParser
	{
		public static IHandler DoParse(IHandler handler, Stream stream, string fileName, out bool success, out TimeSpan durationOfParsing)
		{
			success = true;
			string error = string.Empty;
			durationOfParsing = new TimeSpan(0);
			
			//// for mesuring time needed for processing
			DateTime startTime = DateTime.Now;
			DateTime stopTime;

			try
			{
				XmlReaderSettings settings = new XmlReaderSettings();
				settings.IgnoreComments = true;
				settings.IgnoreWhitespace = true;
				XmlTextReader readerTXT = new XmlTextReader(stream);
				XmlReader reader = XmlReader.Create(readerTXT, settings);
				handler.StartDocument(fileName);
					
				string localName = string.Empty;
				string name = string.Empty;
				bool isEmptyElement = false;
				SortedList<string, string> atts = null;

				while(reader.Read())
				{
					reader.MoveToElement();
					switch(reader.NodeType)
					{
						case XmlNodeType.Element:
							{
								atts = new SortedList<string,string>();
								localName = reader.LocalName;
								name = reader.Name;
								isEmptyElement = reader.IsEmptyElement;
								if (reader.HasAttributes)
								{
									atts = GetAttributes(reader);
								}
								handler.StartElement(localName, name, atts);
								if (isEmptyElement)
								{
									handler.EndElement(localName, name);
								}
								break;
							}
						case XmlNodeType.EndElement:
							{
								handler.EndElement(reader.LocalName, reader.Name);
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
			}
			catch(Exception e)
			{
                throw new Exception(e.Message);
			}

			stopTime = DateTime.Now;
			durationOfParsing = stopTime - startTime;
			return handler;
		}

		private static SortedList<string, string> GetAttributes(XmlReader reader)
		{
			SortedList<string, string> atts = new SortedList<string, string>();
			if(reader.HasAttributes)
			{
				reader.MoveToFirstAttribute();
				do
				{
					if (!atts.ContainsKey(reader.Name))
					{
						atts.Add(reader.Name, reader.Value.ToString());
					}
				} while (reader.MoveToNextAttribute());
			}
			return atts;
		}
	}
}
