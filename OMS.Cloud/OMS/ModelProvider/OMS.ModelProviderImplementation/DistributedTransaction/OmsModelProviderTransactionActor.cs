using Common.OMS;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.TmsContracts;
using System;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ModelProviderImplementation.DistributedTransaction
{
    public class OmsModelProviderTransactionActor : ITransactionActorContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        #region Reliable Dictionaries
        private bool isOutageTopologyModelInitialized;
        private bool isCommandedElementsInitialized;
        private bool isOptimumIsolatioPointsInitialized;
        private bool ReliableDictionariesInitialized
        {
            get
            {
                return isOutageTopologyModelInitialized &&
                       isCommandedElementsInitialized &&
                       isOptimumIsolatioPointsInitialized;
            }
        }

        private ReliableDictionaryAccess<long, OutageTopologyModel> OutageTopologyModel { get; set; }
        private ReliableDictionaryAccess<long, long> CommandedElements { get; set; }
        private ReliableDictionaryAccess<long, long> OptimumIsolatioPoints { get; set; }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.OutageTopologyModel)
                {
                    OutageTopologyModel = await ReliableDictionaryAccess<long, OutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.OutageTopologyModel);
                    this.isOutageTopologyModelInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.OutageTopologyModel}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.CommandedElements)
                {
                    CommandedElements = await ReliableDictionaryAccess<long, long>.Create(stateManager, ReliableDictionaryNames.CommandedElements);
                    this.isCommandedElementsInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.CommandedElements}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.OptimumIsolatioPoints)
                {
                    OptimumIsolatioPoints = await ReliableDictionaryAccess<long, long>.Create(stateManager, ReliableDictionaryNames.OptimumIsolatioPoints);
                    this.isOptimumIsolatioPointsInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.OptimumIsolatioPoints}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        public OmsModelProviderTransactionActor(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isOutageTopologyModelInitialized = false;
            this.isCommandedElementsInitialized = false;
            this.isOptimumIsolatioPointsInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region ITransactionActorContract
        public async Task<bool> Prepare()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            return true;
        }
        
        public async Task Commit()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                await OutageTopologyModel.ClearAsync();
                await CommandedElements.ClearAsync();
                await OptimumIsolatioPoints.ClearAsync();

                string message = $"{baseLogString} Commit => {MicroserviceNames.OmsModelProviderService} confirmed model changes.";
                Logger.LogInformation(message);

                await LogAllReliableCollections();
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Commit => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
            }

        }

        public async Task Rollback()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                string message = $"{baseLogString} Rollback => {MicroserviceNames.OmsModelProviderService} rejected model changes.";
                Logger.LogInformation(message);

                await LogAllReliableCollections();
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Rollback => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
            }
        }
        
        public Task<bool> IsAlive()
        {
            return Task.Run(() => true);
        }
        #endregion ITransactionActorContract

        private async Task LogAllReliableCollections()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Reliable Collections");

            sb.AppendLine("OutageTopologyModel =>");
            var outageTopologyModel = await OutageTopologyModel.GetEnumerableDictionaryAsync();
            foreach (var element in outageTopologyModel)
            {
                sb.AppendLine($"Key => {element.Key}, Value => FirstNode: 0x{element.Value.FirstNode:X16}, Next Nodes Count: {element.Value.OutageTopology.Count} ....");
            }
            sb.AppendLine();

            sb.AppendLine("CommandedElements =>");
            var commandedElements = await CommandedElements.GetEnumerableDictionaryAsync();
            foreach (var element in commandedElements)
            {
                sb.AppendLine($"Key => 0x{element.Key:X16}, Value => 0x{element.Value:X16}");
            }
            sb.AppendLine();

            sb.AppendLine("OptimumIsolatioPoints =>");
            var optimumIsolatioPoints = await OptimumIsolatioPoints.GetEnumerableDictionaryAsync();
            foreach (var element in optimumIsolatioPoints)
            {
                sb.AppendLine($"Key => 0x{element.Key:X16}, Value => 0x{element.Value:X16}");
            }
            sb.AppendLine();

            Logger.LogDebug($"{baseLogString} LogAllReliableCollections => {sb}");
        }
    }
}
