using System;
using System.IO;
using System.Text;

namespace CIM.Manager
{
    /// <summary>
    /// FileManager manages all file system operations during application's execusion.
    /// <para>@author: Stanislava Selena</para>
    /// </summary>
    static class FileManager
    {
        /// <summary> ".rdf" </summary>
        public const string ExtensionRDF = ".rdf";
        /// <summary> ".txt" </summary>
        public const string ExtensionTXT = ".txt";
        /// <summary> ".svg" </summary>
        public const string ExtensionSVG = ".svg";
        /// <summary> ".xml" </summary>
        public const string ExtensionXML = ".xml";
        /// <summary> ".cimws" </summary>
        public const string ExtensionWorkspace = ".cimws";
        /// <summary> ".cimproj" </summary>
        public const string ExtensionCIMProject = ".cimproj";
        /// <summary> "" </summary>
        public const string ExtensionNone = "";

        /// <summary> File path to the application's directory for saving configuration files and other output. </summary>
        public static readonly string DefaultDirectory = @"C:\";

        /// <summary> Directory path to the default open file location. </summary>
        public static string DefaultOpeningDirectoryPath = DefaultDirectory;
        /// <summary> Directory path to the default save file location. </summary>
        public static string DefaultSavingDirectoryPath = DefaultDirectory;
        /// <summary> Directory path to the default workspace location. </summary> 
        public static string DefaultWorkspaceDirectoryPath = DefaultDirectory + "\\workspace";


        static FileManager()
        {
            
        }


        /// <summary>
        /// Method reads the file size for given file path. Size is given in MB.
        /// <para>If given file was not found, method returns 0.</para>
        /// </summary>
        /// <param name="pathString">full file path</param>
        /// <returns></returns>
        public static double ReadFileSizeInMBForFile(string pathString)
        {
            double fileSize = 0;
            if (!string.IsNullOrEmpty(pathString))
            {
                try
                {
                    FileInfo info = new FileInfo(pathString);
                    if ((info != null) && (info.Exists))
                    {
                        fileSize = (double)info.Length / (1024 * 1024);
                        fileSize = Math.Round(fileSize, 3);
                    }
                }
                catch (Exception)
                {
                }
            }
            return fileSize;
        }

        /// <summary>
        /// Method reads time of last modification of file.
        /// <para>If given file was not found, method returns null value.</para>
        /// </summary>
        /// <param name="pathString">full file path</param>
        /// <returns></returns>
        public static DateTime? ReadLastModificationTimeForFile(string pathString)
        {
            DateTime? modifiedAt = null;
            if (!string.IsNullOrEmpty(pathString))
            {
                try
                {
                    FileInfo info = new FileInfo(pathString);
                    if ((info != null) && (info.Exists))
                    {
                        modifiedAt = info.LastWriteTime;
                    }
                }
                catch (Exception)
                {
                }
            }
            return modifiedAt;
        }

        /// <summary>
        /// Method compares given time with time of last modification of file.
        /// <para>If both times are the same, method returns true, otherwise it returns false.</para>
        /// </summary>
        /// <param name="time">time for comparing</param>
        /// <param name="pathString">full file path</param>
        /// <returns>true or false</returns>
        public static bool CompareTimeWithLastModificationTimeForFile(DateTime? time, string pathString)
        {
            bool isEqual = false;
            DateTime? modifiedAt = ReadLastModificationTimeForFile(pathString);
            isEqual = CompareTimes(time, modifiedAt);
            return isEqual;
        }

        public static bool CompareTimes(DateTime? time1, DateTime? time2)
        {
            bool isEqual = false;           
            if (!time1.HasValue && !time2.HasValue)
            {
                isEqual = true;
            }
            else if (time1.HasValue && time2.HasValue)
            {
                isEqual = (DateTime.Compare(time1.Value, time2.Value) == 0);
            }
            return isEqual;
        }

        /// <summary>
        /// Method reads the text segment from given file.
        /// </summary>
        /// <param name="textFilePath">full file path to the text file</param>
        /// <param name="startLine">(1 based)index of line in text from which to start reading</param>
        /// <param name="startColumn">(1 based)index of column in startLine from which to start reading</param>
        /// <param name="endLine">(1 based)index of line in text from which to stop reading</param>
        /// <param name="endColumn">(1 based)index of column in endLine from which to stop reading</param>
        /// <returns></returns>
        public static string ReadTextSegmentFromFile(string textFilePath, long startLine, int startColumn, long endLine, int endColumn)
        {
            StringBuilder segment = new StringBuilder(string.Empty);

            if (System.IO.File.Exists(textFilePath))
            {
                using (StreamReader sr = new StreamReader(textFilePath))
                {
                    long i = 1;
                    while ((sr.Peek() >= 0) && (i <= endLine))
                    {
                        string line = sr.ReadLine();
                        if ((i >= startLine) && (i <= endLine))
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                if (i == startLine)
                                {
                                    segment.AppendLine((line.Substring(startColumn - 1)));
                                }
                                else if (i == endLine)
                                {
                                    segment.AppendLine((line.Substring(0, endColumn)).Trim());
                                }
                                else
                                {
                                    segment.AppendLine(line);
                                }
                            }
                        }
                        i++;
                    }
                }
            }
            return segment.ToString();
        }

        /// <summary>
        /// Method reads encoding of given XML file.
        /// <para>If no encoding definition is found, method will return the "utf-8" encoding.</para>
        /// </summary>
        /// <param name="xmlFilePath">full file path to the XML file</param>
        /// <returns>string with encoding name</returns>
        public static string ReadXMLFileEncoding(string xmlFilePath)
        {
            string encoding = "utf-8";
            string procInstruction = "<?";
            string xml = "xml";
            string encodingTitle = "encoding";
            string simbolEqual = "=";
            string simbolQuote = "\"";
            if (System.IO.File.Exists(xmlFilePath))
            {                       
                using (StreamReader sr = new StreamReader(xmlFilePath))
                {                    
                    int i = 0;
                    while ((sr.Peek() >= 0) && (i < 20))
                    {
                        string line = sr.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            line = line.Trim();
                            if (line.Contains(procInstruction))
                            {
                                line = line.Substring(line.IndexOf(procInstruction) + 2).Trim();
                                if (line.StartsWith(xml))
                                {
                                    line = line.Substring(4);
                                    if (line.Contains(encodingTitle))
                                    {
                                        line = line.Substring(line.IndexOf(encodingTitle) + 8).Trim();
                                        if (line.StartsWith(simbolEqual) && line.Contains(simbolQuote))
                                        {
                                            line = line.Substring(line.IndexOf(simbolQuote) + 1);
                                            if (line.Contains(simbolQuote))
                                            {
                                                line = line.Substring(0, line.IndexOf(simbolQuote));
                                                encoding = line;
                                            }
                                        }
                                    }
                                    break;
                                }
                            };
                        };
                    }
                }
            }
            return encoding.Trim();
        }
        
    }
}
