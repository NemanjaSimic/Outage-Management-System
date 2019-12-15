using System;
using System.Xml;

namespace CIMParser
{
	/// <summary>
	/// Class XMLParseError is used for reporting errors occured during running
	/// of some specific DefaultXMLHandler.</para>
	/// <para>@author: Stanislava Selena</para>
	/// </summary>
	class XMLParseError
	{
		private Exception occurredException = null;
        private string message = string.Empty;
		private int lineNumber;
		private int columnNumber;


        public XMLParseError() : base()
        {
        }

        public XMLParseError(Exception occurredException) : base()
        {
            this.occurredException = occurredException;
			InitXMLParseError();
        }

		/// <summary>
		/// Gets the Exception of parse error.
		/// </summary>
		public Exception OccurredException
        {
            get
            {  
                return occurredException;
            }
        }

		/// <summary>
		/// Gets the string message describing this parse error.
		/// </summary>
        public string Message
        {
            get
            {   
                return message;
            }
        }

		/// <summary>
		/// Gets the line number of XML file where this parse error has occurred.
		/// </summary>
        public long LineNumber
        {
            get
            {
				return lineNumber;
            }
        }

		/// <summary>
		/// Gets the column number of XML file where this parse error has occurred.
		/// </summary>
        public long ColumnNumber
        {
			get
			{
				return columnNumber;
			}
        }


		private void InitXMLParseError()
		{
			if (occurredException != null)
			{
				message = occurredException.Message;
				if (occurredException is XmlException)
				{	
					lineNumber = ((XmlException)occurredException).LineNumber;
					columnNumber = ((XmlException)occurredException).LinePosition;
				}
				else
				{
					lineNumber = 0;
					columnNumber = 0;
				}
			}
		}
	}
}
