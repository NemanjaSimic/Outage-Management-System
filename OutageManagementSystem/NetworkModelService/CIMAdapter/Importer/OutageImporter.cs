using CIM.Model;
using Outage.DataImporter.CIMAdapter.Manager;
using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outage.Common.ServiceProxies;

namespace Outage.DataImporter.CIMAdapter.Importer
{
    public class OutageImporter
    {
        private ILogger logger = LoggerWrapper.Instance;

        private static OutageImporter outageImporter = null;
        private static object singletoneLock = new object();

        private ConcreteModel concreteModel;
        private Delta delta;
        //private Dictionary<long, ResourceDescription> negativeGidToResource;
        private Dictionary<string, long> mridToPositiveGidFromServer;
        private HashSet<string> mridsFromConcreteModel;
        private Dictionary<long, string> negativeGidToMrid;
        private ImportHelper importHelper;
        private TransformAndLoadReport report;

        #region Properties
        public static OutageImporter Instance
        {
            get
            {
                if(outageImporter == null)
                {
                    lock (singletoneLock)
                    {
                        if(outageImporter == null)
                        {
                            outageImporter = new OutageImporter();
                            outageImporter.Reset();
                        }
                    }
                }
                return outageImporter;
            }
        }

        public Delta NMSDelta
        {
            get
            {
                return delta;
            }
        }

        /// <summary>
        /// Dictionary which contains all data: Key - negative gid, Value - MRID
        /// </summary>
        public Dictionary<long, string>NegativeGidToMrid
        {
            get
            {
                return negativeGidToMrid ?? (negativeGidToMrid = new Dictionary<long, string>());
            }
        }

        /// <summary>
        /// Dictionary which contains all data: Key - MRID, Value - gid with positive counter
        /// </summary>
        public Dictionary<string, long> MridToPositiveGidFromServer
        {
            get
            {
                return mridToPositiveGidFromServer ?? (mridToPositiveGidFromServer = new Dictionary<string, long>());
            }
        }

        /// <summary>
		/// Dictionary which contains all data: Key - MRID, Value - Resource
		/// </summary>
        public HashSet<string> MridsFromConcreteModel
        {
            get
            {
                return mridsFromConcreteModel ?? (mridsFromConcreteModel = new HashSet<string>());
            }
        }
        #endregion


        public void Reset()
        {
            concreteModel = null;
            delta = new Delta();
            //mridToResource = new Dictionary<string, ResourceDescription>();
            mridToPositiveGidFromServer = new Dictionary<string, long>();
            mridsFromConcreteModel = new HashSet<string>();
            negativeGidToMrid = new Dictionary<long, string>();
            importHelper = new ImportHelper();
            report = null;
        }

        public TransformAndLoadReport CreateNMSDelta(ConcreteModel cimConcreteModel, NetworkModelGDAProxy gdaQueryProxy, ModelResourcesDesc resourcesDesc)
        {
            logger.LogInfo("Importing Outage Elements...");
            report = new TransformAndLoadReport();
            concreteModel = cimConcreteModel;
            delta.ClearDeltaOperations();
            //mridToResource.Clear();
            //negativeGidToResource.Clear();
            MridToPositiveGidFromServer.Clear();
            MridsFromConcreteModel.Clear();
            NegativeGidToMrid.Clear();

            if ((concreteModel != null) && (concreteModel.ModelMap != null))
            {
                try
                {
                    ConvertModelAndPopulateDelta(gdaQueryProxy, resourcesDesc);
                }
                catch (Exception ex)
                {
                    string message = $"{DateTime.Now} - ERROR in data import - {ex.Message}";
                    //LogManager.Log(message);
                    logger.LogError(message, ex);
                    report.Report.AppendLine(ex.Message);
                    report.Success = false;
                }
            }
            //LogManager.Log("Importing Outage Elements - END", LogLevel.Info);
            logger.LogInfo("Importing Outage Elements - END");
            return report;
        }

        private void ConvertModelAndPopulateDelta(NetworkModelGDAProxy gdaQueryProxy, ModelResourcesDesc resourcesDesc)
        {
            //LogManager.Log("Loading elements and creating delta...", LogLevel.Info);
            logger.LogInfo("Loading elements and creating delta...");

            PopulateNmsDataFromServer(gdaQueryProxy, resourcesDesc);
            
            //// import all concrete model types (DMSType enum)
            ImportBaseVoltages();
            ImportPowerTransformers();
            ImportTransformerWindings();
            ImportEnergySources();
            ImportEnergyConsumers();
            ImportFuses();
            ImportDisconnectors();
            ImportBreakers();
            ImportLoadBreakSwitches();
            ImportACLineSegments();
            ImportConnectivityNodes();
            ImportTerminals();
            ImportDiscretes();
            ImportAnalogs();


            CorrectNegativeReferences();
            CreateAndInsertDeleteOperations();
            //LogManager.Log("Loading elements and creating delta completed.", LogLevel.Info);
            logger.LogInfo("Loading elements and creating delta completed.");

        }

        private bool PopulateNmsDataFromServer(NetworkModelGDAProxy gdaQueryProxy, ModelResourcesDesc resourcesDesc)
        {
            bool success = false;
            string message = "Getting nms data from server started.";
            CommonTrace.WriteTrace(CommonTrace.TraceError, message);

            HashSet<ModelCode> requiredEntityTypes = new HashSet<ModelCode>();

            foreach (DMSType dmsType in Enum.GetValues(typeof(DMSType)))
            {
                if (dmsType == DMSType.MASK_TYPE)
                {
                    continue;
                }

                ModelCode mc = resourcesDesc.GetModelCodeFromType(dmsType);

                if (!requiredEntityTypes.Contains(mc))
                {
                    requiredEntityTypes.Add(mc);
                }
            }

            List<ModelCode> mrIdProp = new List<ModelCode>() { ModelCode.IDOBJ_MRID };
            foreach (ModelCode modelCodeType in requiredEntityTypes)
            {
                int iteratorId = 0;
                int resourcesLeft = 0;
                int numberOfResources = 10000; //TODO: connfigurabilno

                try
                {
                    iteratorId = gdaQueryProxy.GetExtentValues(modelCodeType, mrIdProp);
                    resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> gdaResult = gdaQueryProxy.IteratorNext(numberOfResources, iteratorId);

                        foreach (ResourceDescription rd in gdaResult)
                        {
                            if (rd.Properties[0].Id != ModelCode.IDOBJ_MRID)
                            {
                                continue;
                            }

                            string mrId = rd.Properties[0].PropertyValue.StringValue;

                            if(!MridToPositiveGidFromServer.ContainsKey(mrId))
                            {
                                MridToPositiveGidFromServer.Add(mrId, rd.Id);
                            }
                            else
                            {
                                throw new NotImplementedException("Method PopulateNmsDataFromServer() -> MridToPositiveGid.ContainsKey(mrId) == true");
                            }
                        }

                        resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);
                    }

                    gdaQueryProxy.IteratorClose(iteratorId);

                    message = "Getting nms data from server successfully finished.";
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    success = true;
                }
                catch (Exception e)
                {
                    message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeType, e.Message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    success = false;
                }
            }
               
            return success;
        }

        #region Import
        private void ImportPowerTransformers()
        {
            SortedDictionary<string, object> cimPowerTransformers = concreteModel.GetAllObjectsOfType("Outage.PowerTransformer");
            if (cimPowerTransformers != null)
            {
                foreach(KeyValuePair<string, object> cimPowerTransformerPair in cimPowerTransformers)
                {
                    Outage.PowerTransformer cimPowerTransformer = cimPowerTransformerPair.Value as Outage.PowerTransformer;
                    ResourceDescription rd = CreatePowerTransformerResourceDescription(cimPowerTransformer);
                    if (rd != null)
                    {
                        string mrid = cimPowerTransformer.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("PowerTransformer ID: ").Append(cimPowerTransformer.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("PowerTransformer ID: ").Append(cimPowerTransformer.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreatePowerTransformerResourceDescription(Outage.PowerTransformer cimPowerTransformer)
        {
            ResourceDescription rd = null;
            if (cimPowerTransformer != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.POWERTRANSFORMER, importHelper.CheckOutIndexForDMSType(DMSType.POWERTRANSFORMER));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimPowerTransformer.ID, gid);

                OutageConverter.PopulatePowerTransformerProperties(cimPowerTransformer, rd);
            }

            return rd;
        }

        private void ImportTransformerWindings()
        {
            SortedDictionary<string, object> cimTransformerWindings = concreteModel.GetAllObjectsOfType("Outage.TransformerWinding");
            if (cimTransformerWindings != null)
            {
                foreach (KeyValuePair<string, object> cimTransformerWindingPair in cimTransformerWindings)
                {
                    Outage.TransformerWinding cimTransformerWinding = cimTransformerWindingPair.Value as Outage.TransformerWinding;
                    ResourceDescription rd = CreateTransformerWindingResourceDescription(cimTransformerWinding);
                    if (rd != null)
                    {
                        string mrid = cimTransformerWinding.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("TransformerWinding ID: ").Append(cimTransformerWinding.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("TransformerWinding ID: ").Append(cimTransformerWinding.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateTransformerWindingResourceDescription(Outage.TransformerWinding cimTransformerWinding)
        {
            ResourceDescription rd = null;
            if (cimTransformerWinding != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.TRANSFORMERWINDING, importHelper.CheckOutIndexForDMSType(DMSType.TRANSFORMERWINDING));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimTransformerWinding.ID, gid);

                OutageConverter.PopulateTransformerWindingProperties(cimTransformerWinding, rd, importHelper, report);
            }
           

            return rd;
        }

        private void ImportBaseVoltages()
        {
            SortedDictionary<string, object> cimBaseVoltages = concreteModel.GetAllObjectsOfType("Outage.BaseVoltage");
            if (cimBaseVoltages != null)
            {
                foreach(KeyValuePair<string, object> cimBaseVoltagePair in cimBaseVoltages)
                {
                    Outage.BaseVoltage cimBaseVoltage = cimBaseVoltagePair.Value as Outage.BaseVoltage;
                    ResourceDescription rd = CreateBaseVoltageResourceDescription(cimBaseVoltage);
                    if (rd != null)
                    {
                        string mrid = cimBaseVoltage.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("BaseVoltage ID: ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("BaseVoltage ID: ").Append(cimBaseVoltage.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateBaseVoltageResourceDescription(Outage.BaseVoltage cimBaseVoltage)
        {
            ResourceDescription rd = null;

            if (cimBaseVoltage != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.BASEVOLTAGE, importHelper.CheckOutIndexForDMSType(DMSType.BASEVOLTAGE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimBaseVoltage.ID, gid);

                OutageConverter.PopulateBaseVoltageProperties(cimBaseVoltage, rd);
            }

            return rd;
        }

        private void ImportEnergySources()
        {
            SortedDictionary<string, object> cimEnergySources = concreteModel.GetAllObjectsOfType("Outage.EnergySource");

            if (cimEnergySources != null)
            {
                foreach(KeyValuePair<string, object> cimEnergySourcePair in cimEnergySources)
                {
                    Outage.EnergySource cimEnergySource = cimEnergySourcePair.Value as Outage.EnergySource;
                    ResourceDescription rd = CreateEnergySourceResourceDescription(cimEnergySource);
                    if (rd != null)
                    {
                        string mrid = cimEnergySource.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("EnergySource ID: ").Append(cimEnergySource.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("EnergySource ID: ").Append(cimEnergySource.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateEnergySourceResourceDescription(Outage.EnergySource cimEnergySource)
        {
            ResourceDescription rd = null;

            if (cimEnergySource != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ENERGYSOURCE, importHelper.CheckOutIndexForDMSType(DMSType.ENERGYSOURCE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimEnergySource.ID, gid);

                OutageConverter.PopulateEnergySourceProperties(cimEnergySource, rd, importHelper, report);
            }

            return rd;
        }

        private void ImportEnergyConsumers()
        {
            SortedDictionary<string, object> cimEnergyConsumers = concreteModel.GetAllObjectsOfType("Outage.EnergyConsumer");

            if (cimEnergyConsumers != null)
            {
                foreach (KeyValuePair<string, object> cimEnergyConsumerPair in cimEnergyConsumers)
                {
                    Outage.EnergyConsumer cimEnergyConsumer = cimEnergyConsumerPair.Value as Outage.EnergyConsumer;
                    ResourceDescription rd = CreateEnergyConsumerResourceDescription(cimEnergyConsumer);
                    if (rd != null)
                    {
                        string mrid = cimEnergyConsumer.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("EnergyConsumer ID: ").Append(cimEnergyConsumer.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("EnergyConsumer ID: ").Append(cimEnergyConsumer.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateEnergyConsumerResourceDescription(Outage.EnergyConsumer cimEnergyConsumer)
        {
            ResourceDescription rd = null;

            if (cimEnergyConsumer != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ENERGYCONSUMER, importHelper.CheckOutIndexForDMSType(DMSType.ENERGYCONSUMER));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimEnergyConsumer.ID, gid);

                OutageConverter.PopulateEnergyConsumerProperties(cimEnergyConsumer, rd, importHelper, report);
            }

            return rd;
        }

        private void ImportFuses()
        {
            SortedDictionary<string, object> cimFuses = concreteModel.GetAllObjectsOfType("Outage.Fuse");

            if (cimFuses != null)
            {
                foreach (KeyValuePair<string, object> cimFusePair in cimFuses)
                {
                    Outage.Fuse cimFuse = cimFusePair.Value as Outage.Fuse;
                    ResourceDescription rd = CreateFuseResourceDescription(cimFuse);
                    if (rd != null)
                    {
                        string mrid = cimFuse.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("Fuse ID: ").Append(cimFuse.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("Fuse ID: ").Append(cimFuse.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateFuseResourceDescription(Outage.Fuse cimFuse)
        {
            ResourceDescription rd = null;

            if (cimFuse != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.FUSE, importHelper.CheckOutIndexForDMSType(DMSType.FUSE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimFuse.ID, gid);

                OutageConverter.PopulateFuseProperties(cimFuse, rd, importHelper, report);
            }

            return rd;
        }

        private void ImportDisconnectors()
        {
            SortedDictionary<string, object> cimDisconnectors = concreteModel.GetAllObjectsOfType("Outage.Disconnector");

            if (cimDisconnectors != null)
            {
                foreach (KeyValuePair<string, object> cimDisconnectorPair in cimDisconnectors)
                {
                    Outage.Disconnector cimDisconnector = cimDisconnectorPair.Value as Outage.Disconnector;
                    ResourceDescription rd = CreateDisconnectorResourceDescription(cimDisconnector);
                    if (rd != null)
                    {
                        string mrid = cimDisconnector.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("Disconnector ID: ").Append(cimDisconnector.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("Disconnector ID: ").Append(cimDisconnector.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateDisconnectorResourceDescription(Outage.Disconnector cimDisconnector)
        {
            ResourceDescription rd = null;

            if (cimDisconnector != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.DISCONNECTOR, importHelper.CheckOutIndexForDMSType(DMSType.DISCONNECTOR));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimDisconnector.ID, gid);

                OutageConverter.PopulateDisconnectorProperties(cimDisconnector, rd, importHelper, report);
            }

            return rd;
        }

        private void ImportBreakers()
        {
            SortedDictionary<string, object> cimBreakers = concreteModel.GetAllObjectsOfType("Outage.Breaker");

            if (cimBreakers != null)
            {
                foreach (KeyValuePair<string, object> cimBreakerPair in cimBreakers)
                {
                    Outage.Breaker cimBreaker = cimBreakerPair.Value as Outage.Breaker;
                    ResourceDescription rd = CreateBreakerResourceDescription(cimBreaker);
                    if (rd != null)
                    {
                        string mrid = cimBreaker.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("Breaker ID: ").Append(cimBreaker.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("Breaker ID: ").Append(cimBreaker.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateBreakerResourceDescription(Outage.Breaker cimBreaker)
        {
            ResourceDescription rd = null;

            if (cimBreaker != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.BREAKER, importHelper.CheckOutIndexForDMSType(DMSType.BREAKER));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimBreaker.ID, gid);

                OutageConverter.PopulateBreakerProperties(cimBreaker, rd, importHelper, report);
            }

            return rd;
        }

        private void ImportLoadBreakSwitches()
        {
            SortedDictionary<string, object> cimLoadBreakSwitchs = concreteModel.GetAllObjectsOfType("Outage.LoadBreakSwitch");

            if (cimLoadBreakSwitchs != null)
            {
                foreach (KeyValuePair<string, object> cimLoadBreakSwitchPair in cimLoadBreakSwitchs)
                {
                    Outage.LoadBreakSwitch cimLoadBreakSwitch = cimLoadBreakSwitchPair.Value as Outage.LoadBreakSwitch;
                    ResourceDescription rd = CreateLoadBreakSwitchResourceDescription(cimLoadBreakSwitch);
                    if (rd != null)
                    {
                        string mrid = cimLoadBreakSwitch.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("LoadBreakSwitch ID: ").Append(cimLoadBreakSwitch.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("LoadBreakSwitch ID: ").Append(cimLoadBreakSwitch.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateLoadBreakSwitchResourceDescription(Outage.LoadBreakSwitch cimLoadBreakSwitch)
        {
            ResourceDescription rd = null;

            if (cimLoadBreakSwitch != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.LOADBREAKSWITCH, importHelper.CheckOutIndexForDMSType(DMSType.LOADBREAKSWITCH));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimLoadBreakSwitch.ID, gid);

                OutageConverter.PopulateLoadBreakSwitchProperties(cimLoadBreakSwitch, rd, importHelper, report);
            }

            return rd;
        }

        private void ImportACLineSegments()
        {
            SortedDictionary<string, object> cimACLineSegments = concreteModel.GetAllObjectsOfType("Outage.ACLineSegment");

            if (cimACLineSegments != null)
            {
                foreach (KeyValuePair<string, object> cimACLineSegmentPair in cimACLineSegments)
                {
                    Outage.ACLineSegment cimACLineSegment = cimACLineSegmentPair.Value as Outage.ACLineSegment;
                    ResourceDescription rd = CreateACLineSegmentResourceDescription(cimACLineSegment);
                    if (rd != null)
                    {
                        string mrid = cimACLineSegment.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("ACLineSegment ID: ").Append(cimACLineSegment.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("ACLineSegment ID: ").Append(cimACLineSegment.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateACLineSegmentResourceDescription(Outage.ACLineSegment cimACLineSegment)
        {
            ResourceDescription rd = null;

            if (cimACLineSegment != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ACLINESEGMENT, importHelper.CheckOutIndexForDMSType(DMSType.ACLINESEGMENT));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimACLineSegment.ID, gid);

                OutageConverter.PopulateACLineSegmentProperties(cimACLineSegment, rd, importHelper, report);
            }

            return rd;
        }

        private void ImportConnectivityNodes()
        {
            SortedDictionary<string, object> cimConnectivityNodes = concreteModel.GetAllObjectsOfType("Outage.ConnectivityNode");

            if (cimConnectivityNodes != null)
            {
                foreach (KeyValuePair<string, object> cimConnectivityNodePair in cimConnectivityNodes)
                {
                    Outage.ConnectivityNode cimConnectivityNode = cimConnectivityNodePair.Value as Outage.ConnectivityNode;
                    ResourceDescription rd = CreateConnectivityNodeResourceDescription(cimConnectivityNode);
                    if (rd != null)
                    {
                        string mrid = cimConnectivityNode.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("ConnectivityNode ID: ").Append(cimConnectivityNode.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("ConnectivityNode ID: ").Append(cimConnectivityNode.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateConnectivityNodeResourceDescription(Outage.ConnectivityNode cimConnectivityNode)
        {
            ResourceDescription rd = null;

            if (cimConnectivityNode != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.CONNECTIVITYNODE, importHelper.CheckOutIndexForDMSType(DMSType.CONNECTIVITYNODE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimConnectivityNode.ID, gid);

                OutageConverter.PopulateConnectivityNodeProperties(cimConnectivityNode, rd);
            }

            return rd;
        }

        private void ImportTerminals()
        {
            SortedDictionary<string, object> cimTerminals = concreteModel.GetAllObjectsOfType("Outage.Terminal");

            if (cimTerminals != null)
            {
                foreach (KeyValuePair<string, object> cimTerminalPair in cimTerminals)
                {
                    Outage.Terminal cimTerminal = cimTerminalPair.Value as Outage.Terminal;
                    ResourceDescription rd = CreateTerminalResourceDescription(cimTerminal);
                    if (rd != null)
                    {
                        string mrid = cimTerminal.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("Terminal ID: ").Append(cimTerminal.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("Terminal ID: ").Append(cimTerminal.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateTerminalResourceDescription(Outage.Terminal cimTerminal)
        {
            ResourceDescription rd = null;

            if (cimTerminal != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.TERMINAL, importHelper.CheckOutIndexForDMSType(DMSType.TERMINAL));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimTerminal.ID, gid);

                OutageConverter.PopulateTerminalProperties(cimTerminal, rd, importHelper, report);
            }

            return rd;
        }

        private void ImportDiscretes()
        {
            SortedDictionary<string, object> cimDiscretes = concreteModel.GetAllObjectsOfType("Outage.Discrete");

            if (cimDiscretes != null)
            {
                foreach (KeyValuePair<string, object> cimDiscretePair in cimDiscretes)
                {
                    Outage.Discrete cimDiscrete = cimDiscretePair.Value as Outage.Discrete;
                    ResourceDescription rd = CreateDiscreteResourceDescription(cimDiscrete);
                    if (rd != null)
                    {
                        string mrid = cimDiscrete.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("Discret ID: ").Append(cimDiscrete.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("Discret ID: ").Append(cimDiscrete.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateDiscreteResourceDescription(Outage.Discrete cimDiscrete)
        {
            ResourceDescription rd = null;

            if (cimDiscrete != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.DISCRETE, importHelper.CheckOutIndexForDMSType(DMSType.DISCRETE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimDiscrete.ID, gid);

                OutageConverter.PopulateDiscreteProperties(cimDiscrete, rd, importHelper, report);
            }

            return rd;
        }

        private void ImportAnalogs()
        {
            SortedDictionary<string, object> cimAnalogs = concreteModel.GetAllObjectsOfType("Outage.Analog");

            if (cimAnalogs != null)
            {
                foreach (KeyValuePair<string, object> cimAnalogPair in cimAnalogs)
                {
                    Outage.Analog cimAnalog = cimAnalogPair.Value as Outage.Analog;
                    ResourceDescription rd = CreateAnalogResourceDescription(cimAnalog);
                    if (rd != null)
                    {
                        string mrid = cimAnalog.MRID;
                        CreateAndInsertDeltaOperation(mrid, rd);

                        report.Report.Append("Analog ID: ").Append(cimAnalog.ID).Append(" SUCCESSFULLY converted to GID: ").AppendLine($"0x{rd.Id:X16}");
                    }
                    else
                    {
                        report.Report.Append("Analog ID: ").Append(cimAnalog.ID).AppendLine(" FAILED to be converted");
                    }
                }
            }
        }

        private ResourceDescription CreateAnalogResourceDescription(Outage.Analog cimAnalog)
        {
            ResourceDescription rd = null;

            if (cimAnalog != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ANALOG, importHelper.CheckOutIndexForDMSType(DMSType.ANALOG));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimAnalog.ID, gid);

                OutageConverter.PopulateAnalogProperties(cimAnalog, rd, importHelper, report);
            }

            return rd;
        }
        #endregion

        private void CreateAndInsertDeltaOperation(string mrid, ResourceDescription rd)
        {
            long negGid = 0;
            DeltaOpType deltaOp = DeltaOpType.Insert;
            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id);

            if (ModelCodeHelper.ExtractEntityIdFromGlobalId(rd.Id) < 0 && !NegativeGidToMrid.ContainsKey(rd.Id))
            {
                negGid = rd.Id;
                NegativeGidToMrid.Add(rd.Id, mrid);
            }

            if (MridToPositiveGidFromServer.ContainsKey(mrid))
            {
                rd.Id = MridToPositiveGidFromServer[mrid];
                deltaOp = DeltaOpType.Update;
            }

            delta.AddDeltaOperation(deltaOp, rd, true);

            if (!MridsFromConcreteModel.Contains(mrid))
            {
                MridsFromConcreteModel.Add(mrid);
            }

            report.Report.Append("Operation: ").Append(deltaOp).Append(" ").Append(type)
                         .Append(" mrid: ").Append(mrid)
                         .Append(" ID: ").Append(string.Format("0x{0:X16}", negGid))
                         .Append(" after correction is GID: ").AppendLine($"0x{rd.Id:X16}");
        }

        private void CorrectNegativeReferences()
        {
            foreach (ResourceDescription rd in delta.InsertOperations)
            { 
                foreach(Property prop in rd.Properties)
                {
                    if(prop.Type == PropertyType.Reference)
                    {
                        long targetGid = prop.AsLong();

                        if (ModelCodeHelper.ExtractEntityIdFromGlobalId(targetGid) < 0)
                        {
                            if(NegativeGidToMrid.ContainsKey(targetGid))
                            {
                                string mrid = NegativeGidToMrid[targetGid];
                                if (MridToPositiveGidFromServer.ContainsKey(mrid))
                                {
                                    long positiveGid = MridToPositiveGidFromServer[mrid];
                                    prop.SetValue(positiveGid);
                                }
                            }
                        }
                    }
                }
            }

            foreach (ResourceDescription rd in delta.UpdateOperations)
            {
                foreach (Property prop in rd.Properties)
                {
                    if (prop.Type == PropertyType.Reference)
                    {
                        long targetGid = prop.AsLong();

                        if (ModelCodeHelper.ExtractEntityIdFromGlobalId(targetGid) < 0)
                        {
                            if (NegativeGidToMrid.ContainsKey(targetGid))
                            {
                                string mrid = NegativeGidToMrid[targetGid];
                                if (MridToPositiveGidFromServer.ContainsKey(mrid))
                                {
                                    long positiveGid = MridToPositiveGidFromServer[mrid];
                                    prop.SetValue(positiveGid);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreateAndInsertDeleteOperations()
        {
            foreach(string mrid in MridToPositiveGidFromServer.Keys)
            {
                if(!MridsFromConcreteModel.Contains(mrid))
                {
                    long serverGid = MridToPositiveGidFromServer[mrid];
                    ResourceDescription rd = new ResourceDescription(serverGid);
                    DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id);
                    DeltaOpType deltaOp = DeltaOpType.Delete;

                    delta.AddDeltaOperation(deltaOp, rd, true);

                    report.Report.Append("Operation: ").Append(deltaOp).Append(" ").Append(type)
                                 .Append(" mrid: ").Append(mrid)
                                 .Append(" ID: ").AppendLine(string.Format("0x{0:X16}", serverGid));
                }
            }
        }
    }
}
