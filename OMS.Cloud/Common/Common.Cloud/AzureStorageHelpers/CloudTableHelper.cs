using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using OMS.Common.Cloud.Logger;
using System;
using System.Configuration;

namespace OMS.Common.Cloud.AzureStorageHelpers
{
    public static class CloudTableHelper
    {
        private static ICloudLogger logger;
        private static ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public static bool TryGetTable(string queueName, out CloudTable table)
        {
            bool success;
            table = null;

            try
            {
                if (ConfigurationManager.AppSettings["StorageConnectionString"] is string storageConnectionString)
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                    table = tableClient.GetTableReference(queueName);
                    table.CreateIfNotExists();
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception e)
            {
                string message = "Exception caught in TryGetTable.";
                Logger.LogError(message, e);
                success = false;
            }

            return success;
        }
    }
}
