using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud.Logger;
using System;
using System.Configuration;

namespace OMS.Common.Cloud.AzureStorageHelpers
{
    public static class CloudQueueHelper
    {
        private static ICloudLogger logger;
        private static ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public static bool TryGetQueue(string queueName, out CloudQueue queue)
        {
            bool success;
            queue = null;

            try
            {
                if (ConfigurationManager.AppSettings["StorageConnectionString"] is string storageConnectionString)
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                    queue = queueClient.GetQueueReference(queueName);
                    queue.CreateIfNotExists();
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception e)
            {
                string message = "Exception caught in TryGetQueue.";
                Logger.LogError(message, e);
                success = false;
            }

            return success;
        }
    }
}
