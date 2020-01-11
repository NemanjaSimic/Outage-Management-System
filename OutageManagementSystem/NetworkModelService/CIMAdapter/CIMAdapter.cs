using CIM.Model;
using CIMParser;
using Outage.DataImporter.CIMAdapter;
using Outage.DataImporter.CIMAdapter.Importer;
using Outage.DataImporter.CIMAdapter.Manager;
using Outage.Common.GDA;
using Outage.Common.ServiceProxies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Outage.Common;

namespace Outage.DataImporter.CIMAdapter
{
    public class CIMAdapterClass
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private ModelResourcesDesc resourcesDesc = new ModelResourcesDesc();
        private TransformAndLoadReport report;

        #region Proxies
        private NetworkModelGDAProxy gdaQueryProxy = null;
        protected NetworkModelGDAProxy GdaQueryProxy
        {
            get
            {
                int numberOfTries = 0;

                while (numberOfTries < 10)
                {
                    try
                    {
                        if (gdaQueryProxy != null)
                        {
                            gdaQueryProxy.Abort();
                            gdaQueryProxy = null;
                        }

                        gdaQueryProxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint);
                        gdaQueryProxy.Open();
                        break;
                    }
                    catch (Exception ex)
                    {
                        string message = $"Exception on NetworkModelGDAProxy initialization. Message: {ex.Message}";
                        Logger.LogError(message, ex);
                        gdaQueryProxy = null;
                    }
                    finally
                    {
                        numberOfTries++;
                        Logger.LogDebug($"CIMAdapterClass: GdaQueryProxy getter, try number: {numberOfTries}.");
                        Thread.Sleep(500);
                    }
                }

                return gdaQueryProxy;
            }
        }
        #endregion

        public CIMAdapterClass()
        {
        }

        public Delta CreateDelta(Stream extract, SupportedProfiles extractType, out string log)
        {
            Delta nmsDelta = null;
            ConcreteModel concreteModel = null;
            Assembly assembly = null;

            string loadLog = string.Empty;
            string transformLog = string.Empty;

            report = new TransformAndLoadReport();

            if (LoadModelFromExtractFile(extract, extractType, ref concreteModel, ref assembly, out loadLog))
            {
                DoTransformAndLoad(assembly, concreteModel, extractType, out nmsDelta, out transformLog);
            }

            log = string.Concat("Load report:\r\n", loadLog, "\r\nTransform report:\r\n", transformLog, "\r\n\r\n");

            return nmsDelta;
        }

        public string ApplyUpdates(Delta delta)
        {
            string updateResult = "Apply Updates Report:\r\n";
            System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            if ((delta != null) && (delta.NumberOfOperations != 0))
            {
                //// NetworkModelService->ApplyUpdates
                using(NetworkModelGDAProxy gdaQueryProxy = GdaQueryProxy)
                {
                    if (gdaQueryProxy != null)
                    {
                        updateResult = gdaQueryProxy.ApplyUpdate(delta).ToString();
                    }
                    else
                    {
                        string message = "NetworkModelGDAProxy is null.";
                        Logger.LogWarn(message);
                        throw new NullReferenceException(message);
                    }
                }
            }

            Thread.CurrentThread.CurrentCulture = culture;
            return updateResult;
        }


        private bool LoadModelFromExtractFile(Stream extract, SupportedProfiles extractType, ref ConcreteModel concreteModelResult, ref Assembly assembly, out string log)
        {
            bool valid = false;
            log = string.Empty;

            System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            try
            {
                ProfileManager.LoadAssembly(extractType, out assembly);
                if (assembly != null)
                {
                    CIMModel cimModel = new CIMModel();
                    CIMModelLoaderResult modelLoadResult = CIMModelLoader.LoadCIMXMLModel(extract, ProfileManager.Namespace, out cimModel);
                    if (modelLoadResult.Success)
                    {
                        concreteModelResult = new ConcreteModel();
                        ConcreteModelBuilder builder = new ConcreteModelBuilder();
                        ConcreteModelBuildingResult modelBuildResult = builder.GenerateModel(cimModel, assembly, ProfileManager.Namespace, ref concreteModelResult);

                        if (modelBuildResult.Success)
                        {
                            valid = true;
                        }
                        log = modelBuildResult.Report.ToString();
                    }
                    else
                    {
                        log = modelLoadResult.Report.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                log = e.Message;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }
            return valid;
        }

        private bool DoTransformAndLoad(Assembly assembly, ConcreteModel concreteModel, SupportedProfiles extractType, out Delta nmsDelta, out string log)
        {
            nmsDelta = null;
            log = string.Empty;

            bool success = false;

            try
            {
                Logger.LogInfo($"Importing {extractType} data...");

                switch (extractType)
                {
                    case SupportedProfiles.Outage:
                        {
                            TransformAndLoadReport report = OutageImporter.Instance.CreateNMSDelta(concreteModel, GdaQueryProxy, resourcesDesc);

                            if (report.Success)
                            {
                                nmsDelta = OutageImporter.Instance.NMSDelta;
                                success = true;
                            }
                            else
                            {
                                success = false;
                            }
                            log = report.Report.ToString();
                            OutageImporter.Instance.Reset();

                            break;
                        }
                    default:
                        {
                            Logger.LogWarn($"Import of {extractType} data is NOT SUPPORTED.");
                            break;
                        }
                }

                return success;
            }
            catch (Exception ex)
            {
                Logger.LogError("Import unsuccessful.", ex);
                return false;
            }
        }
    }
}
