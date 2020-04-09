using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Cloud.SCADA.FunctionExecutorService.CommandEnqueuers
{
    internal class ModelUpdateCommandEnqueuer : IModelUpdateCommandEnqueuer
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

            Dictionary<long, AnalogModbusData> analogData = new Dictionary<long, AnalogModbusData>();
            Dictionary<long, DiscreteModbusData> discreteData = new Dictionary<long, DiscreteModbusData>();
            //MeasurementsCache.Clear();

            try
            {
                //Dictionary<long, ISCADAModelPointItem> currentScadaModel = new Dictionary<long, ISCADAModelPointItem>(); //TODO: Preuzeti od providera

                Task[] addTasks = new Task[modbusFunctions.Count];
                for (int i = 0; i < modbusFunctions.Count; i++)
                {
                    addTasks[i] = this.modelUpdateCommandQueue.AddMessageAsync(new CloudQueueMessage(Serialization.ObjectToByteArray(modbusFunctions[i])));
                }

                Task.WaitAll(addTasks);

                success = true;
                this.writeCommandQueue.Clear();
                this.readCommandQueue.Clear();
                //this.commandEvent.Set();
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
