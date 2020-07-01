﻿using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.FunctionExecutior;
using System;
using System.Threading.Tasks;

namespace SCADA.FunctionExecutorImplementation.CommandEnqueuers
{
    public class WriteCommandEnqueuer : IWriteCommandEnqueuer
    {
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private readonly CloudQueue readCommandQueue;
        private readonly CloudQueue writeCommandQueue;
        private readonly CloudQueue modelUpdateCommandQueue;

        public WriteCommandEnqueuer()
        {
            CloudQueueHelper.TryGetQueue("readcommandqueue", out this.readCommandQueue);
            CloudQueueHelper.TryGetQueue("writecommandqueue", out this.writeCommandQueue);
            CloudQueueHelper.TryGetQueue("mucommandqueue", out this.modelUpdateCommandQueue);
        }

        #region Command Enqueuers
        public async Task<bool> EnqueueWriteCommand(IWriteModbusFunction modbusFunction)
        {
            bool success;

            while (modelUpdateCommandQueue.PeekMessage() != null)
            {
                await Task.Delay(1000);
            }

            try
            {
                await this.writeCommandQueue.AddMessageAsync(new CloudQueueMessage(Serialization.ObjectToByteArray(modbusFunction)));
                this.readCommandQueue.Clear();
                success = true;
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