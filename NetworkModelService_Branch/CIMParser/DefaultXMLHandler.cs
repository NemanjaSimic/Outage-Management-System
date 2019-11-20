using System;
using System.Collections.Generic;
using System.IO;

namespace CIMParser
{
	/// <summary>
	/// Class DefaultXMLHandler represents an abstract XML parsing handler.
	/// <para>Class is used for generalization of handlers needed in XMLParsingManager class.</para>
	/// <remarks>See also:<seealso cref="T:XMLParsingManager"/></remarks>
	/// <para>@author: Stanislava Selena</para>
	/// </summary>
	abstract class DefaultXMLHandler
	{
		protected XMLParseError occurredError = null;
		protected string filePath;
		protected Stream stream = null;

		/// <summary>
		/// Gets the eventual parsing error from handler or null if no error occured.
		/// </summary>
		public XMLParseError OccurredError
		{
			get { return occurredError; }
		}

		/// <summary>
		/// Gets and sets the file directory of file parsed by handler.
		/// </summary>
		public string FilePath
		{
			set { filePath = value; }
			get { return filePath; }
		}

		public Stream Stream
		{
			get { return stream; }
			set { stream = value; }
		}

		public abstract void StartDocument();
		public abstract void StartElement(string localName, string qName, SortedList<string, string> attributes, int lineNumber = 0, int columnNumber = 0);
		public abstract void EndElement(string localName, string qName, int lineNumber = 0, int columnNumber = 0);
		public abstract void Characters(string text);
		/// <summary>
		///  Method should process occurrence of XML namespace definition.
		/// </summary>
		/// <param name="prefix">prefix of a namspace (e.g. "cim")</param>
		/// <param name="uri">URI of the namespace</param>
		public abstract void StartPrefixMapping(string prefix, string uri);
		public abstract void EndDocument();

		public abstract void FatalError(Exception error);
		public abstract void Error(Exception error);
		public abstract void Warning(Exception error);
	}
}
