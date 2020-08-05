using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.FunctionExecutorImplementation.CommandEnqueuers
{
    public class WriteCommandEnqueuer : IWriteCommandEnqueuerContract
    {
        private readonly string baseLogString;
        private readonly CloudQueue readCommandQueue;
        private readonly CloudQueue writeCommandQueue;
        private readonly CloudQueue modelUpdateCommandQueue;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public WriteCommandEnqueuer()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.ReadCommandQueue, out this.readCommandQueue);
            CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.WriteCommandQueue, out this.writeCommandQueue);
            CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.ModelUpdateCommandQueue, out this.modelUpdateCommandQueue);

            string debugMessage = $"{baseLogString} Ctor => CloudQueues initialized.";
            Logger.LogDebug(debugMessage);
        }

        #region Command Enqueuers
        public async Task<bool> EnqueueWriteCommand(IWriteModbusFunction modbusFunction)
        {
            string verboseMessage = $"{baseLogString} entering EnqueueWriteCommand, FunctionCode: {modbusFunction.FunctionCode}, CommandOrigin: {modbusFunction.CommandOrigin}.";
            Logger.LogVerbose(verboseMessage);

            bool success;

            if (!(modbusFunction is IWriteModbusFunction writeModbusFunction))
            {
                string message = $"{baseLogString} EnqueueWriteCommand => trying to enqueue modbus function that does not implement IWriteModbusFunction interface.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if (modelUpdateCommandQueue.PeekMessage() != null)
            {
                verboseMessage = $"{baseLogString} EnqueueWriteCommand => '{CloudStorageQueueNames.ModelUpdateCommandQueue}' queue is not empty.";
                Logger.LogDebug(verboseMessage);

                return false;
            }

            try
            {
                //KEY LOGIC
                if (modbusFunction.CommandOrigin == CommandOriginType.MODEL_UPDATE_COMMAND)
                {
                    await this.modelUpdateCommandQueue.AddMessageAsync(new CloudQueueMessage(Serialization.ObjectToByteArray(modbusFunction)));
                }
                else
                {
                    await this.writeCommandQueue.AddMessageAsync(new CloudQueueMessage(Serialization.ObjectToByteArray(modbusFunction)));
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
                
                this.readCommandQueue.Clear();
                
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
        #endregion
    }
}
