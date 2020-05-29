using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCADA.FunctionExecutorImplementation.CommandEnqueuers
{
    public class ModelUpdateCommandEnqueuer : IModelUpdateCommandEnqueuer
    {
        private ILogger logger;
        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private readonly CloudQueue readCommandQueue;
        private readonly CloudQueue writeCommandQueue;
        private readonly CloudQueue modelUpdateCommandQueue;

        public ModelUpdateCommandEnqueuer()
        {
            CloudQueueHelper.TryGetQueue("readcommandqueue", out this.readCommandQueue);
            CloudQueueHelper.TryGetQueue("writecommandqueue", out this.writeCommandQueue);
            CloudQueueHelper.TryGetQueue("mucommandqueue", out this.modelUpdateCommandQueue);
        }

        #region IModelUpdateCommandEnqueuer
        public async Task<bool> EnqueueModelUpdateCommands(List<IWriteModbusFunction> modbusFunctions)
        {
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
                this.writeCommandQueue.Clear();
                this.readCommandQueue.Clear();
            }
            catch (Exception e)
            {
                success = false;
                string message = "Exception caught in EnqueueModelUpdateCommands() method.";
                Logger.LogError(message, e);
            }

            return success;
        }
        #endregion
    }
}
