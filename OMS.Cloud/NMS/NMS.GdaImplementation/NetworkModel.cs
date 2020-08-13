using NMS.DataModel;
using NMS.GdaImplementation.GDA;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.NMS;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.TmsContracts;
using OMS.Common.TmsContracts.Notifications;
using OMS.Common.WcfClient.TMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpdateResult = OMS.Common.NmsContracts.GDA.UpdateResult;

namespace NMS.GdaImplementation
{
    enum NetworkModelState
    {
        NOT_INITIALIZED = 1,
        CURRENTLY_INITIALIZING = 2,
        INITIALIZED = 3,
        IN_TRANSACTION = 4,
    }

    public class NetworkModel : ITransactionActorContract
    {
        #region Fields
        private readonly string baseLogString;
        private readonly MongoAccess mongoDbAccess;

        /// <summary>
        /// ModelResourceDesc class contains metadata of the model
        /// </summary>
        private readonly ModelResourcesDesc resourcesDescs;

        private NetworkModelState networkModelState;
        //private bool isModelInitialized; //todo: zameniti sa nekim semaforom ako neko bude imao vremena
        //private bool isTransactionInProgress;

        private Delta currentDelta;

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

        #region Private Properties
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        #region Public Properties
        /// <summary>
        /// Dictionary which contains all data: Key - DMSType, Value - Container
        /// </summary>
        private Dictionary<DMSType, Container> networkDataModel;

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

        public long CurrentVersion { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the Model class.
        /// </summary>
        public NetworkModel()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.mongoDbAccess = new MongoAccess();
            this.resourcesDescs = new ModelResourcesDesc();

            this.networkModelState = NetworkModelState.NOT_INITIALIZED; 
            //this.isModelInitialized = false;
            //this.isTransactionInProgress = false;
    }

        public async Task InitializeNetworkModel()
        {
            //this.isModelInitialized = false;
            this.networkModelState = NetworkModelState.CURRENTLY_INITIALIZING;

            long latestNetworkModelVersion = mongoDbAccess.GetLatestNetworkModelVersions();
            long latestDeltaVersion = mongoDbAccess.GetLatestDeltaVersions();
            
            if (latestNetworkModelVersion < 0 || latestDeltaVersion < 0)
            {
                string errorMessage = $"{baseLogString} InitializeNetworkModel => latest version has a negative value.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            long latestVersion = latestDeltaVersion > latestNetworkModelVersion ? latestDeltaVersion : latestNetworkModelVersion;

            //If either one is true
            //  1) "No NetworkModels or Deltas in database." => latestNetworkModelVersion == 0 && latestDeltaVersion == 0
            //  2) "Latest NetworkModel in use is already saved." => latestNetworkModelVersion > latestDeltaVersion
            //there is no need to save NetworkModel

            if(latestVersion == CurrentVersion)
            {
                //no need to do anything
            }
            else if (latestNetworkModelVersion == 0 && latestDeltaVersion == 0)
            {
                CurrentVersion = 0;
            }
            else if (latestNetworkModelVersion < latestDeltaVersion)
            {
                Logger.LogDebug($"{baseLogString} InitializeNetworkModel => Delta version is higher then network model version.");

                networkDataModel = mongoDbAccess.GetNetworkModel(latestNetworkModelVersion);
                List<Delta> deltas = mongoDbAccess.GetAllDeltasFromVersionRange(latestNetworkModelVersion + 1, latestDeltaVersion);

                foreach (Delta delta in deltas)
                {
                    try
                    {
                        await ApplyDelta(delta);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"{baseLogString} InitializeNetworkModel => Error while applying delta (id: {delta.Id}) durning service initialization. {ex.Message}", ex);
                    }
                }

                CurrentVersion = latestVersion + 1;
                mongoDbAccess.SaveNetworkModel(NetworkDataModel, CurrentVersion);
            }
            else if (latestNetworkModelVersion > latestDeltaVersion)
            {
                this.networkDataModel = mongoDbAccess.GetNetworkModel(latestNetworkModelVersion);
                CurrentVersion = latestNetworkModelVersion;
            }
            else
            {
                if(latestNetworkModelVersion == latestDeltaVersion)
                {
                    string errorMessage = $"{baseLogString} InitializeNetworkModel => Invalid versions. LatestNetworkModelVersion equals LatestDeltaVersion. Value: {latestNetworkModelVersion}";
                    Logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }

                throw new NotImplementedException($"{baseLogString} InitializeNetworkModel => Unknown scenario. LatestNetworkModelVersion: {latestNetworkModelVersion}, LatestDeltaVersion: {latestDeltaVersion}");
            }

            //this.isModelInitialized = true;
            this.networkModelState = NetworkModelState.INITIALIZED;
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

        #region INetworkModelGDAContract Methods
        public async Task<UpdateResult> ApplyDelta(Delta delta)
        {
            UpdateResult updateResult = new UpdateResult();
            
            //DELTAS FROM IMPORTER WILL BE PROCCESSED ONLY IF MODEL IS IN INITIALIZED STATE
            if (delta.DeltaOrigin == DeltaOriginType.ImporterDelta && networkModelState != NetworkModelState.INITIALIZED)
            {
                updateResult.Result = ResultType.Failed;
                string message = $"Delta is rejected. NetworkModel is currently in {networkModelState} state.";
                updateResult.Message = message;
                Logger.LogWarning(message);

                return updateResult;
            }

            currentDelta = delta;

            //shallow copy 
            incomingNetworkDataModel = new Dictionary<DMSType, Container>(NetworkDataModel);
            Logger.LogDebug($"Incoming model [HashCode: 0x{incomingNetworkDataModel.GetHashCode():X16}] is shallow copy of Current model [HashCode: 0x{networkDataModel.GetHashCode():X16}].");

            try
            {
                Logger.LogInformation("Applying delta to network model.");

                Dictionary<short, int> typesCounters = GetCounters();
                Dictionary<long, long> globalIdPairs = new Dictionary<long, long>();

                if (delta.DeltaOrigin != DeltaOriginType.DatabaseDelta)
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

                if (delta.DeltaOrigin == DeltaOriginType.DatabaseDelta)
                {
                    await Commit();
                    updateResult.Result = ResultType.Succeeded;
                }
                else if(delta.DeltaOrigin == DeltaOriginType.ImporterDelta)
                {
                    if(await StartDistributedTransaction(delta))
                    {
                        updateResult.Result = ResultType.Succeeded;
                    }
                    else
                    {
                        updateResult.Result = ResultType.Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = $"Applying delta to network model failed. Message: {ex.Message}";
                Logger.LogError(message, ex);

                updateResult.Result = ResultType.Failed;
                updateResult.Message = message;
                currentDelta = null;
            }
            finally
            {
                if (updateResult.Result == ResultType.Succeeded)
                {
                    string message = "Applying delta to network model SUCCESSFULLY finished.";
                    Logger.LogInformation(message);
                    updateResult.Message = $"{updateResult.Message}{Environment.NewLine}{message}";
                }
                else if(updateResult.Result == ResultType.Failed)
                {
                    string message = "Applying delta to network model UNSUCCESSFULLY finished.";
                    Logger.LogInformation(message);
                    updateResult.Message = $"{updateResult.Message}{Environment.NewLine}{message}";
                }
            }

            return updateResult;
        }

        /// <summary>
        /// Gets resource description for entity requested by globalId.
        /// </summary>
        /// <param name="globalId">Id of the entity</param>
        /// <param name="properties">List of requested properties</param>		
        /// <returns>Resource description of the specified entity</returns>
        public async Task<ResourceDescription> GetValues(long globalId, List<ModelCode> properties)
        {
            while (networkModelState == NetworkModelState.NOT_INITIALIZED || networkModelState == NetworkModelState.CURRENTLY_INITIALIZING)
            {
                await Task.Delay(1000);
            }

            Logger.LogDebug($"Getting values for GID: 0x{globalId:X16}.");

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

                Logger.LogDebug($"Getting values for GID: 0x{globalId:X16} succedded.");

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
        public async Task<ResourceIterator> GetExtentValues(ModelCode entityType, List<ModelCode> properties)
        {
            while (networkModelState == NetworkModelState.NOT_INITIALIZED || networkModelState == NetworkModelState.CURRENTLY_INITIALIZING)
            {
                await Task.Delay(1000);
            }

            Logger.LogDebug($"Getting extent values for entity type: {entityType}.");

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

                ResourceIterator ri = new ResourceIterator(globalIds, class2PropertyIDs, this);

                Logger.LogDebug($"Getting extent values for entity type: {entityType} succedded.");

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
        public async Task<ResourceIterator> GetRelatedValues(long source, List<ModelCode> properties, Association association)
        {
            while (networkModelState == NetworkModelState.NOT_INITIALIZED || networkModelState == NetworkModelState.CURRENTLY_INITIALIZING)
            {
                await Task.Delay(1000);
            }

            Logger.LogDebug($"Getting related values for source: 0x{source:X16}.");

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

                ResourceIterator ri = new ResourceIterator(relatedGids, class2PropertyIDs, this);

                Logger.LogDebug($"Getting related values for source: 0x{source:X16} succedded.");

                return ri;
            }
            catch (Exception ex)
            {
                string message = String.Format("Failed to get related values for source GID: 0x{0:X16}. {1}.", source, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        #region Private Methods
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
                Logger.LogInformation("Insert entity is not done because update operation is empty.");
                return;
            }

            long globalId = rd.Id;
            Logger.LogInformation($"Inserting entity with GID: 0x{globalId:X16}");

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

                Logger.LogInformation($"Inserting entity with GID: 0x{globalId:X16} SUCCESSFULLY finished.");
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
                Logger.LogInformation("Update entity is not done because update operation is empty.");
                return;
            }

            try
            {
                long globalId = rd.Id;
                Logger.LogInformation($"Updating entity with GID: 0x{globalId:X16}.");

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

                Logger.LogInformation($"Updating entity with GID: 0x{globalId:X16} SUCCESSFULLY finished.");
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
                Logger.LogInformation("Delete entity is not done because update operation is empty.");
                return;
            }

            try
            {
                long globalId = rd.Id;
                Logger.LogInformation($"Deleting entity with GID: 0x{globalId:X16}");

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
                Logger.LogInformation($"Deleting entity with GID: 0x{globalId:X16} SUCCESSFULLY finished.");
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
        #endregion Private Methods
        
        #endregion INetworkModelGDAContract Methods

        #region ITransactionActorContract
        public Task<bool> Prepare()
        {
            return Task.Run(() =>
            {
                return oldNetworkDataModel != null && networkDataModel != null && oldNetworkDataModel.GetHashCode() != networkDataModel.GetHashCode();
            });
        }

        /// <summary>
        /// 2PhaseCommitProtocol - Commit Phase
        /// </summary>
        /// <param name="isInitialization">Indicates if changes are commited in initialization step, after the Network Model service has started.</param>
        /// <returns></returns>
        public Task Commit()
        {
            return Task.Run(() =>
            {
                //this.isTransactionInProgress = false;
                this.networkModelState = NetworkModelState.INITIALIZED;

                if (currentDelta != null && currentDelta.DeltaOrigin == DeltaOriginType.ImporterDelta)
                {
                    long latestVersion = mongoDbAccess.GetLatestVersion();

                    currentDelta.Id = latestVersion + 1;
                    mongoDbAccess.SaveDelta(currentDelta);

                    CurrentVersion = currentDelta.Id;
                }

                currentDelta = null;
                oldNetworkDataModel = null;
                Logger.LogDebug($"{baseLogString} Commit => Current model [HashCode: 0x{incomingNetworkDataModel.GetHashCode():X16}] commited. Old model is set to null.");
            });
        }

        public Task Rollback()
        {
            return Task.Run(() =>
            {
                //this.isTransactionInProgress = false;
                this.networkModelState = NetworkModelState.INITIALIZED;

                currentDelta = null;
                networkDataModel = oldNetworkDataModel;
                Logger.LogDebug($"{baseLogString} Rollback => Current model [HashCode: 0x{networkDataModel.GetHashCode():X16}] rollbacked to Old model [HashCode: 0x{oldNetworkDataModel.GetHashCode():X16}].");
            });
        }

        #region Private Members
        private async Task<bool> StartDistributedTransaction(Delta delta)
        {
            //this.isTransactionInProgress = true;
            this.networkModelState = NetworkModelState.IN_TRANSACTION;
            var transactionActors = NetorkModelUpdateTransaction.Instance.TransactionActorsNames;
            var modelChanges = CreateModelChangesData(delta);
            
            var transactionCoordinatorClient = TransactionCoordinatorClient.CreateClient();
            await transactionCoordinatorClient.StartDistributedTransaction(DistributedTransactionNames.NetworkModelUpdateTransaction, transactionActors);
            Logger.LogDebug($"{baseLogString} StartDistributedTransaction => StartDistributedTransaction('{DistributedTransactionNames.NetworkModelUpdateTransaction}', transactionActors count: {transactionActors.Count()}) called.");

            var tasks = new List<Task<Tuple<string, bool>>>();
            foreach (var transactionActorName in NetorkModelUpdateTransaction.Instance.TransactionActorsNames)
            {
                if(transactionActorName == MicroserviceNames.NmsGdaService)
                {
                    //No need to notify NMS about update
                    continue;
                }
                else
                {
                    //Notifying service about Model Update
                    tasks.Add(Task.Run(async () =>
                    {
                        INotifyNetworkModelUpdateContract notifyNetworkModelUpdateClient = NotifyNetworkModelUpdateClient.CreateClient(transactionActorName);
                        Logger.LogDebug($"{baseLogString} StartDistributedTransaction => calling Notify() method for '{transactionActorName}' Transaction actor.");

                        var taskSuccess = await notifyNetworkModelUpdateClient.Notify(modelChanges);
                        Logger.LogDebug($"{baseLogString} StartDistributedTransaction => Notify() method invoked on '{transactionActorName}' Transaction actor.");

                        return new Tuple<string, bool>(transactionActorName, taskSuccess);
                    }));
                }
            }

            var taskResults = await Task.WhenAll(tasks.ToArray());
            var notifyPhaseSuccess = true;

            //Checking task results
            foreach (var taskResult in taskResults)
            {
                var actorName = taskResult.Item1;
                var notifySuccess = taskResult.Item2;

                notifyPhaseSuccess = notifyPhaseSuccess && notifySuccess;

                if (notifyPhaseSuccess)
                {
                    Logger.LogInformation($"{baseLogString} StartDistributedTransaction => Notify on Transaction actor: {actorName} finsihed SUCCESSFULLY.");
                }
                else
                {
                    Logger.LogInformation($"{baseLogString} StartDistributedTransaction => Notify on Transaction actor: {actorName} finsihed UNSUCCESSFULLY.");
                    break;
                }
            }

            bool nmsEnlistSuccess;

            //IF ALL notify tasks return true
            if (notifyPhaseSuccess)
            {
                nmsEnlistSuccess = await EnlistNmsTransactionActor();
            }
            else
            {
                //NO need to enlist nms if any of notify taks failed
                nmsEnlistSuccess = false;
            }

            await transactionCoordinatorClient.FinishDistributedTransaction(DistributedTransactionNames.NetworkModelUpdateTransaction, nmsEnlistSuccess);
            Logger.LogDebug($"FinishDistributedUpdate() invoked on Transaction Coordinator with parameter 'success' value: {nmsEnlistSuccess}.");

            return nmsEnlistSuccess;
        }

        private Dictionary<DeltaOpType, List<long>> CreateModelChangesData(Delta delta)
        {
            var modelChanges = new Dictionary<DeltaOpType, List<long>>()
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

            return modelChanges;
        }

        private async Task<bool> EnlistNmsTransactionActor()
        {
            var transactionEnlistmentClient = TransactionEnlistmentClient.CreateClient();
            bool nmsEnlistSuccess = await transactionEnlistmentClient.Enlist(DistributedTransactionNames.NetworkModelUpdateTransaction, MicroserviceNames.NmsGdaService);
            Logger.LogDebug("{baseLogString} StartDistributedTransaction => Enlist() method invoked on Transaction Coordinator.");

            return nmsEnlistSuccess;
        }
        #endregion Private Members

        #endregion ITransactionActorContract

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
    }
}
