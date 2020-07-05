using CIM.Model;
using CIMParser;
using Outage.DataImporter.CIMAdapter.Importer;
using Outage.DataImporter.CIMAdapter.Manager;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

using System.Threading.Tasks;
using System.Text;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.NMS;
using OMS.Common.NmsContracts;

namespace Outage.DataImporter.CIMAdapter
{
    public struct ConditionalValue<TValue>
    { 
        public ConditionalValue(bool hasValue, TValue value)
        {
            HasValue = hasValue;
            Value = value;
        }

        public bool HasValue { get; private set; }

        public TValue Value { get; private set; }
    }


    public class CIMAdapterClass
    {
        private readonly ModelResourcesDesc resourcesDesc;
        private TransformAndLoadReport report;

        #region Private Properties
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public CIMAdapterClass()
        {
            resourcesDesc = new ModelResourcesDesc();
        }

        public async Task<ConditionalValue<Delta>> CreateDelta(Stream extract, SupportedProfiles extractType, StringBuilder logBuilder)
        {
            ConditionalValue<Delta> result;
            ConcreteModel concreteModel = null;
            Assembly assembly = null;

            report = new TransformAndLoadReport();

            string loadLog;

            if (LoadModelFromExtractFile(extract, extractType, ref concreteModel, ref assembly, out loadLog))
            {
                logBuilder.AppendLine($"Load report:\r\n{loadLog}");
                result = await DoTransformAndLoad(assembly, concreteModel, extractType, logBuilder);
            }
            else
            {
                result = new ConditionalValue<Delta>(false, null);
            }

            return result;
        }

        public async Task<string> ApplyUpdates(Delta delta)
        {
            string updateResult = "Apply Updates Report:\r\n";
            System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            if ((delta != null) && (delta.NumberOfOperations != 0))
            {
                INetworkModelGDAContract nmsGdaClient = NetworkModelGdaClient.CreateClient();

                if (nmsGdaClient == null)
                {
                    string message = "NetworkModelGdaClient is null.";
                    Logger.LogWarning(message);
                    throw new NullReferenceException(message);
                }
                    
                var result = await nmsGdaClient.ApplyUpdate(delta);
                updateResult = result.ToString();
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

        private async Task<ConditionalValue<Delta>> DoTransformAndLoad(Assembly assembly, ConcreteModel concreteModel, SupportedProfiles extractType, StringBuilder logBuilder)
        {
            Delta nmsDelta;
            bool success;

            try
            {
                Logger.LogInformation($"Importing {extractType} data...");

                if(extractType != SupportedProfiles.Outage)
                {
                    Logger.LogWarning($"Import of {extractType} data is NOT SUPPORTED.");
                }

                TransformAndLoadReport report = await OutageImporter.Instance.CreateNMSDelta(concreteModel, resourcesDesc);
                success = report.Success;

                nmsDelta = success ? OutageImporter.Instance.NMSDelta : null;

                logBuilder.Append(report.Report.ToString());
                OutageImporter.Instance.Reset();
            }
            catch (Exception ex)
            {
                success = false;
                nmsDelta = null;
                Logger.LogError("Import unsuccessful.", ex);
            }
            
            return new ConditionalValue<Delta>(success, nmsDelta);
        }
    }
}
