using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using Outage.Common;
using System;
using System.Threading.Tasks;

namespace SCADA.FunctionExecutorImplementation.CommandEnqueuers
{
    public class ReadCommandEnqueuer : IReadCommandEnqueuer
    {
        private ILogger logger;
        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private readonly CloudQueue readCommandQueue;
        private readonly CloudQueue writeCommandQueue;
        private readonly CloudQueue modelUpdateCommandQueue;

        public ReadCommandEnqueuer()
        {
            CloudQueueHelper.TryGetQueue("readcommandqueue", out this.readCommandQueue);
            CloudQueueHelper.TryGetQueue("writecommandqueue", out this.writeCommandQueue);
            CloudQueueHelper.TryGetQueue("mucommandqueue", out this.modelUpdateCommandQueue);
        }

        #region IReadCommandEnqueuer
        public async Task<bool> EnqueueReadCommand(IReadModbusFunction modbusFunction)
        {
            bool success;

            if (!(modbusFunction is IReadAnalogModusFunction || modbusFunction is IReadDiscreteModbusFunction))
            {
                string message = "EnqueueReadCommand => trying to enqueue modbus function that implements neither IReadDiscreteModbusFunction nor IReadDiscreteModbusFunction interface.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            while (modelUpdateCommandQueue.PeekMessage() != null || writeCommandQueue.PeekMessage() != null)
            {
                while (modelUpdateCommandQueue.PeekMessage() != null)
                {
                    await Task.Delay(1000);
                }

                while (writeCommandQueue.PeekMessage() == null)
                {
                    await Task.Delay(1000);
                }
            }

            try
            {
                await this.readCommandQueue.AddMessageAsync(new CloudQueueMessage(Serialization.ObjectToByteArray(modbusFunction)));
                //this.commandEvent.Set();
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
