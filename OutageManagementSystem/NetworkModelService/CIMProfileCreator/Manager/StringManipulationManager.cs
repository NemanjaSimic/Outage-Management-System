using System;
using System.Text;

namespace FTN.ESI.SIMES.CIM.Manager
{
    /// <summary>
    /// StringManipulationManager contains methods for basic string manipulation and formating
    /// needed by application.
    /// </summary>
    class StringManipulationManager
    {        
        /// <summary> " " </summary>
        public const string SeparatorSpace = " ";
        /// <summary> "#" </summary>
        public const string SeparatorSharp = "#";
        /// <summary> "." </summary>
        public const string SeparatorDot = ".";
        /// <summary> ":" </summary>
        public const string SeparatorColon = ":";
        /// <summary> "/" </summary>
        public const string SeparatorSlash = "/";
        /// <summary> "\\" </summary>
        public const string SeparatorBackslash = "\\";
        /// <summary> "\n" </summary>
        public const string SeparatorNewLine = "\n";
        /// <summary> '\n' </summary>
        public const char SeparatorNewLineAsChar = '\n';
        /// <summary> "&" </summary>
        public const string SymbolAmpersand = "&";
        /// <summary> "&amp;" </summary>
        public const string SymbolAmpersandInXML = "&amp;";
        /// <summary> less-than character </summary>
        public const string SymbolLessThan = "<";
        /// <summary> less-than character in XML "&lt;" </summary>
        public const string SymbolLessThanInXML = "&lt;";
        /// <summary> greater-than character </summary>
        public const string SymbolGreaterThan = ">";
        /// <summary> greater-than character in XML "&gt;" </summary>
        public const string SymbolGreaterThanInXML = "&gt;";        
        /// <summary> minus "-" character </summary>
        public const string SeparatorMinus = "-";

        /// <summary>
        /// Method extracts the shortest fullName for given full fullName and separator string.
        /// </summary>
        /// <param fullName="fullName">full fullName</param>
        /// <param fullName="separator">separator character or string used in full fullName</param>
        /// <returns>shortest fullName ie. substring after last separator in full fullName</returns>
        public static string ExtractShortestName(string fullName, string separator)
        {
            string extarcted = fullName;
            if ((extarcted != null) && (extarcted.Length > 0))
            {
                int index = extarcted.LastIndexOf(separator);
                if (index > -1)
                {
                    extarcted = extarcted.Substring(index + 1);
                }
            }
            return extarcted;
        }

        /// <summary>
        /// Method extracts everithing after firts occurence of given separator string in given fullName.
        /// </summary>
        /// <param fullName="fullName">full fullName</param>
        /// <param fullName="separator">separator character or string used in full fullName</param>
        /// <returns>shortest fullName ie. substring after last separator in full fullName</returns>
        public static string ExtractAllAfterSeparator(string fullName, string separator)
        {
            string extarcted = fullName;
            if ((extarcted != null) && (extarcted.Length > 0))
            {
                int index = extarcted.IndexOf(separator);
                if ((index > -1) && (extarcted.Length > (index + 1)))
                {
                    extarcted = extarcted.Substring(index + 1);
                }
            }
            return extarcted;
        }

        /// <summary>
        /// If fullName parameter contains given separator, method will trim everithing begining from the
        /// last occurrence of that separator.
        /// </summary>
        /// <param fullName="fullName">full fullName</param>
        /// <param fullName="separator">separator character or string used in full fullName</param>
        /// <returns>trimed full fullName</returns>
        public static string TrimAfterLastSeparator(string fullName, string separator)
        {
            string extarcted = fullName;
            if ((extarcted != null) && (extarcted.Length > 0))
            {
                int index = extarcted.LastIndexOf(separator);
                if (index > 0)
                {
                    extarcted = extarcted.Substring(0, index);
                }
            }
            return extarcted;
        }

        /// <summary>
        /// Formats DateTime object into "yyyy-MM-dd_HH.mm.ss" string
        /// </summary>
        /// <param fullName="dateTime"></param>
        /// <returns>formated dateTime string</returns>
        public static string FormatDateTimeAsFileNameAdequate(DateTime dateTime)
        {
            string formatedDateTime = string.Empty;
            if (dateTime != null)
            {
                formatedDateTime = string.Format("{0:yyyy-MM-dd_HH.mm.ss}", dateTime);
            }
            
            return formatedDateTime;
        }

        /// <summary>
        /// Formats DateTime object into "yyyy-MM-dd HH:mm:ss" string
        /// </summary>
        /// <param fullName="dateTime"></param>
        /// <returns>formated dateTime string</returns>
        public static string FormatDateTimeAsSortable(DateTime dateTime)
        {
            string formatedDateTime = string.Empty;
            if (dateTime != null)
            {
                formatedDateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dateTime);
            }

            return formatedDateTime;
        }

        /// <summary>
        /// Formats DateTime object into "yyyy-MM-dd HH:mm:ss.FFFF" string
        /// </summary>
        /// <param fullName="dateTime"></param>
        /// <returns>formated dateTime string</returns>
        public static string FormatDateTimeAsSortableWithFraction(DateTime dateTime)
        {
            string formatedDateTime = string.Empty;
            if (dateTime != null)
            {
                formatedDateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss.FFFF}", dateTime);
            }

            return formatedDateTime;
        }

        /// <summary>
        /// Formats DateTime string in format "HH:mm:ss" (ONLY) into string array {HH,mm,ss}
        /// </summary>
        /// <param fullName="dateTime"></param>
        /// <returns>formated dateTime string</returns>
        public static string[] SplitStringForDateInitialization_TimeOnly(string time)
        {
            string[] temp = time.Split(':');
            for(int i =0; i<temp.Length; i++)
            {
                while(temp[i][0] == '0' && temp[i].Length > 1)
                    temp[i] = temp[i].Remove(0, 1);
            }
            return temp;
        }

        /// <summary>
        /// Method recognises diferent words in given string if every word begin with an uppercase,
        /// <para>and build new string with those words separeted with given separator string.</para>
        /// <remarks>Example: 
        /// <para>input = "TheStringForTokenization", separator = "."</para>
        /// <para> output = "The.String.For.Tokenization"</para>
        /// </remarks>
        /// </summary>
        /// <param fullName="input">input string</param>
        /// <param fullName="separator">separator</param>
        /// <returns></returns>
        public static string TokenizeStringByUpercase(string input, string separator)
        {
            StringBuilder output = new StringBuilder();
            if (input != null)
            {
                char[] charArray = input.ToCharArray();
                foreach (char character in charArray)
                {
                    if (char.IsUpper(character))
                    {
                        output.Append(" ");
                    }
                    output.Append(character);
                }                
            }
            return output.ToString().Trim();
        }

        /// <summary>
        /// Method creates new string from given value by replacing all special simbols with
        /// character entities: &amp; &lt; &gt;
        /// </summary>
        /// <param name="value">string for adjusting</param>
        /// <returns>XML adjusted string</returns>
        public static string PrepareStringForXMLFormat(string value)
        {
            string xmlValue = value;
            if (!string.IsNullOrEmpty(value))
            {
                xmlValue = xmlValue.Replace(SymbolAmpersand, SymbolAmpersandInXML);
                xmlValue = xmlValue.Replace(SymbolGreaterThan, SymbolGreaterThanInXML);
                xmlValue = xmlValue.Replace(SymbolLessThan, SymbolLessThanInXML);
            }
            return xmlValue;
        }

        /// <summary>
        /// Method replaces '-', '/' and 'º' characters with "MINUS", "_" and "Deg"
        /// If the string is URI then all characters before "." will be removed
        /// </summary>
        /// <param name="val">string for adjusting</param>
        /// <returns>adjusted string</returns>
        public static string ReplaceInvalidEnumerationCharacters(string val)
        {
            val = ExtractAllAfterSeparator(val, SeparatorSharp);
            val = ExtractAllAfterSeparator(val, SeparatorDot);
            val = val.Replace("-", "MINUS");
            val = val.Replace("/", "_");
            val = val.Replace("º", "Deg");
            return val;
        }

        /// <summary>
        /// Method extracts type from long name of List type e.g. for string "System.Collenctions.Generic.List[System.Double]" returning string would be "System.Double"
        /// </summary>
        /// <param name="value">string from which to extract</param>
        /// <returns>extracted string</returns>
        public static string ExtractTypeOfList(string value)
        {
            value = value.Substring(value.IndexOf('[')+1);
            value = value.Remove(value.IndexOf(']'));
            return value;
        }

		/// <summary>
		/// Method capitalizes the first letter of the given string value
		/// </summary>
		/// <param name="value">string that will be processed</param>
		/// <returns>string with capitalized first letter</returns>
		public static string CreateHungarianNotation(string value)
		{
			if(string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}
			char[] a = value.ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}
    }
}
