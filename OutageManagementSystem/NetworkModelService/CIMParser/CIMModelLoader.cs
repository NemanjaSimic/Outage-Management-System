using CIM.Handler;
using CIM.Model;
using CIM.ModelDifference;
using CIM.Specific;
using System;
using System.IO;

namespace CIMParser
{
    public static class CIMModelLoader
    {
        public static CIMModelLoaderResult LoadCIMXMLModel(Stream stream, string fileName, out CIMModel model, string enumMappingFilePath = "")
        {
            CIMModelLoaderResult result = new CIMModelLoaderResult();
            try
            {
                bool success;
                TimeSpan durationOfParsing = new TimeSpan(0);
                CIMXMLReaderHandler handler = new CIMXMLReaderHandler();

                handler = (CIMXMLReaderHandler)XMLParser.DoParse(handler, stream, fileName, out success, out durationOfParsing);

                if (success)
                {
                    model = handler.Model;
                }
                else
                {
                    result.Report.AppendLine("Loading CIM model was unsuccessful.");
                    model = null;
                    result.Success = false;
                }
            }
            catch (Exception e)
            {
                model = null;
                result.Success = false;
                result.Report.AppendLine("XML FORMAT ERROR! XML FILE: " + Path.GetFileName(fileName) + "\n\t Description: " + e.Message);
            }
            return result;
        }

        public static CIMModelLoaderResult LoadCIMModelForDifference(ref CIMDifference cimDifference)
        {
            CIMModelLoaderResult result = new CIMModelLoaderResult();
            if (cimDifference != null)
            {
                try
                {
                    bool success;
                    TimeSpan durationOfParsing;

                    //// call parsing for begin version:
                    CIMRDFComparerXMLHandler handler = new CIMRDFComparerXMLHandler();
                    handler = (CIMRDFComparerXMLHandler)XMLParsingManager.DoParse(handler, XMLParsingManager.ParsingFileTypes.CIMModelFile, cimDifference.Extract, out success, out durationOfParsing);
                    if (success)
                    {
                        cimDifference.Model = handler.Model;
                        cimDifference.Model.Header = handler.Start;
                        cimDifference.AddRDFNamespaces(handler.Namespaces);
                    }
                    else
                    {
                        throw new Exception("Failed to parse extract RDF. ");
                    }

                    //// call parsing for after version:
                    CIMDifferenceXMLHandler diffHandler = new CIMDifferenceXMLHandler();
                    diffHandler = new CIMDifferenceXMLHandler();
                    diffHandler = (CIMDifferenceXMLHandler)XMLParsingManager.DoParse(diffHandler, XMLParsingManager.ParsingFileTypes.CIMModelFile, cimDifference.Difference, out success, out durationOfParsing);
                    if (success)
                    {
                        cimDifference.Added = diffHandler.Added;
                        cimDifference.Removed = diffHandler.Removed;
                        cimDifference.AddRDFNamespaces(handler.Namespaces);
                    }
                    else
                    {
                        throw new Exception("Failed to parse difference RDF. ");
                    }
                }
                catch (Exception e)
                {
                    result.Report.AppendLine("Error loading CIM/XML model: " + e.Message);
                    result.Success = false;
                }
            }
            else
            {
                result.Report.AppendLine("CIMDifference is null.");
                result.Success = false;
            }

            return result;
        }

        public static void LoadCIMObject(ref CIMObject cimObject, CIMModelContext cimModelContext, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                bool success;
                TimeSpan durationOfParsing;

                CIMRDFObjectXMLHandler handler = new CIMRDFObjectXMLHandler();
                handler.CimModelContext = cimModelContext;
                //// call parsing:
                handler = (CIMRDFObjectXMLHandler)XMLParsingManager.DoParseText(handler, XMLParsingManager.ParsingFileTypes.CIMModelFile, text, out success, out durationOfParsing);

                if (success)
                {
                    cimObject = handler.Object;
                }
            }
        }
    }
}