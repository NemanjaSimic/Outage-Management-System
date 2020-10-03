using Common.OMS;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.TmsContracts;
using OMS.Common.WcfClient.NMS;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerImplementation.DistributedTransaction
{
    public class OmsHistoryTransactionActor : ITransactionActorContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        private UnitOfWork unitOfWork;
        private ModelResourcesDesc modelResourcesDesc;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        #region Reliable Dictionaries
        private bool isOpenedSwitchesInitialized;
        private bool isUnenergizedConsumersInitialized;
        private bool isHistoryModelChangesInitialized;
        private bool ReliableDictionariesInitialized
        {
            get
            {
                return true;
            }
        }

        private ReliableDictionaryAccess<long, long> OpenedSwitches { get; set; }
        private ReliableDictionaryAccess<long, long> UnenergizedConsumers { get; set; }
        private ReliableDictionaryAccess<byte, List<long>> HistoryModelChanges { get; set; }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs eventArgs)
        {
            try
            {
                await InitializeReliableCollections(eventArgs);
            }
            catch (FabricNotPrimaryException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => NotPrimaryException. To be ignored.");
            }
            catch (FabricObjectClosedException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => FabricObjectClosedException. To be ignored.");
            }
            catch (COMException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => {typeof(COMException)}. To be ignored.");
            }
        }

        private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.OpenedSwitches)
                {
                    OpenedSwitches = await ReliableDictionaryAccess<long, long>.Create(stateManager, ReliableDictionaryNames.OpenedSwitches);
                    this.isOpenedSwitchesInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.OpenedSwitches}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.UnenergizedConsumers)
                {
                    UnenergizedConsumers = await ReliableDictionaryAccess<long, long>.Create(stateManager, ReliableDictionaryNames.UnenergizedConsumers);
                    this.isUnenergizedConsumersInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.UnenergizedConsumers}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.HistoryModelChanges)
                {
                    HistoryModelChanges = await ReliableDictionaryAccess<byte, List<long>>.Create(stateManager, ReliableDictionaryNames.HistoryModelChanges);
                    this.isHistoryModelChangesInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.HistoryModelChanges}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        public OmsHistoryTransactionActor(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isOpenedSwitchesInitialized = false;
            this.isUnenergizedConsumersInitialized = false;
            this.isHistoryModelChangesInitialized = false;

            this.stateManager = stateManager;
            OpenedSwitches = new ReliableDictionaryAccess<long, long>(stateManager, ReliableDictionaryNames.OpenedSwitches);
            UnenergizedConsumers = new ReliableDictionaryAccess<long, long>(stateManager, ReliableDictionaryNames.UnenergizedConsumers);
            HistoryModelChanges = new ReliableDictionaryAccess<byte, List<long>>(stateManager, ReliableDictionaryNames.HistoryModelChanges);

            this.modelResourcesDesc = new ModelResourcesDesc();
        }

        #region ITransactionActorContract
        public async Task<bool> Prepare()
        {
            bool success;

            try
            {
                Logger.LogDebug("Enter prepare method");
                this.unitOfWork = new UnitOfWork();
                List<Consumer> consumerDbEntities = this.unitOfWork.ConsumerRepository.GetAll().ToList();

                var resourceDescriptions = await GetExtentValues(ModelCode.ENERGYCONSUMER, modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONSUMER));
                var modelChangesEnumerable = await HistoryModelChanges.GetEnumerableDictionaryAsync();


                List<OutageEntity> activeOutages = this.unitOfWork.OutageRepository.GetAllActive().ToList();
               				
                this.unitOfWork.OutageRepository.RemoveRange(activeOutages);
				
                //this.unitOfWork.OutageRepository.RemoveAll();
                //this.unitOfWork.EquipmentRepository.RemoveAll();
                //this.unitOfWork.EquipmentHistoricalRepository.RemoveAll();
                //this.unitOfWork.ConsumerHistoricalRepository.RemoveAll();

                foreach (Consumer consumer in consumerDbEntities)
                {
                    if (modelChangesEnumerable[(byte)DeltaOpType.Delete].Contains(consumer.ConsumerId))
                    {
                        this.unitOfWork.ConsumerRepository.Remove(consumer);
                    }
                    else if (modelChangesEnumerable[(byte)DeltaOpType.Update].Contains(consumer.ConsumerId))
                    {
                        consumer.ConsumerMRID = resourceDescriptions[consumer.ConsumerId].GetProperty(ModelCode.IDOBJ_MRID).AsString();
                        consumer.FirstName = resourceDescriptions[consumer.ConsumerId].GetProperty(ModelCode.ENERGYCONSUMER_FIRSTNAME).AsString();
                        consumer.LastName = resourceDescriptions[consumer.ConsumerId].GetProperty(ModelCode.ENERGYCONSUMER_LASTNAME).AsString();
                        consumer.Type = (EnergyConsumerType)resourceDescriptions[consumer.ConsumerId].GetProperty(ModelCode.ENERGYCONSUMER_TYPE).AsEnum();
                        consumer.Outages.Clear();

                        this.unitOfWork.ConsumerRepository.Update(consumer);
                    }
                }

                foreach (long gid in modelChangesEnumerable[(byte)DeltaOpType.Insert])
                {
                    ModelCode type = modelResourcesDesc.GetModelCodeFromId(gid);

                    if (type != ModelCode.ENERGYCONSUMER)
                    {
                        continue;
                    }
                    
                    ResourceDescription resourceDescription = resourceDescriptions[gid];

                    if (resourceDescription == null)
                    {
                        Logger.LogWarning($"Consumer with gid 0x{gid:X16} is not in network model");
                        continue;
                    }
                    
                    Consumer consumer = new Consumer
                    {
                        ConsumerId = resourceDescription.Id,
                        ConsumerMRID = resourceDescription.GetProperty(ModelCode.IDOBJ_MRID).AsString(),
                        FirstName = resourceDescription.GetProperty(ModelCode.ENERGYCONSUMER_FIRSTNAME).AsString(),
                        LastName = resourceDescription.GetProperty(ModelCode.ENERGYCONSUMER_LASTNAME).AsString(),
                        Type = (EnergyConsumerType)resourceDescriptions[resourceDescription.Id].GetProperty(ModelCode.ENERGYCONSUMER_TYPE).AsEnum(),
                    };

                    if(this.unitOfWork.ConsumerRepository.Get(consumer.ConsumerId) == null)
                    {
                        this.unitOfWork.ConsumerRepository.Add(consumer);
                    }
                    else
                    {
                        Logger.LogWarning($"{baseLogString} Prepare => Consumer with gid 0x{consumer.ConsumerId:X16} already exists in DB. Delta Operation: {DeltaOpType.Insert}. Potential fixes: " +
                            $"{Environment.NewLine}If MongoDB is empty => Delte rows from Consumer Table. " +
                            $"{Environment.NewLine}If Mongo contains stored NetworkModel => Ignore this warn as it is part of initialization (part of the first distributed transaction).");
                    }
                }

                success = true;
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} Prepare => Exception: {e.Message}";
                Logger.LogError(message, e);
                success = false;
                this.unitOfWork.Dispose();
            }

            return success;
        }
        
        public async Task Commit()
        {
            try
            {
                this.unitOfWork.Complete();

                await OpenedSwitches.ClearAsync();
                await UnenergizedConsumers.ClearAsync();
                await HistoryModelChanges.ClearAsync();

                string message = $"{baseLogString} Commit => {MicroserviceNames.OmsHistoryDBManagerService} confirmed model changes.";
                Logger.LogInformation(message);

                await LogAllReliableCollections();
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} Commit => Exception: {e.Message}";
                Logger.LogError(message, e);
            }
            finally
            {
                this.unitOfWork.Dispose();
                this.unitOfWork = null;
            }
        }

        public async Task Rollback()
        {
            try
            {
                await HistoryModelChanges.ClearAsync();

                string message = $"{baseLogString} Rollback => {MicroserviceNames.OmsHistoryDBManagerService} rejected model changes.";
                Logger.LogInformation(message);

                await LogAllReliableCollections();
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} Rollback => Exception: {e.Message}";
                Logger.LogError(message, e);
            }
            finally
            {
                this.unitOfWork.Dispose();
                this.unitOfWork = null;
            }
        }
        
        public Task<bool> IsAlive()
        {
            return Task.Run(() => true);
        }
        #endregion ITransactionActorContract

        #region GDAHelper
        private async Task<Dictionary<long, ResourceDescription>> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            int iteratorId;

            try
            {
                INetworkModelGDAContract gdaClient = NetworkModelGdaClient.CreateClient();
                iteratorId = await gdaClient.GetExtentValues(entityType, propIds);
            }
            catch (Exception e)
            {
                string message = $"GetExtentValues => Entity type: {entityType}, Exception Message: {e.Message}.";
                Logger.LogError(message, e);
                throw e;
            }

            return await ProcessIterator(iteratorId);
        }

        private async Task<Dictionary<long, ResourceDescription>> ProcessIterator(int iteratorId)
        {
            int resourcesLeft;
            int numberOfResources = 10000;
            Dictionary<long, ResourceDescription> resourceDescriptions;

            try
            {
                INetworkModelGDAContract gdaClient = NetworkModelGdaClient.CreateClient();
                resourcesLeft = await gdaClient.IteratorResourcesTotal(iteratorId);
                resourceDescriptions = new Dictionary<long, ResourceDescription>(resourcesLeft);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> resources = await gdaClient.IteratorNext(numberOfResources, iteratorId);

                    foreach (ResourceDescription resource in resources)
                    {
                        resourceDescriptions.Add(resource.Id, resource);
                    }

                    resourcesLeft = await gdaClient.IteratorResourcesLeft(iteratorId);
                }

                await gdaClient.IteratorClose(iteratorId);
            }
            catch (Exception e)
            {
                string message = $"Failed to retrieve all Resourse descriptions with iterator {iteratorId}.";
                Logger.LogError(message, e);
                throw e;
            }

            return resourceDescriptions;
        }
        #endregion

        private async Task LogAllReliableCollections()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Reliable Collections");

            sb.AppendLine("OpenedSwitches =>");
            var openedSwitches = await OpenedSwitches.GetEnumerableDictionaryAsync();
            foreach (var element in openedSwitches)
            {
                sb.AppendLine($"Key => {element.Key}, Value => 0x{element.Value:X16}");
            }
            sb.AppendLine();

            sb.AppendLine("UnenergizedConsumers =>");
            var unenergizedConsumers = await UnenergizedConsumers.GetEnumerableDictionaryAsync();
            foreach (var element in unenergizedConsumers)
            {
                sb.AppendLine($"Key => 0x{element.Key:X16}, Value => 0x{element.Value:X16}");
            }
            sb.AppendLine();

            sb.AppendLine("HistoryModelChanges =>");
            var historyModelChanges = await HistoryModelChanges.GetEnumerableDictionaryAsync();
            foreach (var element in historyModelChanges)
            {
                sb.AppendLine($"Key => 0x{element.Key:X16}, Value => changes count: {element.Value.Count}");
            }
            sb.AppendLine();

            Logger.LogDebug($"{baseLogString} LogAllReliableCollections => {sb}");
        }
    }
}
