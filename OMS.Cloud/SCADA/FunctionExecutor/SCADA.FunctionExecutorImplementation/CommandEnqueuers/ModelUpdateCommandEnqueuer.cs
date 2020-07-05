using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCADA.FunctionExecutorImplementation.CommandEnqueuers
{
    public class ModelUpdateCommandEnqueuer : IModelUpdateCommandEnqueuer
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

        public ModelUpdateCommandEnqueuer()
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

        #region IModelUpdateCommandEnqueuer
        public async Task<bool> EnqueueModelUpdateCommands(List<IWriteModbusFunction> modbusFunctions)
        {
            string verboseMessage = $"{baseLogString} entering EnqueueModelUpdateCommands, modbus functions count: {modbusFunctions.Count}.";
            Logger.LogVerbose(verboseMessage);

            bool success;

            try
            {
                Task[] addTasks = new Task[modbusFunctions.Count];

                for (int i = 0; i < modbusFunctions.Count; i++)
                {
                    addTasks[i] = this.modelUpdateCommandQueue.AddMessageAsync(new CloudQueueMessage(Serialization.ObjectToByteArray(modbusFunctions[i])));
                }

                Task.WaitAll(addTasks);
                success = true;

                string informationMessage = $"{baseLogString} EnqueueModelUpdateCommands => {modbusFunctions.Count} commands SUCCESSFULLY enqueued to '{CloudStorageQueueNames.ModelUpdateCommandQueue}' queue.";
                Logger.LogInformation(informationMessage);

                this.writeCommandQueue.Clear();
                this.readCommandQueue.Clear();

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
        #endregion
    }
}
