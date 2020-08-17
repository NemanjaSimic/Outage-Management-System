using Common.CeContracts.ModelProvider;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.TmsContracts;
using OMS.Common.WcfClient.CE;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CE.ModelProviderImplementation
{
    public class CeTransactionActor : ITransactionActorContract
    {
        private readonly string baseLogString;
        private readonly IModelProviderContract modelProviderClient;
        //private readonly IReliableStateManager stateManager;

        #region Private Properties
        private ICloudLogger logger;

        public ReliableDictionaryAccess<byte, List<long>> ModelChanges { get; private set; }

        private bool isModelChangesInitialized;

        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
		#endregion

		public CeTransactionActor()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            modelProviderClient = ModelProviderClient.CreateClient();

            Logger.LogDebug($"{baseLogString} ctor initialized.");
        }

        //private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        //{
        //    if (e.Action == NotifyStateManagerChangedAction.Add)
        //    {
        //        var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
        //        string reliableStateName = operation.ReliableState.Name.AbsolutePath;

        //        if (reliableStateName == ReliableDictionaryNames.ModelChanges)
        //        {
        //            ModelChanges = await ReliableDictionaryAccess<byte, List<long>>.Create(stateManager, ReliableDictionaryNames.ModelChanges);
        //            this.isModelChangesInitialized = true;

        //            string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.ModelChanges}' ReliableDictionaryAccess initialized.";
        //            Logger.LogDebug(debugMessage);
        //        }
        //    }
        //}
        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }

        #region ITransactionActorContract
        public async Task<bool> Prepare()
        {
            logger.LogDebug($"{baseLogString} Prepare entered.");
            bool success;

            //while (!isModelChangesInitialized)
            //{
            //    await Task.Delay(1000);
            //}

            try
            {
                success = await modelProviderClient.Prepare();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{baseLogString} Prepare => Exception: {ex.Message}", ex);
                success = false;
            }

            if (success)
            {
                Logger.LogInformation($"{baseLogString} Prepare => Preparation on CE Transaction actor SUCCESSFULLY finished.");
            }
            else
            {
                Logger.LogInformation($"{baseLogString} Prepare => Preparation on CE Transaction actor UNSUCCESSFULLY finished.");
            }

            return success;
        }

        public async Task Commit()
        {
            logger.LogDebug($"{baseLogString} Commit entered.");

            //while (!isModelChangesInitialized)
            //{
            //    await Task.Delay(1000);
            //}

            try
            {
                await modelProviderClient.Commit();
                Logger.LogInformation($"{baseLogString} Commit => Commit on CE Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"{baseLogString} Commit => " +
                    $"{Environment.NewLine} Exception: {ex.Message} " +
                    $"{Environment.NewLine} Stack trace: {ex.StackTrace}", ex);
                Logger.LogInformation($"{baseLogString} Commit => Commit on CE Transaction actor UNSUCCESSFULLY finished.");
            }
        }

        public async Task Rollback()
        {
            logger.LogDebug($"{baseLogString} Rollback entered.");

            //while (!isModelChangesInitialized)
            //{
            //    await Task.Delay(1000);
            //}

            try
            {
                await modelProviderClient.Rollback();
                Logger.LogInformation($"{baseLogString} Rollback => Rollback on CE Transaction actor SUCCESSFULLY finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"{baseLogString} Rollback => Exception: {ex.Message}", ex);
                Logger.LogInformation($"{baseLogString} Rollback => Rollback on CE Transaction actor UNSUCCESSFULLY finished.");
            }
        }
        #endregion
    }
}
