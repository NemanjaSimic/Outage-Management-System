using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceProxies;
using System;
using System.Collections.Generic;

namespace NetworkModelServiceFunctions
{
	public class NetworkModelGDA
	{
		private ILogger logger = LoggerWrapper.Instance;
		public List<ResourceDescription> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			int iteratorId = 0;
			int numberOfTries = 0;
			while (numberOfTries < 5)
			{
				try
				{
					numberOfTries++;
					using (var proxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint))
					{
						iteratorId = proxy.GetExtentValues(entityType, propIds);
					}
					break;
				}
				catch (Exception ex)
				{				
					logger.LogError($"Failed to get extent values for entity type {entityType.ToString()}. Exception message: " + ex.Message);
					logger.LogWarn($"Retrying to connect to NMSProxy. Number of tries: {numberOfTries}.");
				}
			}

			return ProcessIterator(iteratorId);
		}
		public List<ResourceDescription> GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			int iteratorId = 0;
			try
			{
				using (var proxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint))
				{
					iteratorId = proxy.GetRelatedValues(source, propIds, association);
				}
			}
			catch (Exception ex)
			{
				string message = $"Failed to get related values for element with GID {source.ToString()}. Exception message: " + ex.Message;
				logger.LogError(message);
			}

			return ProcessIterator(iteratorId);
		}
		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
		{
			ResourceDescription rs = new ResourceDescription();
			try
			{
				using (var proxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint))
				{
					rs = proxy.GetValues(resourceId, propIds);
				}
			}
			catch (Exception ex)
			{
				string message = $"Failed to get values for elemnt with GID {resourceId.ToString()}. Exception message: " + ex.Message;
				logger.LogError(message);
			}
			return rs;
		}
		private List<ResourceDescription> ProcessIterator(int iteratorId)
		{
            //TODO: mozda vec ovde napakovati dictionary<long, rd> ?
			int numberOfResources = 10000, resourcesLeft = 0;
			List<ResourceDescription> resourceDescriptions = new List<ResourceDescription>();

			try
			{
				using (var proxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint))
				{
					do
					{
						List<ResourceDescription> rds = proxy.IteratorNext(numberOfResources, iteratorId);
						resourceDescriptions.AddRange(rds);

						resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

					} while (resourcesLeft > 0);

					proxy.IteratorClose(iteratorId);
				}
			}
			catch (Exception ex)
			{
				string message = $"Failed to retrieve all Resourse descriptions with iterator {iteratorId}. Exception message: " + ex.Message;
				logger.LogError(message);
			}
			return resourceDescriptions;
		}
	}
}
