﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.DistributedTransaction;
using Outage.DataModel;
using Outage.DBModel.NetworkModelService;
using Outage.NetworkModelService.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Outage.NetworkModelService
{
    public class NetworkModel
    {
        #region Fields
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private IMongoDatabase db;
        private Delta currentDelta;

        ProxyFactory proxyFactory;

        /// <summary>
        /// ModelResourceDesc class contains metadata of the model
        /// </summary>
        private ModelResourcesDesc resourcesDescs;

        /// <summary>
        /// Dictionary which contains all data: Key - DMSType, Value - Container
        /// </summary>
        private Dictionary<DMSType, Container> networkDataModel;

        /// <summary>
		/// Dictionaru which contains all incoming data: Key - DMSType, Value - Container;
        /// Used while applying deltas.
		/// </summary>
        private Dictionary<DMSType, Container> incomingNetworkDataModel;

        /// <summary>
		/// Dictionaru which contains all incoming data: Key - DMSType, Value - Container;
        /// Contains old network model during distributed transaction.
		/// </summary>
        private Dictionary<DMSType, Container> oldNetworkDataModel;
        #endregion

        #region Properties
        /// <summary>
        /// Dictionary which contains all data: Key - DMSType, Value - Container
        /// </summary>
        public Dictionary<DMSType, Container> NetworkDataModel
        {
            get
            {
                return networkDataModel ?? (networkDataModel = new Dictionary<DMSType, Container>());
            }
        }

        #endregion


        /// <summary>
        /// Initializes a new instance of the Model class.
        /// </summary>
        public NetworkModel()
        {
            InitializeBsonSerializer();

            networkDataModel = new Dictionary<DMSType, Container>();
            resourcesDescs = new ModelResourcesDesc();
            proxyFactory = new ProxyFactory();

            try
            {
                MongoClient dbClient = new MongoClient(Config.Instance.DbConnectionString);
                db = dbClient.GetDatabase("NMSDatabase");
            }
            catch (Exception e)
            {
                Logger.LogError("Error on database Init.", e);
            }

            Initialize();
        }


        #region Find

        public bool EntityExists(long globalId)
        {
            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);

            if (networkDataModel.ContainsKey(type))
            {
                Container container = networkDataModel[type];

                if (container.EntityExists(globalId))
                {
                    return true;
                }
            }

            return false;
        }

        private bool EntityExistsInIncomingData(long globalId)
        {
            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);

            if (incomingNetworkDataModel.ContainsKey(type))
            {
                Container container = incomingNetworkDataModel[type];
                return container.EntityExists(globalId);
            }

            return false;
        }

        public IdentifiedObject GetEntity(long globalId)
        {
            if (EntityExists(globalId))
            {
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
                IdentifiedObject io = networkDataModel[type].GetEntity(globalId);

                return io;
            }
            else
            {
                string message = string.Format("Entity  (GID: 0x{0:X16}) does not exist.", globalId);
                Logger.LogError(message);
                throw new Exception(message);
            }
        }

        private IdentifiedObject GetEntityFromIncomingData(long globalId)
        {
            if (EntityExistsInIncomingData(globalId))
            {
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
                IdentifiedObject io = incomingNetworkDataModel[type].GetEntity(globalId);

                return io;
            }
            else
            {
                string message = string.Format("Entity  (GID: 0x{0:X16}) does not exist.", globalId);
                Logger.LogError(message);
                throw new Exception(message);
            }
        }

        #endregion Find


        #region GDA query

        /// <summary>
        /// Gets resource description for entity requested by globalId.
        /// </summary>
        /// <param name="globalId">Id of the entity</param>
        /// <param name="properties">List of requested properties</param>		
        /// <returns>Resource description of the specified entity</returns>
        public ResourceDescription GetValues(long globalId, List<ModelCode> properties)
        {
            Logger.LogInfo($"Getting values for GID: 0x{globalId:X16}.");

            try
            {
                IdentifiedObject io = GetEntity(globalId);

                ResourceDescription rd = new ResourceDescription(globalId);

                Property property = null;

                // insert specified properties
                foreach (ModelCode propId in properties)
                {
                    property = new Property(propId);
                    io.GetProperty(property);
                    rd.AddProperty(property);
                }
                Logger.LogInfo($"Getting values for GID: 0x{globalId:X16} succedded.");

                return rd;
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to get values for entity with GID: 0x{0:X16}. {1}", globalId, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Gets resource iterator that holds descriptions for all entities of the specified type.
        /// </summary>		
        /// <param name="type">Type of entity that is requested</param>
        /// <param name="properties">List of requested properties</param>		
        /// <returns>Resource iterator for the requested entities</returns>
        public ResourceIterator GetExtentValues(ModelCode entityType, List<ModelCode> properties)
        {
            Logger.LogInfo($"Getting extent values for entity type: {entityType}.");

            try
            {
                List<long> globalIds = new List<long>();
                Dictionary<DMSType, List<ModelCode>> class2PropertyIDs = new Dictionary<DMSType, List<ModelCode>>();

                DMSType entityDmsType = ModelCodeHelper.GetTypeFromModelCode(entityType);

                if (NetworkDataModel.ContainsKey(entityDmsType))
                {
                    Container container = NetworkDataModel[entityDmsType];
                    globalIds = container.GetEntitiesGlobalIds();
                    class2PropertyIDs.Add(entityDmsType, properties);
                }

                ResourceIterator ri = new ResourceIterator(this, globalIds, class2PropertyIDs);

                Logger.LogInfo($"Getting extent values for entity type: {entityType} succedded.");

                return ri;
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to get extent values for entity type: {0}. {1}", entityType, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Gets resource iterator that holds descriptions for all entities related to specified source.
        /// </summary>
        /// <param name="contextId">Context Id</param>
        /// <param name="properties">List of requested properties</param>
        /// <param name="association">Relation between source and entities that should be returned</param>
        /// <param name="source">Id of entity that is start for association search</param>
        /// <param name="typeOfQuery">Query type choice(global or local)</param>
        /// <returns>Resource iterator for the requested entities</returns>
        public ResourceIterator GetRelatedValues(long source, List<ModelCode> properties, Association association)
        {
            Logger.LogInfo($"Getting related values for source: 0x{source:X16}.");

            try
            {
                List<long> relatedGids = ApplyAssocioationOnSource(source, association);

                Dictionary<DMSType, List<ModelCode>> class2PropertyIDs = new Dictionary<DMSType, List<ModelCode>>();

                foreach (long relatedGid in relatedGids)
                {
                    DMSType entityDmsType = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(relatedGid);

                    if (!class2PropertyIDs.ContainsKey(entityDmsType))
                    {
                        class2PropertyIDs.Add(entityDmsType, properties);
                    }
                }

                ResourceIterator ri = new ResourceIterator(this, relatedGids, class2PropertyIDs);

                Logger.LogInfo($"Getting related values for source: 0x{source:X16} succedded.");

                return ri;
            }
            catch (Exception ex)
            {
                string message = String.Format("Failed to get related values for source GID: 0x{0:X16}. {1}.", source, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        #endregion GDA query	


        public Common.GDA.UpdateResult ApplyDelta(Delta delta, bool isInitialization = false)
        {
            currentDelta = delta;

            Common.GDA.UpdateResult updateResult = new Common.GDA.UpdateResult();

            //shallow copy 
            incomingNetworkDataModel = new Dictionary<DMSType, Container>(NetworkDataModel);
            Logger.LogDebug($"Incoming model [HashCode: 0x{incomingNetworkDataModel.GetHashCode():X16}] is shallow copy of Current model [HashCode: 0x{networkDataModel.GetHashCode():X16}].");

            try
            {
                Logger.LogInfo("Applying delta to network model.");

                Dictionary<short, int> typesCounters = GetCounters();
                Dictionary<long, long> globalIdPairs = new Dictionary<long, long>();

                if (!isInitialization)
                {
                    delta.FixNegativeToPositiveIds(ref typesCounters, ref globalIdPairs);

                }

                updateResult.GlobalIdPairs = globalIdPairs;
                delta.SortOperations();

                foreach (ResourceDescription rd in delta.InsertOperations)
                {
                    InsertEntity(rd);
                }

                foreach (ResourceDescription rd in delta.UpdateOperations)
                {
                    UpdateEntity(rd);
                }

                foreach (ResourceDescription rd in delta.DeleteOperations)
                {
                    DeleteEntity(rd);
                }

                oldNetworkDataModel = networkDataModel;
                Logger.LogDebug($"Old model [HashCode: 0x{networkDataModel.GetHashCode():X16}] becomes Current model [HashCode: 0x{incomingNetworkDataModel.GetHashCode():X16}].");

                networkDataModel = incomingNetworkDataModel;
                Logger.LogDebug($"Current model [HashCode: 0x{networkDataModel.GetHashCode():X16}] becomes Incoming model [HashCode: 0x{incomingNetworkDataModel.GetHashCode():X16}].");

                if (isInitialization)
                {
                    Commit(isInitialization);
                }
                else
                {
                    StartDistributedTransaction(delta);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Applying delta to network model failed. {0}.", ex.Message);
                Logger.LogError(message, ex);

                updateResult.Result = ResultType.Failed;
                updateResult.Message = message;
                currentDelta = null;
            }
            finally
            {
                //if (!isInitialization)
                //{
                //    SaveDelta(delta);
                //}

                if (updateResult.Result == ResultType.Succeeded)
                {
                    string message = "Applying delta to network model successfully finished.";
                    Logger.LogInfo(message);
                    updateResult.Message = message;
                }
            }

            return updateResult;
        }


        #region ITransactionActorContract
        public bool Prepare()
        {
            return oldNetworkDataModel != null && networkDataModel != null && oldNetworkDataModel.GetHashCode() != networkDataModel.GetHashCode();
        }

        /// <summary>
        /// 2PhaseCommitProtocol - Commit Phase
        /// </summary>
        /// <param name="isInitialization">Indicates if changes are commited in initialization step, after the Network Model service has started.</param>
        /// <returns></returns>
        public bool Commit(bool isInitialization = false)
        {
            if (!isInitialization && currentDelta != null)
            {
                SaveDelta(currentDelta);
            }

            currentDelta = null;
            oldNetworkDataModel = null;
            Logger.LogDebug($"Current model [HashCode: 0x{incomingNetworkDataModel.GetHashCode():X16}] commited. Old model is set to null.");
            return true;
        }

        public bool Rollback()
        {
            currentDelta = null;
            networkDataModel = oldNetworkDataModel;
            Logger.LogDebug($"Current model [HashCode: 0x{networkDataModel.GetHashCode():X16}] rollbacked to Old model [HashCode: 0x{oldNetworkDataModel.GetHashCode():X16}].");
            return true;
        }
        #endregion

        #region Private Members
        private void Initialize()
        {
            long networkModelVersion = 0, deltaVersion = 0;
            var versionsCollection = db.GetCollection<ModelVersionDocument>("versions");
            var networkDataModelCollection = db.GetCollection<NetworkDataModelDocument>("networkModels");

            GetVersions(ref networkModelVersion, ref deltaVersion, versionsCollection);

            if (deltaVersion > networkModelVersion)
            {
                Logger.LogDebug("Delta version is higher then network model version.");
                List<Delta> result = ReadAllDeltas(deltaVersion, networkModelVersion);

                var networkModelFilter = Builders<NetworkDataModelDocument>.Filter.Eq("_id", networkModelVersion);
                if (networkModelVersion > 0)
                {
                    networkDataModel = networkDataModelCollection.Find(networkModelFilter).First().NetworkModel;
                }

                foreach (Delta delta in result)
                {
                    try
                    {
                        ApplyDelta(delta, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error while applying delta (id: {delta.Id}) durning service initialization. {ex.Message}", ex);
                    }
                }
                SaveNetworkModel();
            }
            else if (networkModelVersion > 0)
            {
                var networkDataModelFilter = Builders<NetworkDataModelDocument>.Filter.Eq("_id", networkModelVersion);
                networkDataModel = networkDataModelCollection.Find(networkDataModelFilter).First().NetworkModel;
            }
        }

        private void StartDistributedTransaction(Delta delta)
        {
            using (TransactionCoordinatorProxy transactionCoordinatorProxy = proxyFactory.CreateProxy<TransactionCoordinatorProxy, ITransactionCoordinatorContract>(EndpointNames.TransactionCoordinatorEndpoint))
            {
                if (transactionCoordinatorProxy == null)
                {
                    Logger.LogWarn("TransactionCoordinatorProxy is not initialized. This can be due to TransactionCoordinator not being stared.");
                    Commit(false);
                    return;
                }

                transactionCoordinatorProxy.StartDistributedUpdate();
                Logger.LogDebug("StartDistributedUpdate() invoked on Transaction Coordinator.");
            }

            Dictionary<DeltaOpType, List<long>> modelChanges = new Dictionary<DeltaOpType, List<long>>()
            {
                { DeltaOpType.Insert, new List<long>(delta.InsertOperations.Count) },
                { DeltaOpType.Update, new List<long>(delta.UpdateOperations.Count) },
                { DeltaOpType.Delete, new List<long>(delta.DeleteOperations.Count) },
            };

            foreach (ResourceDescription rd in delta.InsertOperations)
            {
                modelChanges[DeltaOpType.Insert].Add(rd.Id);
            }

            foreach (ResourceDescription rd in delta.UpdateOperations)
            {
                modelChanges[DeltaOpType.Update].Add(rd.Id);
            }

            foreach (ResourceDescription rd in delta.DeleteOperations)
            {
                modelChanges[DeltaOpType.Delete].Add(rd.Id);
            }

            bool success = false;

            using (ModelUpdateNotificationProxy scadaModelUpdateNotifierProxy = proxyFactory.CreateProxy<ModelUpdateNotificationProxy, IModelUpdateNotificationContract>(EndpointNames.SCADAModelUpdateNotifierEndpoint))
            {
                if (scadaModelUpdateNotifierProxy == null)
                {
                    string message = "ModelUpdateNotificationProxy for SCADA is null.";
                    Logger.LogWarn(message);
                    throw new NullReferenceException(message);
                }

                success = scadaModelUpdateNotifierProxy.NotifyAboutUpdate(modelChanges);
                Logger.LogDebug("NotifyAboutUpdate() method invoked on SCADA Transaction actor.");
            }

            if (success)
            {
                using (ModelUpdateNotificationProxy calculationEngineModelUpdateNotifierProxy = proxyFactory.CreateProxy<ModelUpdateNotificationProxy, IModelUpdateNotificationContract>(EndpointNames.CalculationEngineModelUpdateNotifierEndpoint))
                {
                    if (calculationEngineModelUpdateNotifierProxy == null)
                    {
                        string message = "ModelUpdateNotificationProxy for CalculationEngine is null.";
                        Logger.LogWarn(message);
                        throw new NullReferenceException(message);
                    }

                    success = calculationEngineModelUpdateNotifierProxy.NotifyAboutUpdate(modelChanges);
                    Logger.LogDebug("NotifyAboutUpdate() method invoked on CE Transaction actor.");
                }

                if (success)
                {
                    using (ModelUpdateNotificationProxy outageModelUpdateNotifierProxy = proxyFactory.CreateProxy<ModelUpdateNotificationProxy, IModelUpdateNotificationContract>(EndpointNames.OutageModelUpdateNotifierEndpoint))
                    {
                        if (outageModelUpdateNotifierProxy == null)
                        {
                            string message = "ModelUpdateNotificationProxy for Outage is null.";
                            Logger.LogWarn(message);
                            throw new NullReferenceException(message);
                        }

                        success = outageModelUpdateNotifierProxy.NotifyAboutUpdate(modelChanges);
                        Logger.LogDebug("NotifyAboutUpdate() method invoked on Outage Transaction actor. ");
                    }

                    if (success)
                    {
                        using (TransactionEnlistmentProxy transactionEnlistmentProxy = proxyFactory.CreateProxy<TransactionEnlistmentProxy, ITransactionEnlistmentContract>(EndpointNames.TransactionEnlistmentEndpoint))
                        {
                            if (transactionEnlistmentProxy == null)
                            {
                                string message = "TransactionEnlistmentProxy is null.";
                                Logger.LogWarn(message);
                                throw new NullReferenceException(message);    
                            }

                            success = transactionEnlistmentProxy.Enlist(ServiceNames.NetworkModelService);
                            Logger.LogDebug("Enlist() method invoked on Transaction Coordinator.");
                        }

                    }
                }
            }

            using (TransactionCoordinatorProxy transactionCoordinatorProxy = proxyFactory.CreateProxy<TransactionCoordinatorProxy, ITransactionCoordinatorContract>(EndpointNames.TransactionCoordinatorEndpoint))
            {
                if (transactionCoordinatorProxy == null)
                {
                    string message = "TransactionCoordinatorProxy is null.";
                    Logger.LogWarn(message);
                    throw new NullReferenceException(message);
                }
                
                transactionCoordinatorProxy.FinishDistributedUpdate(success);
                Logger.LogDebug($"FinishDistributedUpdate() invoked on Transaction Coordinator with parameter 'success' value: {success}.");
            }
        }

        private Dictionary<short, int> GetCounters()
        {
            Dictionary<short, int> typesCounters = new Dictionary<short, int>();

            foreach (DMSType type in Enum.GetValues(typeof(DMSType)))
            {
                typesCounters[(short)type] = 0;

                if (networkDataModel.ContainsKey(type))
                {
                    typesCounters[(short)type] = networkDataModel[type].Count;
                }
            }

            return typesCounters;
        }

        /// <summary>
        /// Inserts entity into the network model.
        /// </summary>
        /// <param name="rd">Description of the resource that should be inserted</param>        
        private void InsertEntity(ResourceDescription rd)
        {
            if (rd == null)
            {
                Logger.LogInfo("Insert entity is not done because update operation is empty.");
                return;
            }

            long globalId = rd.Id;
            Logger.LogInfo($"Inserting entity with GID: 0x{globalId:X16}");

            // check if mapping for specified global id already exists			
            if (this.EntityExistsInIncomingData(globalId))
            {
                string message = String.Format("Failed to insert entity because entity with specified GID: 0x{0:X16} already exists in network model.", globalId);
                Logger.LogError(message);
                throw new Exception(message);
            }

            try
            {
                // find type
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);

                // get container or create container 
                Container incomingContainer = null;

                //get container from incoming model
                if (incomingNetworkDataModel.ContainsKey(type))
                {
                    incomingContainer = incomingNetworkDataModel[type];

                    //get container from current model
                    if (networkDataModel.ContainsKey(type))
                    {
                        Container currentContainer = networkDataModel[type];

                        if (currentContainer.GetHashCode() == incomingContainer.GetHashCode())
                        {
                            incomingContainer = GetContainerShallowCopy(type, currentContainer);
                        }
                    }
                }
                //create new container or make the shallow copy
                else
                {
                    incomingContainer = new Container();
                    incomingNetworkDataModel.Add(type, incomingContainer);
                    Logger.LogDebug($"Container [{type}, HashCode: 0x{incomingContainer.GetHashCode():X16}] created and added to Incoming model.");

                }

                // create entity and add it to container
                IdentifiedObject io = incomingContainer.CreateEntity(globalId);

                // apply properties on created entity
                if (rd.Properties != null)
                {
                    foreach (Property property in rd.Properties)
                    {
                        // globalId must not be set as property
                        if (property.Id == ModelCode.IDOBJ_GID)
                        {
                            continue;
                        }

                        if (property.Type == PropertyType.Reference)
                        {
                            // if property is a reference to another entity 
                            long targetGlobalId = property.AsReference();

                            if (targetGlobalId != 0)
                            {
                                if (!EntityExistsInIncomingData(targetGlobalId))
                                {
                                    string message = string.Format("Failed to get target entity with GID: 0x{0:X16}.", targetGlobalId);
                                    throw new Exception(message);
                                }

                                // find type
                                DMSType targetType = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(targetGlobalId);

                                //get container from incoming model
                                Container incomingTargetContainer = incomingNetworkDataModel[targetType];
                                // get referenced entity for update from incoming model
                                IdentifiedObject incomingTargetEntity = incomingTargetContainer.Entities[targetGlobalId];

                                if (EntityExists(targetGlobalId))
                                {
                                    Container currentTargetContainer = networkDataModel[targetType];

                                    if (currentTargetContainer.GetHashCode() == incomingTargetContainer.GetHashCode())
                                    {
                                        incomingTargetContainer = GetContainerShallowCopy(targetType, currentTargetContainer);
                                    }

                                    IdentifiedObject currentTargetEntity = currentTargetContainer.Entities[targetGlobalId];

                                    if (incomingTargetEntity.GetHashCode() == currentTargetEntity.GetHashCode())
                                    {
                                        incomingTargetEntity = GetEntityShallowCopy(targetGlobalId, incomingTargetContainer, currentTargetEntity);
                                    }
                                }

                                incomingTargetEntity.AddReference(property.Id, io.GlobalId);
                            }

                            io.SetProperty(property);
                        }
                        else
                        {
                            io.SetProperty(property);
                        }
                    }
                }

                Logger.LogInfo($"Inserting entity with GID: 0x{globalId:X16} successfully finished.");
            }
            catch (Exception ex)
            {
                string message = String.Format("Failed to insert entity (GID: 0x{0:X16}) into model. {1}", rd.Id, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Updates entity in block model.
        /// </summary>
        /// <param name="rd">Description of the resource that should be updated</param>		
        private void UpdateEntity(ResourceDescription rd)
        {
            if (rd == null || rd.Properties == null && rd.Properties.Count == 0)
            {
                Logger.LogInfo("Update entity is not done because update operation is empty.");
                return;
            }

            try
            {
                long globalId = rd.Id;
                Logger.LogInfo($"Updating entity with GID: 0x{globalId:X16}.");

                if (!this.EntityExistsInIncomingData(globalId))
                {
                    string message = String.Format("Failed to update entity because entity with specified GID: 0x{0:X16} does not exist in network model.", globalId);
                    Logger.LogError(message);
                    throw new Exception(message);
                }

                // find type
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
                //get container from incoming model
                Container incomingContainer = incomingNetworkDataModel[type];
                //get entity form incoming container
                IdentifiedObject incomingEntity = incomingContainer.Entities[globalId];

                //get container from current model
                if (networkDataModel.ContainsKey(type))
                {
                    Container currentContainer = networkDataModel[type];

                    if (currentContainer.GetHashCode() == incomingContainer.GetHashCode())
                    {
                        incomingContainer = GetContainerShallowCopy(type, currentContainer);
                    }

                    if (currentContainer.Entities.ContainsKey(globalId))
                    {
                        IdentifiedObject currentEntity = currentContainer.Entities[globalId];

                        if (currentEntity.GetHashCode() == incomingEntity.GetHashCode())
                        {
                            incomingEntity = GetEntityShallowCopy(globalId, incomingContainer, currentEntity);
                        }
                    }
                }

                // updating properties of entity
                foreach (Property property in rd.Properties)
                {
                    if (property.Type == PropertyType.Reference)
                    {
                        long oldTargetGlobalId = incomingEntity.GetProperty(property.Id).AsReference();

                        if (oldTargetGlobalId != 0)
                        {
                            if (!EntityExistsInIncomingData(oldTargetGlobalId))
                            {
                                string message = string.Format("Failed to get old target entity with GID: 0x{0:X16}.", oldTargetGlobalId);
                                throw new Exception(message);
                            }

                            // find type
                            DMSType oldTargetType = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(oldTargetGlobalId);
                            //get container from incoming model
                            Container incomingOldTargetContainer = incomingNetworkDataModel[oldTargetType];
                            // get referenced entity for update from incoming model
                            IdentifiedObject incomingOldTargetEntity = incomingOldTargetContainer.Entities[oldTargetGlobalId];

                            //get container from current model
                            if (EntityExists(oldTargetGlobalId))
                            {
                                Container currentOldTargetContainer = networkDataModel[oldTargetType];

                                if (currentOldTargetContainer.GetHashCode() == incomingOldTargetContainer.GetHashCode())
                                {
                                    incomingOldTargetContainer = GetContainerShallowCopy(oldTargetType, currentOldTargetContainer);
                                }

                                IdentifiedObject currentOldTargetEntity = currentOldTargetContainer.Entities[oldTargetGlobalId];

                                if (incomingOldTargetEntity.GetHashCode() == currentOldTargetEntity.GetHashCode())
                                {
                                    incomingOldTargetEntity = GetEntityShallowCopy(oldTargetGlobalId, incomingOldTargetContainer, currentOldTargetEntity);
                                }
                            }

                            incomingOldTargetEntity.RemoveReference(property.Id, globalId);
                        }

                        // updating reference of entity
                        long targetGlobalId = property.AsReference();

                        if (targetGlobalId != 0)
                        {
                            if (!EntityExistsInIncomingData(targetGlobalId))
                            {
                                string message = string.Format("Failed to get target entity with GID: 0x{0:X16}.", targetGlobalId);
                                throw new Exception(message);
                            }

                            // find type
                            DMSType targetType = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(targetGlobalId);
                            //get container from incoming model
                            Container incomingTargetContainer = incomingNetworkDataModel[targetType];
                            // get referenced entity for update from incoming model
                            IdentifiedObject incomingTargetEntity = incomingTargetContainer.Entities[targetGlobalId];

                            //get container from current model
                            if (EntityExists(targetGlobalId))
                            {
                                Container currentTargetContainer = networkDataModel[targetType];

                                if (currentTargetContainer.GetHashCode() == incomingTargetContainer.GetHashCode())
                                {
                                    incomingTargetContainer = GetContainerShallowCopy(targetType, currentTargetContainer);
                                }

                                IdentifiedObject currentTargetEntity = currentTargetContainer.Entities[targetGlobalId];

                                if (incomingTargetEntity.GetHashCode() == currentTargetEntity.GetHashCode())
                                {
                                    incomingTargetEntity = GetEntityShallowCopy(targetGlobalId, incomingTargetContainer, currentTargetEntity);
                                }
                            }

                            incomingTargetEntity.AddReference(property.Id, globalId);
                        }

                        // update value of the property in specified entity
                        incomingEntity.SetProperty(property);
                    }
                    else
                    {
                        // update value of the property in specified entity
                        incomingEntity.SetProperty(property);
                    }
                }

                Logger.LogInfo($"Updating entity with GID: 0x{globalId:X16} successfully finished.");
            }
            catch (Exception ex)
            {
                string message = String.Format("Failed to update entity (GID: 0x{0:X16}) in model. {1} ", rd.Id, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Deletes resource from the netowrk model.
        /// </summary>
        /// <param name="rd">Description of the resource that should be deleted</param>		
        private void DeleteEntity(ResourceDescription rd)
        {
            if (rd == null)
            {
                Logger.LogInfo("Delete entity is not done because update operation is empty.");
                return;
            }

            try
            {
                long globalId = rd.Id;
                Logger.LogInfo($"Deleting entity with GID: 0x{globalId:X16}");

                // check if entity exists
                if (!this.EntityExistsInIncomingData(globalId))
                {
                    string message = String.Format("Failed to delete entity because entity with specified GID: 0x{0:X16} does not exist in network model.", globalId);
                    Logger.LogError(message);
                    throw new Exception(message);
                }

                // find type
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
                //get container from incoming model
                Container incomingContainer = incomingNetworkDataModel[type];
                //entity to be deleted
                IdentifiedObject incomingEntity = incomingContainer.Entities[globalId];

                //get container from current model
                if (networkDataModel.ContainsKey(type))
                {
                    Container currentContainer = networkDataModel[type];

                    if (currentContainer.GetHashCode() == incomingContainer.GetHashCode())
                    {
                        incomingContainer = GetContainerShallowCopy(type, currentContainer);
                    }

                    if (currentContainer.Entities.ContainsKey(globalId))
                    {
                        IdentifiedObject currentEntity = currentContainer.Entities[globalId];

                        if (currentEntity.GetHashCode() == incomingEntity.GetHashCode())
                        {
                            incomingEntity = GetEntityShallowCopy(globalId, incomingContainer, currentEntity);
                        }
                    }
                }

                // check if entity could be deleted (if it is not referenced by any other entity)
                if (incomingEntity.IsReferenced)
                {
                    Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();
                    incomingEntity.GetReferences(references, TypeOfReference.Target);

                    StringBuilder sb = new StringBuilder();

                    foreach (KeyValuePair<ModelCode, List<long>> kvp in references)
                    {
                        foreach (long referenceGlobalId in kvp.Value)
                        {
                            sb.AppendFormat("0x{0:X16}, ", referenceGlobalId);
                        }
                    }

                    string message = String.Format("Failed to delete entity (GID: 0x{0:X16}) because it is referenced by entities with GIDs: {1}.", globalId, sb.ToString());
                    Logger.LogError(message);
                    throw new Exception(message);
                }

                // find property ids
                List<ModelCode> propertyIds = resourcesDescs.GetAllSettablePropertyIdsForEntityId(incomingEntity.GlobalId);

                // remove references
                Property property = null;
                foreach (ModelCode propertyId in propertyIds)
                {
                    PropertyType propertyType = Property.GetPropertyType(propertyId);

                    if (propertyType == PropertyType.Reference)
                    {
                        property = incomingEntity.GetProperty(propertyId);

                        // get target entity and remove reference to another entity
                        long targetGlobalId = property.AsReference();

                        if (targetGlobalId != 0)
                        {
                            if (!EntityExistsInIncomingData(targetGlobalId))
                            {
                                string message = string.Format("Failed to get target entity with GID: 0x{0:X16}.", targetGlobalId);
                                throw new Exception(message);
                            }

                            // find type
                            DMSType targetType = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(targetGlobalId);
                            //get container from incoming model
                            Container incomingTargetContainer = incomingNetworkDataModel[targetType];
                            // get incoming target entity
                            IdentifiedObject incomingTargetEntity = incomingTargetContainer.Entities[targetGlobalId];

                            //get container from current model
                            if (EntityExists(targetGlobalId))
                            {
                                Container currentTargetContainer = networkDataModel[targetType];

                                if (currentTargetContainer.GetHashCode() == incomingTargetContainer.GetHashCode())
                                {
                                    incomingTargetContainer = GetContainerShallowCopy(targetType, currentTargetContainer);
                                }

                                IdentifiedObject currentTargetEntity = currentTargetContainer.Entities[targetGlobalId];

                                if (incomingTargetEntity.GetHashCode() == currentTargetEntity.GetHashCode())
                                {
                                    incomingTargetEntity = GetEntityShallowCopy(targetGlobalId, incomingTargetContainer, currentTargetEntity);
                                }
                            }

                            incomingTargetEntity.RemoveReference(property.Id, globalId);
                        }
                    }
                }

                // remove entity form netowrk model
                incomingContainer.RemoveEntity(globalId);
                Logger.LogInfo($"Deleting entity with GID: 0x{globalId:X16} successfully finished.");
            }
            catch (Exception ex)
            {
                string message = String.Format("Failed to delete entity (GID: 0x{0:X16}) from model. {1}", rd.Id, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Returns related gids with source according to the association 
        /// </summary>
        /// <param name="source">source id</param>		
        /// <param name="association">desinition of association</param>
        /// <returns>related gids</returns>
        private List<long> ApplyAssocioationOnSource(long source, Association association)
        {
            List<long> relatedGids = new List<long>();

            if (association == null)
            {
                association = new Association();
            }

            IdentifiedObject io = GetEntity(source);

            if (!io.HasProperty(association.PropertyId))
            {
                throw new Exception(string.Format("Entity with GID: 0x{0:X16} does not contain prperty with Id: {1}.", source, association.PropertyId));
            }

            Property propertyRef = null;
            if (Property.GetPropertyType(association.PropertyId) == PropertyType.Reference)
            {
                propertyRef = io.GetProperty(association.PropertyId);
                long relatedGidFromProperty = propertyRef.AsReference();

                if (relatedGidFromProperty != 0)
                {
                    if (association.Type == 0 || (short)ModelCodeHelper.GetTypeFromModelCode(association.Type) == ModelCodeHelper.ExtractTypeFromGlobalId(relatedGidFromProperty))
                    {
                        relatedGids.Add(relatedGidFromProperty);
                    }
                }
            }
            else if (Property.GetPropertyType(association.PropertyId) == PropertyType.ReferenceVector)
            {
                propertyRef = io.GetProperty(association.PropertyId);
                List<long> relatedGidsFromProperty = propertyRef.AsReferences();

                if (relatedGidsFromProperty != null)
                {
                    foreach (long relatedGidFromProperty in relatedGidsFromProperty)
                    {
                        if (association.Type == 0 || (short)ModelCodeHelper.GetTypeFromModelCode(association.Type) == ModelCodeHelper.ExtractTypeFromGlobalId(relatedGidFromProperty))
                        {
                            relatedGids.Add(relatedGidFromProperty);
                        }
                    }
                }
            }
            else
            {
                throw new Exception(string.Format("Association propertyId: {0} is not reference or reference vector type.", association.PropertyId));
            }

            return relatedGids;
        }

        private Container GetContainerShallowCopy(DMSType type, Container currentContainer)
        {
            Container incomingContainer = currentContainer.Clone();
            incomingNetworkDataModel[type] = incomingContainer;
            Logger.LogDebug($"Incoming model Container [{type}, HashCode: 0x{incomingContainer.GetHashCode():X16}] is shallow copy of Current model Container [HashCode: 0x{currentContainer.GetHashCode():X16}].");
            return incomingContainer;
        }

        private IdentifiedObject GetEntityShallowCopy(long globalId, Container incomingContainer, IdentifiedObject currentEntity)
        {
            IdentifiedObject incomingEntity = currentEntity.Clone();
            incomingContainer.Entities[globalId] = incomingEntity;
            Logger.LogDebug($"Incoming model Entity [0x{globalId:X16}, HashCode: 0x{incomingEntity.GetHashCode():X16}] is shallow copy of Current model Entity [HashCode: 0x{currentEntity.GetHashCode():X16}].");
            return incomingEntity;
        }
        #endregion

        #region MongoDB
        private void InitializeBsonSerializer()
        {
            BsonSerializer.RegisterSerializer(new EnumSerializer<DMSType>(BsonType.String));
            BsonSerializer.RegisterSerializer(new Int64Serializer(BsonType.String));

            BsonClassMap.RegisterClassMap<BaseVoltage>();
            BsonClassMap.RegisterClassMap<Terminal>();
            BsonClassMap.RegisterClassMap<ConnectivityNode>();
            BsonClassMap.RegisterClassMap<PowerTransformer>();
            BsonClassMap.RegisterClassMap<EnergySource>();
            BsonClassMap.RegisterClassMap<EnergyConsumer>();
            BsonClassMap.RegisterClassMap<TransformerWinding>();
            BsonClassMap.RegisterClassMap<Fuse>();
            BsonClassMap.RegisterClassMap<Disconnector>();
            BsonClassMap.RegisterClassMap<Breaker>();
            BsonClassMap.RegisterClassMap<LoadBreakSwitch>();
            BsonClassMap.RegisterClassMap<ACLineSegment>();
            BsonClassMap.RegisterClassMap<Discrete>();
            BsonClassMap.RegisterClassMap<Analog>();
            BsonClassMap.RegisterClassMap<SynchronousMachine>();
        }

        private void SaveDelta(Delta delta)
        {
            long deltaVersion = 0, networkModelVersion = 0, newestVersion = 0;
            var counterCollection = db.GetCollection<ModelVersionDocument>("versions");

            GetVersions(ref networkModelVersion, ref deltaVersion, counterCollection);

            newestVersion = deltaVersion > networkModelVersion ? deltaVersion : networkModelVersion;

            delta.Id = ++newestVersion;

            try
            {
                counterCollection.ReplaceOne(new BsonDocument("_id", "deltaVersion"), new ModelVersionDocument { Id = "deltaVersion", Version = delta.Id }, new UpdateOptions { IsUpsert = true });
                var deltaCollection = db.GetCollection<Delta>("deltas");
                deltaCollection.InsertOne(delta);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error on database: {e.Message}.", e);
            }
        }

        public void SaveNetworkModel()
        {
            long networkModelVersion = 0, deltaVersion = 0;

            var versionsCollection = db.GetCollection<ModelVersionDocument>("versions");
            var networkModelCollection = db.GetCollection<NetworkDataModelDocument>("networkModels");
            var deltasCollection = db.GetCollection<Delta>("deltas");
            GetVersions(ref networkModelVersion, ref deltaVersion, versionsCollection);

            if ((networkModelVersion == 0 && deltaVersion == 0) || (networkModelVersion > deltaVersion)) //there is no model and deltas or model in use is already saved, so there is no need for datamodel storing
            {
                return;
            }
            else if (deltaVersion > networkModelVersion) //there is new deltas since startup, so store current dataModel
            {

                networkModelCollection.InsertOne(new NetworkDataModelDocument { Id = deltaVersion + 1, NetworkModel = networkDataModel });
                versionsCollection.ReplaceOne(new BsonDocument("_id", "networkModelVersion"), new ModelVersionDocument { Id = "networkModelVersion", Version = deltaVersion + 1 }, new UpdateOptions { IsUpsert = true });
            }
            else
            {
                throw new Exception("SaveNetwrokModel error!");  //better message needed :((
            }
        }

        private void GetVersions(ref long networkModelVersion, ref long deltaVersion, IMongoCollection<ModelVersionDocument> versionsCollection)
        {
            var networkModelVersionFilter = Builders<ModelVersionDocument>.Filter.Eq("_id", "networkModelVersion");
            var deltaVersionFilter = Builders<ModelVersionDocument>.Filter.Eq("_id", "deltaVersion");

            if (versionsCollection.Find(networkModelVersionFilter).CountDocuments() > 0)
            {
                networkModelVersion = versionsCollection.Find(networkModelVersionFilter).First().Version;
            }

            if (versionsCollection.Find(deltaVersionFilter).CountDocuments() > 0)
            {
                deltaVersion = versionsCollection.Find(deltaVersionFilter).First().Version;
            }
        }

        private List<Delta> ReadAllDeltas(long deltaVersion, long networkModelVersion)
        {

            List<Delta> deltasFromDb = new List<Delta>();

            var collection = db.GetCollection<Delta>("deltas");

            for (long deltaV = networkModelVersion + 1; deltaV <= deltaVersion; deltaV++)
            {
                var deltaFilter = Builders<Delta>.Filter.Eq("_id", deltaV);
                deltasFromDb.Add(collection.Find(deltaFilter).First());
            }

            return deltasFromDb;
        }
        #endregion MongoDB
    }
}
