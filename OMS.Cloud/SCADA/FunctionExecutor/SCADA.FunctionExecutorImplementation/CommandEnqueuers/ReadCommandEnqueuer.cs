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
    public class ReadCommandEnqueuer : IReadCommandEnqueuer
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
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>";

            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.ReadCommandQueue, out this.readCommandQueue);
            CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.WriteCommandQueue, out this.writeCommandQueue);
            CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.ModelUpdateCommandQueue, out this.modelUpdateCommandQueue);

            string debugMessage = $"{baseLogString} Ctor => CloudQueues initialized.";
            Logger.LogDebug(debugMessage);
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

            //TODO: while treba da predje u IF i da se read commanda ne upise u queue ako je uslov zadovoljen -> tek ce izvrsena w/mu komanda uzrokovati promenu vrednosti, te nema potrebe upisivati r komandu pre toga
            while (modelUpdateCommandQueue.PeekMessage() != null || writeCommandQueue.PeekMessage() != null)
            {
                while (modelUpdateCommandQueue.PeekMessage() != null)
                {
                    verboseMessage = $"{baseLogString} EnqueueReadCommand => waiting for '{CloudStorageQueueNames.ModelUpdateCommandQueue}' queue to be empty.";
                    Logger.LogVerbose(verboseMessage);
                    await Task.Delay(1000);
                }

                while (writeCommandQueue.PeekMessage() == null)
                {
                    verboseMessage = $"{baseLogString} EnqueueReadCommand => waiting for '{CloudStorageQueueNames.WriteCommandQueue}' queue to be empty.";
                    Logger.LogVerbose(verboseMessage);
                    await Task.Delay(1000);
                }
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
