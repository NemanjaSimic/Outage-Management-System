using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using System;
using System.Threading.Tasks;

namespace SCADA.FunctionExecutorImplementation.CommandEnqueuers
{
    public class ReadCommandEnqueuer : IReadCommandEnqueuerContract
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
                else if (reliableStateName == ReliableQueueNames.WriteCommandQueue)
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

        public ReadCommandEnqueuer(IReliableStateManager stateManager)
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

        #region IReadCommandEnqueuer
        public async Task<bool> EnqueueReadCommand(IReadModbusFunction modbusFunction)
        {
            string verboseMessage = $"{baseLogString} entering EnqueueReadCommand, FunctionCode: {modbusFunction.FunctionCode}, StartAddress: {modbusFunction.StartAddress}, Quantity: {modbusFunction.Quantity}.";
            Logger.LogVerbose(verboseMessage);

            bool success;

            while (!ReliableQueuesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                if (!(modbusFunction is IReadModbusFunction readModbusFunction))
                {
                    string message = $"{baseLogString} EnqueueReadCommand => trying to enqueue modbus function that does not implement IReadModbusFunction interface.";
                    Logger.LogError(message);
                    throw new ArgumentException(message);
                }

                var modelUpdatePeakResult = (await modelUpdateCommandQueue.GetCountAsync()) > 0;
                var writePeakResult = (await writeCommandQueue.GetCountAsync()) > 0;

                if (modelUpdatePeakResult || writePeakResult)
                {
                    if (modelUpdatePeakResult)
                    {
                        verboseMessage = $"{baseLogString} EnqueueReadCommand => '{CloudStorageQueueNames.ModelUpdateCommandQueue}' queue to is not empty. ";
                        Logger.LogDebug(verboseMessage);
                    }

                    if (writePeakResult)
                    {
                        verboseMessage = $"{baseLogString} EnqueueReadCommand => '{CloudStorageQueueNames.WriteCommandQueue}' queue to is not empty.";
                        Logger.LogDebug(verboseMessage);
                    }

                    return false;
                }

                await this.readCommandQueue.EnqueueAsync(modbusFunction);
                success = true;

                verboseMessage = $"{baseLogString} EnqueueReadCommand => read command SUCCESSFULLY enqueued to '{CloudStorageQueueNames.ReadCommandQueue}' queue. FunctionCode: {readModbusFunction.FunctionCode}, StartAddress: {readModbusFunction.StartAddress}, Quantity: {readModbusFunction.Quantity}";
                Logger.LogVerbose(verboseMessage);
            }
            catch (Exception e)
            {
                success = false;
                string errorMessage = $"{baseLogString} EnqueueReadCommand => Exception caught: {e.Message}.";
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
