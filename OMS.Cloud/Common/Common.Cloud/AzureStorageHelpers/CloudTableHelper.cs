using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Outage.Common;
using System;
using System.Configuration;

namespace OMS.Common.Cloud.AzureStorageHelpers
{
    public static class CloudTableHelper
    {
        public static bool TryGetTable(string queueName, out CloudTable table)
        {
            ILogger logger = LoggerWrapper.Instance;

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
                logger.LogError(message, e);
                success = false;
            }

            return success;
        }
    }
}
