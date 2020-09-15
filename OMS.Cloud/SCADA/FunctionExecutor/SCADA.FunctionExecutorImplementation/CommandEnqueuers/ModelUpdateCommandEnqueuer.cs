using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCADA.FunctionExecutorImplementation.CommandEnqueuers
{
    public class ModelUpdateCommandEnqueuer : IModelUpdateCommandEnqueuerContract
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

        #region ReliableQueues
        private bool isReadCommandQueueInitialized;
        private bool isWriteCommandQueueInitialized;
        private bool isModelUpdateCommandQueueInitialized;

        private bool ReliableQueuesInitialized
        {
            get
            {
                return isReadCommandQueueInitialized &&
                       isWriteCommandQueueInitialized &&
                       isModelUpdateCommandQueueInitialized;
            }
        }

        private ReliableQueueAccess<IReadModbusFunction> readCommandQueue;
        private ReliableQueueAccess<IReadModbusFunction> ReadCommandQueue
        {
            get { return readCommandQueue; }
        }

        private ReliableQueueAccess<IWriteModbusFunction> writeCommandQueue;
        private ReliableQueueAccess<IWriteModbusFunction> WriteCommandQueue
        {
            get { return writeCommandQueue; }
        }

        private ReliableQueueAccess<IWriteModbusFunction> modelUpdateCommandQueue;
        private ReliableQueueAccess<IWriteModbusFunction> ModelUpdateCommandQueue
        {
            get { return modelUpdateCommandQueue; }
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableQueueNames.ReadCommandQueue)
                {
                    this.readCommandQueue = await ReliableQueueAccess<IReadModbusFunction>.Create(stateManager, ReliableQueueNames.ReadCommandQueue);
                    this.isReadCommandQueueInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableQueueNames.ReadCommandQueue}' ReliableQueueAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if(reliableStateName == ReliableQueueNames.WriteCommandQueue)
                {
                    this.writeCommandQueue = await ReliableQueueAccess<IWriteModbusFunction>.Create(stateManager, ReliableQueueNames.WriteCommandQueue);
                    this.isWriteCommandQueueInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableQueueNames.WriteCommandQueue}' ReliableQueueAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableQueueNames.ModelUpdateCommandQueue)
                {
                    this.modelUpdateCommandQueue = await ReliableQueueAccess<IWriteModbusFunction>.Create(stateManager, ReliableQueueNames.ModelUpdateCommandQueue);
                    this.isModelUpdateCommandQueueInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableQueueNames.ModelUpdateCommandQueue}' ReliableQueueAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion

        public ModelUpdateCommandEnqueuer(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            this.isReadCommandQueueInitialized = false;
            this.isWriteCommandQueueInitialized = false;
            this.isModelUpdateCommandQueueInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region IModelUpdateCommandEnqueuer
        public async Task<bool> EnqueueModelUpdateCommands(List<IWriteModbusFunction> modbusFunctions)
        {
            string verboseMessage = $"{baseLogString} entering EnqueueModelUpdateCommands, modbus functions count: {modbusFunctions.Count}.";
            Logger.LogVerbose(verboseMessage);

            bool success;

            while(!ReliableQueuesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                var addTasks = new List<Task>();

                for (int i = 0; i < modbusFunctions.Count; i++)
                {
                    await this.modelUpdateCommandQueue.EnqueueAsync(modbusFunctions[i]);
                }

                success = true;

                string informationMessage = $"{baseLogString} EnqueueModelUpdateCommands => {modbusFunctions.Count} commands SUCCESSFULLY enqueued to '{CloudStorageQueueNames.ModelUpdateCommandQueue}' queue.";
                Logger.LogInformation(informationMessage);

                await this.writeCommandQueue.ClearAsync();
                await this.readCommandQueue.ClearAsync();

                string debugMessage = $"{baseLogString} EnqueueModelUpdateCommands => cloud storage queues that were cleared: '{CloudStorageQueueNames.WriteCommandQueue}', '{CloudStorageQueueNames.ReadCommandQueue}'";
                Logger.LogDebug(debugMessage);
            }
            catch (Exception e)
            {
                success = false;
                string errorMessage = $"{baseLogString} EnqueueModelUpdateCommands => Exception caught: {e.Message}.";
                Logger.LogError(errorMessage, e);
            }

            return success;
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion
    }
}
