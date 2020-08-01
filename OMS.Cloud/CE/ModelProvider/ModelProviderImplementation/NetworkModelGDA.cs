using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.WcfClient.NMS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModelProviderImplementation
{
	public class NetworkModelGDA
	{
		private readonly NetworkModelGdaClient networkModelGdaClient;

		private readonly string baseLogString;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public NetworkModelGDA()
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);

			networkModelGdaClient = NetworkModelGdaClient.CreateClient();

			string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
			Logger.LogDebug(debugMessage);
		}

		public async Task<List<ResourceDescription>> GetExtentValuesAsync(ModelCode entityType, List<ModelCode> propIds)
		{
			string verboseMessage = $"{baseLogString} entering GetExtentValuesAsync method.";
			Logger.LogVerbose(verboseMessage);

			int iteratorId;

			try
			{
				iteratorId = await networkModelGdaClient.GetExtentValues(entityType, propIds);
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} GetExtentValuesAsync => Failed to get extent values for dms type {entityType}." +
					$"{Environment.NewLine} Exception message: {e.Message}" +
					$"{Environment.NewLine} Stack trace: {e.StackTrace}";
				Logger.LogError(message);
				throw new Exception(message);
			}

			return await ProcessIteratorAsync(iteratorId);
		}
		public async Task<List<ResourceDescription>> GetRelatedValuesAsync(long source, List<ModelCode> propIds, Association association)
		{
			string verboseMessage = $"{baseLogString} entering GetRelatedValuesAsync method.";
			Logger.LogVerbose(verboseMessage);

			int iteratorId;

			try
			{
				iteratorId = await networkModelGdaClient.GetRelatedValues(source, propIds, association);
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} GetRelatedValuesAsync => Failed to get related values for GID {source:X16}." +
					$"{Environment.NewLine} Exception message: {e.Message}" +
					$"{Environment.NewLine} Stack trace: {e.StackTrace}";
				Logger.LogError(message);
				throw new Exception(message);
			}

			return await ProcessIteratorAsync(iteratorId);
		}
		public async Task<ResourceDescription> GetValuesAsync(long resourceId, List<ModelCode> propIds)
		{
			string verboseMessage = $"{baseLogString} entering GetValuesAsync method.";
			Logger.LogVerbose(verboseMessage);

			ResourceDescription resource;

			try
			{
				resource = await networkModelGdaClient.GetValues(resourceId, propIds);
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} GetValuesAsync => Failed to get values for GID {resourceId:X16}." +
					$"{Environment.NewLine} Exception message: {e.Message}" +
					$"{Environment.NewLine} Stack trace: {e.StackTrace}";
				Logger.LogError(message);
				throw new Exception(message);
			}

			return resource;
		}
		private async Task<List<ResourceDescription>> ProcessIteratorAsync(int iteratorId)
		{
			string verboseMessage = $"{baseLogString} entering ProcessIteratorAsync method.";
			Logger.LogVerbose(verboseMessage);

			int resourcesLeft;
			int numberOfResources = 10000;
			List<ResourceDescription> resourceDescriptions;

			try
			{
				resourcesLeft = await networkModelGdaClient.IteratorResourcesTotal(iteratorId);
				resourceDescriptions = new List<ResourceDescription>(resourcesLeft);

				while (resourcesLeft > 0)
				{
					List<ResourceDescription> rds = await networkModelGdaClient.IteratorNext(numberOfResources, iteratorId);
					resourceDescriptions.AddRange(rds);

					resourcesLeft = await networkModelGdaClient.IteratorResourcesLeft(iteratorId);
				}

				await networkModelGdaClient.IteratorClose(iteratorId);
			}
			catch (Exception e)
			{
				string message = $"{baseLogString} ProcessIteratorAsync => Failed to process iterator." +
				$"{Environment.NewLine} Exception message: {e.Message}" +
				$"{Environment.NewLine} Stack trace: {e.StackTrace}";
				Logger.LogError(message);
				throw new Exception(message);
			}
			
			return resourceDescriptions;
		}
	}
}
