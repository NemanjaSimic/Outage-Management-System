using Common.OMS;
using Common.OmsContracts.DataContracts.HistoryModel;
using Common.OmsContracts.HistoryDBManager;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryDBManagerServiceImplementation
{
    public class HistoryDBManager:IHistoryDBManagerContract
    {
        private readonly IReliableStateManager stateManager;

        private ReliableDictionaryAccess<long, long> unenergizedConsumers;
        public ReliableDictionaryAccess<long, long> UnenergizedConsumers
        {
            get
            {
                if(unenergizedConsumers == null)
                {
                    unenergizedConsumers = ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.UnenergizedConsumers).Result;
                }
                return unenergizedConsumers;
            }
            protected set
            {
                unenergizedConsumers = value;
            }
        }

        private ReliableDictionaryAccess<long, long> openedSwitches;
        public ReliableDictionaryAccess<long,long> OpenedSwitches
        {
            get
            {
                if(openedSwitches == null)
                {
                    openedSwitches = ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.OpenedSwitches).Result;
                }
                return openedSwitches;
            }
            protected set
            {
                openedSwitches = value;
            }
        }
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public HistoryDBManager(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += OnStateManagerChangedHandler;
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if(e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;
                if (reliableStateName == ReliableDictionaryNames.OpenedSwitches)
                {
                    OpenedSwitches = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.OpenedSwitches);
                } else if (reliableStateName == ReliableDictionaryNames.UnenergizedConsumers)
                {
                    UnenergizedConsumers = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.UnenergizedConsumers);
                }
            }
        }

        public async Task OnSwitchClosed(long elementGid)
        {
            try
            {
                using(var tx = this.stateManager.CreateTransaction())
                {
                    if(OpenedSwitches.ContainsKey(elementGid))
                    {
                        //TO DO add to db
                        await OpenedSwitches.TryRemoveAsync(tx, elementGid);
                        //SaveChanges
                        await tx.CommitAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                string message = "HistoryDBManager::OnSwitchClosed method => exception on Complete()";
                Logger.LogError(message, ex);
                Console.WriteLine($"{message}, Message: {ex.Message}, Inner Message: {ex.InnerException.Message})");
            }
        }

        public async Task OnConsumerBlackedOut(List<long> consumers, long? outageId)
        {
            List<ConsumerHistorical> consumerHistoricals = new List<ConsumerHistorical>();
            try
            {
                using (var tx = this.stateManager.CreateTransaction())
                {
                    foreach (var consumer in consumers)
                    {
                        if (!UnenergizedConsumers.ContainsKey(consumer))
                        {

                            consumerHistoricals.Add(new ConsumerHistorical() { OutageId = outageId, ConsumerId = consumer, OperationTime = DateTime.Now, DatabaseOperation = DatabaseOperation.INSERT });
                            await UnenergizedConsumers.AddAsync(tx,consumer,0);
                        }
                    }
                    //TO DO add to db
                    //SaveChanges
                    await tx.CommitAsync();
                }
            }
            catch (Exception e)
            {
                string message = "HistoryDBManager::OnConsumersBlackedOut method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message}, Inner Message: {e.InnerException.Message})");
            }
        }

        public async Task OnSwitchOpened(long elementGid, long? outageId)
        {
            try
            {
                using (var tx = this.stateManager.CreateTransaction())
                {
                    if (!OpenedSwitches.ContainsKey(elementGid))
                    {
                        //TO DO add to db
                        await OpenedSwitches.AddAsync(tx,elementGid,0);
                        //SaveChanges
                        await tx.CommitAsync();
                    }
                }
            }
            catch (Exception e)
            {
                string message = "HistoryDBManager::OnSwitchOpened method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message}, Inner Message: {e.InnerException.Message})");
            }
        }

        public async Task OnConsumersEnergized(HashSet<long> consumers)
        {
            List<ConsumerHistorical> consumerHistoricals = new List<ConsumerHistorical>();
            var copy = UnenergizedConsumers.GetDataCopy().Keys.ToList();
            var changedConsumers = copy.Intersect(consumers).ToList();

            foreach (var consumer in changedConsumers)
            {
                consumerHistoricals.Add(new ConsumerHistorical() { ConsumerId = consumer, OperationTime = DateTime.Now, DatabaseOperation = DatabaseOperation.DELETE });
            }

            try
            {
                using (var tx = this.stateManager.CreateTransaction())
                {
                    foreach (var changed in changedConsumers)
                    {
                        if (UnenergizedConsumers.ContainsKey(changed))
                            await UnenergizedConsumers.TryRemoveAsync(tx,changed);
                    }

                    //Add to db
                    //savechanges
                    await tx.CommitAsync();
                }
            }
            catch (Exception e)
            {
                string message = "HistoryDBManager::OnConsumersEnergized method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message}, Inner Message: {e.InnerException.Message})");
            }
        }
    
    }
}
