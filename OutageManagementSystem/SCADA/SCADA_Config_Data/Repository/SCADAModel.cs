using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceProxies;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData;
using Outage.SCADA.SCADAData.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Outage.SCADA.SCADAData.Repository
{
    public class SCADAModel
    {
        private ILogger logger = LoggerWrapper.Instance;
        private ModelResourcesDesc modelResourceDesc;
        //private ConfigWriter configWriter;

        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private ISCADAConfigData configData = SCADAConfigData.Instance;

        private Dictionary<long, ISCADAModelPointItem> incomingScadaModel;
        private Dictionary<ushort, long> incomingAddressToGidMap;
        private Dictionary<long, ISCADAModelPointItem> currentScadaModel;
        private Dictionary<ushort, long> currentAddressToGidMap;


        protected Dictionary<long, ISCADAModelPointItem> IncomingScadaModel
        {
            get { return incomingScadaModel ?? (incomingScadaModel = new Dictionary<long, ISCADAModelPointItem>()); }
        }

        protected Dictionary<ushort, long> IncomingAddressToGidMap
        {
            get { return incomingAddressToGidMap ?? (incomingAddressToGidMap = new Dictionary<ushort, long>(CurrentScadaModel.Count)); }
        }

        public Dictionary<long, ISCADAModelPointItem> CurrentScadaModel
        {
            get { return currentScadaModel ?? (currentScadaModel = new Dictionary<long, ISCADAModelPointItem>()); }
        }

        public Dictionary<ushort, long> CurrentAddressToGidMap
        {
            get { return currentAddressToGidMap ?? (currentAddressToGidMap = new Dictionary<ushort, long>(CurrentScadaModel.Count)); }
        }


        [Obsolete]
        public Dictionary<long, ResourceDescription> NetworkModel { get; protected set; }
        [Obsolete]
        public Dictionary<long, Dictionary<ModelCode, Property>> NetworkModelProps { get; protected set; }

        #region Instance
        private static SCADAModel instance;
        private static readonly object lockSync = new object();

        public static SCADAModel Instance
        {
            get
            {
                if(instance == null)
                {
                    lock(lockSync)
                    {
                        if(instance == null)
                        {
                            instance = new SCADAModel();
                        }
                    }
                }

                return instance;
            }
        }
        #endregion

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
                        logger.LogError(message, ex);
                        gdaQueryProxy = null;
                    }
                    finally
                    {
                        numberOfTries++;
                        logger.LogDebug($"SCADAModel: GdaQueryProxy getter, try number: {numberOfTries}.");
                        Thread.Sleep(500);
                    }
                }

                return gdaQueryProxy;
            }
        }
        #endregion

        private SCADAModel()
        {
            currentScadaModel = new Dictionary<long, ISCADAModelPointItem>();
            incomingScadaModel = new Dictionary<long, ISCADAModelPointItem>();
            modelResourceDesc = new ModelResourcesDesc();
        }

        public bool Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            this.modelChanges = modelChanges;
            return true;
        }

        #region ITransactionActorContract
        public bool Prepare()
        {
            bool success;

            try
            {
                foreach(long gid in CurrentScadaModel.Keys)
                {
                    ISCADAModelPointItem pointItem = CurrentScadaModel[gid].Clone();
                    IncomingScadaModel.Add(gid, pointItem);
                    IncomingAddressToGidMap.Add(pointItem.Address, gid);
                }

                foreach (long gid in modelChanges[DeltaOpType.Insert])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        ISCADAModelPointItem pointItem = CreateConfigItemForEntity(gid);

                        if(IncomingScadaModel.ContainsKey(gid) || IncomingAddressToGidMap.ContainsKey(pointItem.Address))
                        {
                            string message = $"Model update data in fault state. Inserting gid: {gid} or measurement address: {pointItem.Address}, that already exists in SCADA model.";
                            logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        IncomingScadaModel.Add(gid, pointItem);
                        IncomingAddressToGidMap.Add(pointItem.Address, gid);
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Update])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if(type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        ISCADAModelPointItem pointItem = CreateConfigItemForEntity(gid);

                        if (!IncomingScadaModel.ContainsKey(gid) || !IncomingAddressToGidMap.ContainsKey(pointItem.Address))
                        {
                            string message = $"Model update data in fault state. Updating entity with gid: {gid} or measurement address: {pointItem.Address}, that does not exists in SCADA model.";
                            logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        IncomingScadaModel[gid] = pointItem;
                        IncomingAddressToGidMap[pointItem.Address] = gid;
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Delete])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        if (!IncomingScadaModel.ContainsKey(gid))
                        {
                            string message = $"Model update data in fault state. Deleting entity with gid: {gid}, that does not exists in SCADA model.";
                            logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        ushort address = IncomingScadaModel[gid].Address;
                        IncomingAddressToGidMap.Remove(address);
                        IncomingScadaModel.Remove(gid);
                    }
                }

                success = true;
            }
            catch (Exception e)
            {
                logger.LogError($"Exception catched in Prepare method on SCADAModel.", e);
                success = false;
            }

            return success;
        }

        public void Commit()
        {
            currentScadaModel = IncomingScadaModel;
            incomingScadaModel = null;

            currentAddressToGidMap = IncomingAddressToGidMap;
            incomingAddressToGidMap = null;

            modelChanges.Clear();

            string message = $"Incoming SCADA model is confirmed.";
            Console.WriteLine(message);
            logger.LogInfo(message);
        }

        public void Rollback()
        {
            incomingScadaModel = null;
            incomingAddressToGidMap = null;
            modelChanges.Clear();

            string message = $"Incoming SCADA model is rejected.";
            Console.WriteLine(message);
            logger.LogInfo(message);
        }
        #endregion


        #region ImportScadaModel
        private bool ImportModel()
        {
            //TODO: log info
            Console.WriteLine("Importing analog measurements started...");
            bool analogImportSuccess = ImportAnalog();
            //TODO: log info finish
            Console.WriteLine($"Importing analog measurements finished. ['success' value: {analogImportSuccess}]");

            //TODO: log info
            Console.WriteLine("Importing discrete measurements started...");
            bool discreteImportSuccess = ImportDiscrete();
            //TODO: log info finish
            Console.WriteLine($"Importing discrete measurements finished. ['success' value: {discreteImportSuccess}]");

            return analogImportSuccess && discreteImportSuccess;
        }

        private bool ImportAnalog()
        {
            bool success;
            int numberOfResources = 1000;
            List<ModelCode> props = modelResourceDesc.GetAllPropertyIds(ModelCode.ANALOG);

            try
            {
                using (NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
                {
                    if (gdaProxy != null)
                    {
                        int iteratorId = gdaProxy.GetExtentValues(ModelCode.ANALOG, props);
                        int resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);

                        while (resourcesLeft > 0)
                        {
                            List<ResourceDescription> rds = gdaProxy.IteratorNext(numberOfResources, iteratorId);
                            for (int i = 0; i < rds.Count; i++)
                            {
                                if (rds[i] != null)
                                { 
                                    //NetworkModel.Add(rds[i].Id, rds[i]);
                                    ISCADAModelPointItem pointItem = new SCADAModelPointItem(rds[i].Properties, ModelCode.ANALOG);
                                    CurrentScadaModel.Add(rds[i].Id, pointItem);
                                    CurrentAddressToGidMap.Add(pointItem.Address, rds[i].Id);
                                    //TODO: log debug
                                }
                            }
                            resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);
                        }

                        success = true;
                    }
                    else
                    {
                        success = false;
                        string errMsg = "From ImportAnalog() method: NetworkModelGDAProxy is null.";
                        logger.LogWarn(errMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                string errorMessage = $"ImportAnalog failed with error: {ex.Message}";
                Console.WriteLine(errorMessage);
                logger.LogError(errorMessage, ex);
            }

            return success;
        }

        private bool ImportDiscrete()
        {
            bool success;
            int numberOfResources = 1000;
            List<ModelCode> props = modelResourceDesc.GetAllPropertyIds(ModelCode.DISCRETE);

            try
            {
                using (NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
                {
                    if (gdaProxy != null)
                    {
                        int iteratorId = gdaProxy.GetExtentValues(ModelCode.DISCRETE, props);
                        int resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);

                        while (resourcesLeft > 0)
                        {
                            List<ResourceDescription> rds = gdaProxy.IteratorNext(numberOfResources, iteratorId);
                            for (int i = 0; i < rds.Count; i++)
                            {
                                if (rds[i] != null)
                                {
                                    //NetworkModel.Add(rds[i].Id, rds[i]);
                                    ISCADAModelPointItem pointItem = new SCADAModelPointItem(rds[i].Properties, ModelCode.DISCRETE);
                                    CurrentScadaModel.Add(rds[i].Id, pointItem);
                                    CurrentAddressToGidMap.Add(pointItem.Address, rds[i].Id);
                                    //TODO: log debug
                                }
                            }
                            resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);
                        }

                        success = true;
                    }
                    else
                    {
                        success = false;
                        string errMsg = "From ImportDiscrete() method: NetworkModelGDAProxy is null.";
                        logger.LogWarn(errMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                string errorMessage = $"ImportDiscrete failed with error: {ex.Message}";
                Console.WriteLine(errorMessage);
                logger.LogError(errorMessage, ex);
            }

            return success;
        }
        #endregion

        private ISCADAModelPointItem CreateConfigItemForEntity(long gid)
        {
            ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
            List<ModelCode> props;
            ResourceDescription rd;
            ISCADAModelPointItem pointItem;

            using(NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
            {
                if(gdaProxy != null)
                {
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        props = modelResourceDesc.GetAllPropertyIds(type);
                        rd = gdaProxy.GetValues(gid, props);
                        pointItem = new SCADAModelPointItem(rd.Properties, type);
                    }
                    else
                    {
                        string errMessage = $"ResourceDescription type is neither analog nor digital. Type: {type}.";
                        logger.LogWarn(errMessage);
                        pointItem = null;
                    }
                }
                else
                {
                    string message = "From method CreateConfigItemForEntity(): NetworkModelGDAProxy is null.";
                    logger.LogWarn(message);
                    throw new NullReferenceException(message);
                }
            }

            return pointItem;
        }
    }
}