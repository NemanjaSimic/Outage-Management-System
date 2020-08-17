using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud.AzureStorageHelpers;
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

        public ReadCommandEnqueuer()
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
        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #region IReadCommandEnqueuer
        public async Task<bool> EnqueueReadCommand(IReadModbusFunction modbusFunction)
        {
            string verboseMessage = $"{baseLogString} entering EnqueueReadCommand, FunctionCode: {modbusFunction.FunctionCode}, StartAddress: {modbusFunction.StartAddress}, Quantity: {modbusFunction.Quantity}.";
            Logger.LogVerbose(verboseMessage);

            bool success;

            if (!(modbusFunction is IReadModbusFunction readModbusFunction))
            {
                string message = $"{baseLogString} EnqueueReadCommand => trying to enqueue modbus function that does not implement IReadModbusFunction interface.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            var modelUpdatePeakResult = modelUpdateCommandQueue.PeekMessage() != null;
            var writePeakResult = writeCommandQueue.PeekMessage() != null;

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

            try
            {
                await this.readCommandQueue.AddMessageAsync(new CloudQueueMessage(Serialization.ObjectToByteArray(modbusFunction)));
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
        #endregion
    }
}
