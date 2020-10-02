using Common.OMS;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.HistoryDBManager;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerImplementation
{
    public class HistoryDBManager : IHistoryDBManagerContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        #region Reliable Dictionaries
        private bool isUnenergizedConsumersInitialized;
        private bool isOpenedSwitchesInitialized;

        private bool ReliableDictionariesInitialized
        {
            get
            {
                return true;
                       //isUnenergizedConsumersInitialized &&
                       //isOpenedSwitchesInitialized;
            }
        }

        //MODO: translate to ReliableDictionaryAccess<long, Consumer>
        private ReliableDictionaryAccess<long, long> unenergizedConsumers;
        private ReliableDictionaryAccess<long, long> UnenergizedConsumers
        {
            get { return unenergizedConsumers; }
        }

        //MODO: translate to ReliableDictionaryAccess<long, Switch>
        private ReliableDictionaryAccess<long, long> openedSwitches;
        private ReliableDictionaryAccess<long, long> OpenedSwitches
        {
            get { return openedSwitches; }
        }

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
                    openedSwitches = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.OpenedSwitches);
                    this.isOpenedSwitchesInitialized = true;
                }
                else if (reliableStateName == ReliableDictionaryNames.UnenergizedConsumers)
                {
                    unenergizedConsumers = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.UnenergizedConsumers);
                    this.isUnenergizedConsumersInitialized = true;
                }
            }
        }
        #endregion Reliable Dictionaries

        public HistoryDBManager(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            this.isOpenedSwitchesInitialized = false;
            this.isUnenergizedConsumersInitialized = false;

            this.stateManager = stateManager;
            //this.stateManager.StateManagerChanged += OnStateManagerChangedHandler;

            openedSwitches = new ReliableDictionaryAccess<long, long>(stateManager, ReliableDictionaryNames.OpenedSwitches);
            unenergizedConsumers = new ReliableDictionaryAccess<long, long>(stateManager, ReliableDictionaryNames.UnenergizedConsumers);
        }

        #region IHistoryDBManagerContract
        public async Task OnSwitchClosed(long elementGid)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            using (var unitOfWork = new UnitOfWork())
            {
                try
                {
                    if (await OpenedSwitches.ContainsKeyAsync(elementGid))
                    {
                        var equipment = new EquipmentHistorical()
                        {
                            EquipmentId = elementGid,
                            OperationTime = DateTime.Now,
                            DatabaseOperation = DatabaseOperation.DELETE,
                        };

                        unitOfWork.EquipmentHistoricalRepository.Add(equipment);
                        await OpenedSwitches.TryRemoveAsync(elementGid);
                        unitOfWork.Complete();
                    }
                }
                catch (Exception e)
                {
                    string message = $"{baseLogString} OnSwitchClosed => Exception: {e.Message}";
                    Logger.LogError(message, e);
                }
            }
        }

        public async Task UpdateClosedSwitch(long elementGid, long outageId)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            using (var unitOfWork = new UnitOfWork())
            {
                try
                {
                    if (!(await OpenedSwitches.ContainsKeyAsync(elementGid)))
                    {
                        var equipment = new EquipmentHistorical()
                        {
                            EquipmentId = elementGid,
                            OperationTime = DateTime.Now,
                            DatabaseOperation = DatabaseOperation.UPDATE,
                            OutageId = outageId
                        };

                        unitOfWork.EquipmentHistoricalRepository.Add(equipment);
                        //await OpenedSwitches.TryRemoveAsync(elementGid);
                        unitOfWork.Complete();
                    }
                }
                catch (Exception e)
                {
                    string message = $"{baseLogString} UpdateClosedSwitch => Exception: {e.Message}";
                    Logger.LogError(message, e);
                }
            }
        }

        //public async Task OnConsumerBlackedOut(List<long> consumers, long? outageId)
        //{
        //    while (!ReliableDictionariesInitialized)
        //    {
        //        await Task.Delay(1000);
        //    }

        //    using (var unitOfWork = new UnitOfWork())
        //    {
        //        try
        //        {
        //            var consumerHistoricals = new List<ConsumerHistorical>();

        //            foreach (var consumer in consumers)
        //            {
        //                if (!await UnenergizedConsumers.ContainsKeyAsync(consumer))
        //                {
        //                    consumerHistoricals.Add(new ConsumerHistorical() { OutageId = outageId, ConsumerId = consumer, OperationTime = DateTime.Now, DatabaseOperation = DatabaseOperation.INSERT });
        //                    await UnenergizedConsumers.SetAsync(consumer, 0);
        //                }
        //            }

        //            unitOfWork.ConsumerHistoricalRepository.AddRange(consumerHistoricals);
        //            unitOfWork.Complete();
        //        }
        //        catch (Exception e)
        //        {
        //            string message = $"{baseLogString} OnConsumersBlackedOut => Exception: {e.Message}";
        //            Logger.LogError(message, e);
        //        }
        //    }
        //}

        public async Task OnConsumerBlackedOut(long consumer, long? outageId)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            using (var unitOfWork = new UnitOfWork())
            {
                try
                {
                    var consumerHistoricals = new List<ConsumerHistorical>(1);

                    if (!await UnenergizedConsumers.ContainsKeyAsync(consumer))
                    {
                        consumerHistoricals.Add(new ConsumerHistorical() { OutageId = outageId, ConsumerId = consumer, OperationTime = DateTime.Now, DatabaseOperation = DatabaseOperation.INSERT });
                        await UnenergizedConsumers.SetAsync(consumer, 0);
                    }

                    unitOfWork.ConsumerHistoricalRepository.AddRange(consumerHistoricals);
                    unitOfWork.Complete();
                }
                catch (Exception e)
                {
                    string message = $"{baseLogString} OnConsumersBlackedOut => Exception: {e.Message}";
                    Logger.LogError(message, e);
                }
            }
        }

        public async Task UpdateConsumer(long consumer, long outageId)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            using (var unitOfWork = new UnitOfWork())
            {
                try
                {
                    var consumerHistoricals = new List<ConsumerHistorical>(1);

                    if (await UnenergizedConsumers.ContainsKeyAsync(consumer))
                    {
                        consumerHistoricals.Add(new ConsumerHistorical() { OutageId = outageId, ConsumerId = consumer, OperationTime = DateTime.Now, DatabaseOperation = DatabaseOperation.UPDATE });
                        //await UnenergizedConsumers.SetAsync(consumer, 0);
                    }

                    unitOfWork.ConsumerHistoricalRepository.AddRange(consumerHistoricals);
                    unitOfWork.Complete();
                }
                catch (Exception e)
                {
                    string message = $"{baseLogString} UpdateConsumer => Exception: {e.Message}";
                    Logger.LogError(message, e);
                }
            }
        }

        public async Task OnSwitchOpened(long elementGid, long? outageId)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            using (var unitOfWork = new UnitOfWork())
            {
                try
                {
                    if (!await OpenedSwitches.ContainsKeyAsync(elementGid))
                    {
                        var equipment = new EquipmentHistorical()
                        {
                            OutageId = outageId,
                            EquipmentId = elementGid,
                            OperationTime = DateTime.Now,
                            DatabaseOperation = DatabaseOperation.INSERT,
                        };

                        unitOfWork.EquipmentHistoricalRepository.Add(equipment);
                        await OpenedSwitches.SetAsync(elementGid, 0);
                        unitOfWork.Complete();
                    }
                }
                catch (Exception e)
                {
                    string message = $"{baseLogString} OnSwitchOpened => Exception: {e.Message}";
                    Logger.LogError(message, e);
                }
            }
        }

        public async Task OnConsumersEnergized(HashSet<long> consumers)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            using (var unitOfWork = new UnitOfWork())
            {
                try
                {
                    var consumerHistoricals = new List<ConsumerHistorical>();
                    var copy = (await UnenergizedConsumers.GetDataCopyAsync()).Keys.ToList();
                    var changedConsumers = copy.Intersect(consumers).ToList();

                    foreach (var consumer in changedConsumers)
                    {
                        var consumerHistorical = new ConsumerHistorical()
                        {
                            ConsumerId = consumer,
                            OperationTime = DateTime.Now,
                            DatabaseOperation = DatabaseOperation.DELETE,
                        };

                        consumerHistoricals.Add(consumerHistorical);
                    }

                    foreach (var changed in changedConsumers)
                    {
                        if (await UnenergizedConsumers.ContainsKeyAsync(changed))
                        {
                            await UnenergizedConsumers.TryRemoveAsync(changed);
                        }
                    }

                    unitOfWork.ConsumerHistoricalRepository.AddRange(consumerHistoricals);
                    unitOfWork.Complete();
                }
                catch (Exception e)
                {
                    string message = $"{baseLogString} OnConsumersEnergized => Exception: {e.Message}";
                    Logger.LogError(message, e);
                }
            }
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion IHistoryDBManagerContract
    }
}
