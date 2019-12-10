using CIM.Model;
using CIMParser;
using Outage.DataImporter.CIMAdapter;
using Outage.DataImporter.CIMAdapter.Importer;
using Outage.DataImporter.CIMAdapter.Manager;
using Outage.Common.GDA;
using Outage.ServiceContracts;
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
        private NetworkModelGDAProxy gdaQueryProxy = null;
        private ModelResourcesDesc resourcesDesc = new ModelResourcesDesc();
        private TransformAndLoadReport report;

        public CIMAdapterClass()
        {
        }

        private NetworkModelGDAProxy GdaQueryProxy
        {
            get
            {
                if (gdaQueryProxy != null)
                {
                    gdaQueryProxy.Abort();
                    gdaQueryProxy = null;
                }

                gdaQueryProxy = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");
                gdaQueryProxy.Open();

                return gdaQueryProxy;
            }
        }

        public Delta CreateDelta(Stream extract, SupportedProfiles extractType, DeltaOpType deltaOpType, out string log)
        {
            Delta nmsDelta = null;
            Dictionary<string, ResourceDescription> entities = null;
            ConcreteModel concreteModel = null;
            Assembly assembly = null;

            string loadLog = string.Empty;
            string transformLog = string.Empty;
            string correctionLog = string.Empty;

            report = new TransformAndLoadReport();

            if (LoadModelFromExtractFile(extract, extractType, ref concreteModel, ref assembly, out loadLog))
            {
                if(DoTransformAndLoad(assembly, concreteModel, extractType, deltaOpType, out nmsDelta, out entities, out transformLog))
                {
                    CorrectNmsDelta(entities, ref nmsDelta, out correctionLog);
                }
            }
            log = string.Concat("Load report:\r\n", loadLog, "\r\nTransform report:\r\n", "\r\n\tOperation: ", deltaOpType, "\r\n\r\n", transformLog, "\r\n\r\nCorrection report:\r\n", correctionLog);

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
                updateResult = GdaQueryProxy.ApplyUpdate(delta).ToString();
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

        private bool DoTransformAndLoad(Assembly assembly, ConcreteModel concreteModel, SupportedProfiles extractType, DeltaOpType deltaOpType, out Delta nmsDelta, out Dictionary<string, ResourceDescription> entities, out string log)
        {
            nmsDelta = null;
            entities = new Dictionary<string, ResourceDescription>();
            log = string.Empty;
            bool success = false;

            try
            {
                LogManager.Log(string.Format("Importing {0} data...", extractType), LogLevel.Info);

                switch (extractType)
                {
                    case SupportedProfiles.Outage:
                        {
                            TransformAndLoadReport report = OutageImporter.Instance.CreateNMSDelta(concreteModel, deltaOpType);

                            if (report.Success)
                            {
                                nmsDelta = OutageImporter.Instance.NMSDelta;
                                entities = OutageImporter.Instance.Entities;
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
                            LogManager.Log(string.Format("Import of {0} data is NOT SUPPORTED.", extractType), LogLevel.Warning);
                            break;
                        }
                }

                return success;
            }
            catch (Exception ex)
            {
                LogManager.Log(string.Format("Import unsuccessful: {0}", ex.StackTrace), LogLevel.Error);
                return false;
            }
        }

        private bool CorrectNmsDelta(Dictionary<string, ResourceDescription> entities, ref Delta nmsDelta, out string log)
        {
            log = string.Empty;
            bool success = false;

            Dictionary<long, long> negativeToPositiveGidMap = new Dictionary<long, long>(entities.Keys.Count);

            //TODO: initialize at the same time as entities (mozda neg gid, rd)
            foreach (string mrid in entities.Keys)
            {
                negativeToPositiveGidMap.Add(entities[mrid].Id, entities[mrid].Id);
            }

            HashSet<ModelCode> requiredEntityTypes = new HashSet<ModelCode>();
             
            try
            {
                foreach(ResourceDescription rd in nmsDelta.UpdateOperations)
                {
                    ModelCode mc = resourcesDesc.GetModelCodeFromId(rd.Id);
                    if (!requiredEntityTypes.Contains(mc))
                    {
                        requiredEntityTypes.Add(mc);
                    }
                }

                foreach (ResourceDescription rd in nmsDelta.DeleteOperations)
                {
                    ModelCode mc = resourcesDesc.GetModelCodeFromId(rd.Id);
                    if (!requiredEntityTypes.Contains(mc))
                    {
                        requiredEntityTypes.Add(mc);
                    }
                }

                List<ModelCode> mrIdProp = new List<ModelCode>() { ModelCode.IDOBJ_MRID, ModelCode.IDOBJ_GID, ModelCode.IDOBJ_NAME, ModelCode.IDOBJ_DESCRIPTION };
                foreach(ModelCode mc in requiredEntityTypes)
                {
                    int index = GdaQueryProxy.GetExtentValues(mc, mrIdProp);
                    int resourceCount = GdaQueryProxy.IteratorResourcesLeft(index);
                    List<ResourceDescription> gdaResult = GdaQueryProxy.IteratorNext(resourceCount, index);

                    foreach(ResourceDescription rd in gdaResult)
                    {
                        foreach(Property prop in rd.Properties)
                        {
                            if (prop.Id != ModelCode.IDOBJ_MRID)
                            {
                                continue;
                            }

                            string mrId = prop.PropertyValue.StringValue;
                            if(entities.ContainsKey(mrId))
                            {
                                //swap negative gid for positive gid from server (NMS) 
                                negativeToPositiveGidMap[entities[mrId].Id] = rd.Id;
                                entities[mrId].Id = rd.Id;
                            }

                            if(prop.Id == ModelCode.IDOBJ_MRID)
                            {
                                break;
                            }
                        }
                    }


                    GdaQueryProxy.IteratorClose(index);
                }

                foreach (long negGid in negativeToPositiveGidMap.Keys)
                {
                    long positiveGid = negativeToPositiveGidMap[negGid];

                    ModelCode mc = resourcesDesc.GetModelCodeFromId(positiveGid);
                    //report.Report.Append(mc).Append(" mrId = ").Append(mrId).Append(" ID = ").Append(string.Format("0x{0:X16}", negGid)).Append(" after correction is GID = ").AppendLine(string.Format("0x{0:X16}", positiveGid));
                    report.Report.Append(mc)
                                 .Append(" ID = ")
                                 .Append(string.Format("0x{0:X16}", negGid))
                                 .Append(" after correction is GID = ")
                                 .AppendLine(string.Format("0x{0:X16}", positiveGid));
                }



                log = report.Report.ToString();
                LogManager.Log(log, LogLevel.Info);
                return success;
            }
            catch (Exception ex)
            {
                log = string.Format("Correction of delta unsuccessful: {0}", ex.StackTrace);
                LogManager.Log(log, LogLevel.Error);
                return false;
            }
        }
    }
}
