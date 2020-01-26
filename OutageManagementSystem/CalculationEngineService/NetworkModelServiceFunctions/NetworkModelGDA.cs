using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceProxies;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NetworkModelServiceFunctions
{
	public class NetworkModelGDA
	{
		private ILogger logger = LoggerWrapper.Instance;

		#region Proxies

		private NetworkModelGDAProxy gdaQueryProxy = null;

		protected NetworkModelGDAProxy GdaQueryProxy
		{
			get
			{
				int numberOfTries = 0;

				while (numberOfTries < 10)
				{
					try
					{
						if (gdaQueryProxy != null)
						{
							gdaQueryProxy.Abort();
							gdaQueryProxy = null;
						}

						gdaQueryProxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint);
						gdaQueryProxy.Open();
						break;
					}
					catch (Exception ex)
					{
						string message = $"Exception on NetworkModelGDAProxy initialization. Message: {ex.Message}";
						logger.LogError(message, ex);
						gdaQueryProxy = null;
					}
					finally
					{
						numberOfTries++;
						logger.LogDebug($"SCADAModel: GdaQueryProxy getter, try number: {numberOfTries}.");
						Thread.Sleep(500);
					}
				}

				return gdaQueryProxy;
			}
		}

		#endregion Proxies

		public List<ResourceDescription> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			int iteratorId;

			try
			{
				using (NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
				{
					if (gdaProxy != null)
					{
						iteratorId = gdaProxy.GetExtentValues(entityType, propIds);
					}
					else
					{
						string message = "From method GetExtentValues(): NetworkModelGDAProxy is null.";
						logger.LogError(message);
						throw new NullReferenceException(message);
					}
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
				using (NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
				{
					if (gdaProxy != null)
					{
						iteratorId = gdaProxy.GetRelatedValues(source, propIds, association);
					}
					else
					{
						string message = "From method GetRelatedValues(): NetworkModelGDAProxy is null.";
						logger.LogError(message);
						throw new NullReferenceException(message);
					}
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
			ResourceDescription resource;

			try
			{
				using (NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
				{
					if (gdaProxy != null)
					{
						resource = gdaProxy.GetValues(resourceId, propIds);
					}
					else
					{
						string message = "From method GetValues(): NetworkModelGDAProxy is null.";
						logger.LogError(message);
						throw new NullReferenceException(message);
					}
				}
			}
			catch (Exception ex)
			{
				string message = $"Failed to get values for elemnt with GID {resourceId.ToString()}. Exception message: " + ex.Message;
				logger.LogError(message);
				throw ex;
			}

			return resource;
		}
		private List<ResourceDescription> ProcessIterator(int iteratorId)
		{
            //TODO: mozda vec ovde napakovati dictionary<long, rd> ?
			int numberOfResources = 50, resourcesLeft = 0;
			List<ResourceDescription> resourceDescriptions = new List<ResourceDescription>();

			try
			{
				using (NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
				{
					if (gdaProxy != null)
					{
						do
						{
							List<ResourceDescription> rds = gdaProxy.IteratorNext(numberOfResources, iteratorId);
							resourceDescriptions.AddRange(rds);

							resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);

						} while (resourcesLeft > 0);

						gdaProxy.IteratorClose(iteratorId);
					}
					else
					{
						string message = "From method ProcessIterator(): NetworkModelGDAProxy is null.";
						logger.LogError(message);
						throw new NullReferenceException(message);
					}
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
