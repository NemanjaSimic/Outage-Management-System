using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;

namespace NetworkModelServiceFunctions
{
	public class NetworkModelGDA
	{
		private ILogger logger = LoggerWrapper.Instance;
		public List<ResourceDescription> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			int iteratorId;
			try
			{
				using (var proxy = new Outage.Common.ServiceProxies.NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint))
				{
					iteratorId = proxy.GetExtentValues(entityType, propIds);
				}
			}
			catch (Exception ex)
			{
				string message = $"Failed to get extent values for entity type {entityType.ToString()}. Exception message: " + ex.Message;
				logger.LogError(message);
				throw ex;
			}

			return ProcessIterator(iteratorId);
		}
		public List<ResourceDescription> GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			int iteratorId;
			try
			{
				using (var proxy = new Outage.Common.ServiceProxies.NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint))
				{
					iteratorId = proxy.GetRelatedValues(source, propIds, association);
				}
			}
			catch (Exception ex)
			{
				string message = $"Failed to get related values for element with GID {source.ToString()}. Exception message: " + ex.Message;
				logger.LogError(message);
				throw ex;
			}

			return ProcessIterator(iteratorId);
		}
		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
		{
			try
			{
				using (var proxy = new Outage.Common.ServiceProxies.NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint))
				{
					return proxy.GetValues(resourceId, propIds);
				}
			}
			catch (Exception ex)
			{
				string message = $"Failed to get values for elemnt with GID {resourceId.ToString()}. Exception message: " + ex.Message;
				logger.LogError(message);
				throw ex;
			}
		}
		private List<ResourceDescription> ProcessIterator(int iteratorId)
		{
            //TODO: mozda vec ovde napakovati dictionary<long, rd> ?
			int numberOfResources = 50, resourcesLeft = 0;
			List<ResourceDescription> resourceDescriptions = new List<ResourceDescription>();

			try
			{
				using (var proxy = new Outage.Common.ServiceProxies.NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint))
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
				throw ex;
			}
			return resourceDescriptions;
		}
	}
}
