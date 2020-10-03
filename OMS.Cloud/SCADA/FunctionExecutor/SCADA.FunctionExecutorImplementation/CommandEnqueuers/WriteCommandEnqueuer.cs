using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts.ModbusFunctions;
using OMS.Common.ScadaContracts.FunctionExecutior;
using System;
using System.Fabric;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.FunctionExecutorImplementation.CommandEnqueuers
{
    public class WriteCommandEnqueuer : IWriteCommandEnqueuerContract
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
                return true;
            }
        }

        private ReliableQueueAccess<ModbusFunction> readCommandQueue;
        private ReliableQueueAccess<ModbusFunction> ReadCommandQueue
        {
            get { return readCommandQueue; }
        }

        private ReliableQueueAccess<ModbusFunction> writeCommandQueue;
        private ReliableQueueAccess<ModbusFunction> WriteCommandQueue
        {
            get { return writeCommandQueue; }
        }

        private ReliableQueueAccess<ModbusFunction> modelUpdateCommandQueue;
        private ReliableQueueAccess<ModbusFunction> ModelUpdateCommandQueue
        {
            get { return modelUpdateCommandQueue; }
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

                if (reliableStateName == ReliableQueueNames.ReadCommandQueue)
                {
                    this.readCommandQueue = await ReliableQueueAccess<ModbusFunction>.Create(stateManager, ReliableQueueNames.ReadCommandQueue);
                    this.isReadCommandQueueInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableQueueNames.ReadCommandQueue}' ReliableQueueAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableQueueNames.WriteCommandQueue)
                {
                    this.writeCommandQueue = await ReliableQueueAccess<ModbusFunction>.Create(stateManager, ReliableQueueNames.WriteCommandQueue);
                    this.isWriteCommandQueueInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableQueueNames.WriteCommandQueue}' ReliableQueueAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableQueueNames.ModelUpdateCommandQueue)
                {
                    this.modelUpdateCommandQueue = await ReliableQueueAccess<ModbusFunction>.Create(stateManager, ReliableQueueNames.ModelUpdateCommandQueue);
                    this.isModelUpdateCommandQueueInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableQueueNames.ModelUpdateCommandQueue}' ReliableQueueAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion

        public WriteCommandEnqueuer(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            this.isReadCommandQueueInitialized = false;
            this.isWriteCommandQueueInitialized = false;
            this.isModelUpdateCommandQueueInitialized = false;

            this.stateManager = stateManager;
            this.readCommandQueue = new ReliableQueueAccess<ModbusFunction>(stateManager, ReliableQueueNames.ReadCommandQueue);
            this.writeCommandQueue = new ReliableQueueAccess<ModbusFunction>(stateManager, ReliableQueueNames.WriteCommandQueue);
            this.modelUpdateCommandQueue = new ReliableQueueAccess<ModbusFunction>(stateManager, ReliableQueueNames.ModelUpdateCommandQueue);
        }

        #region Command Enqueuers
        public async Task<bool> EnqueueWriteCommand(IWriteModbusFunction modbusFunction)
        {
            string verboseMessage = $"{baseLogString} entering EnqueueWriteCommand, FunctionCode: {modbusFunction.FunctionCode}, CommandOrigin: {modbusFunction.CommandOrigin}.";
            Logger.LogVerbose(verboseMessage);

            bool success;

            while (!ReliableQueuesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            { 
                if (!(modbusFunction is IWriteModbusFunction writeModbusFunction))
                {
                    string message = $"{baseLogString} EnqueueWriteCommand => trying to enqueue modbus function that does not implement IWriteModbusFunction interface.";
                    Logger.LogError(message);
                    throw new ArgumentException(message);
                }

                if ((await modelUpdateCommandQueue.GetCountAsync()) > 0)
                {
                    verboseMessage = $"{baseLogString} EnqueueWriteCommand => '{CloudStorageQueueNames.ModelUpdateCommandQueue}' queue is not empty.";
                    Logger.LogDebug(verboseMessage);

                    return false;
                }

                //KEY LOGIC
                if (modbusFunction.CommandOrigin == CommandOriginType.MODEL_UPDATE_COMMAND)
                {
                    await this.modelUpdateCommandQueue.EnqueueAsync((ModbusFunction)modbusFunction);
                }
                else
                {
                    await this.writeCommandQueue.EnqueueAsync((ModbusFunction)modbusFunction);
                }

                success = true;
                
                if(writeModbusFunction is IWriteSingleFunction writeSingleFunction)
                {
                    string informationMessage = $"{baseLogString} EnqueueWriteCommand => write command SUCCESSFULLY enqueued to '{CloudStorageQueueNames.WriteCommandQueue}' queue. FunctionCode: {writeSingleFunction.FunctionCode}, OutputAddress: {writeSingleFunction.OutputAddress}, CommandValue: {writeSingleFunction.CommandValue}, CommandOrigin: {writeSingleFunction.CommandOrigin},";
                    Logger.LogInformation(informationMessage);
                }
                else if(writeModbusFunction is IWriteMultipleFunction writeMultipleFunction)
                {
                    StringBuilder commandValuesSB = new StringBuilder();
                    commandValuesSB.Append("[ ");
                    foreach (int value in writeMultipleFunction.CommandValues)
                    {
                        commandValuesSB.Append(value);
                        commandValuesSB.Append(" ");
                    }
                    commandValuesSB.Append("]");

                    string informationMessage = $"{baseLogString} EnqueueWriteCommand => write command SUCCESSFULLY enqueued to '{CloudStorageQueueNames.WriteCommandQueue}' queue. FunctionCode: {writeMultipleFunction.FunctionCode}, StartAddress: {writeMultipleFunction.StartAddress}, CommandValue: {commandValuesSB}, CommandOrigin: {writeMultipleFunction.CommandOrigin},";
                    Logger.LogInformation(informationMessage);
                }
                
                await this.readCommandQueue.ClearAsync();
                
                string debugMessage = $"{baseLogString} EnqueueModelUpdateCommands => cloud storage queues that were cleared: '{CloudStorageQueueNames.ReadCommandQueue}'";
                Logger.LogDebug(debugMessage);
            }
            catch (Exception e)
            {
                success = false;
                string message = "Exception caught in EnqueueCommand() method.";
                Logger.LogError(message, e);
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
