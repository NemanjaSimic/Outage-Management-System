using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceProxies;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace NetworkModelServiceFunctions
{
	public class NetworkModelGDA
	{
		private ProxyFactory proxyFactory;

		private ILogger logger = LoggerWrapper.Instance;

		protected ILogger Logger
		{
			get { return logger ?? (logger = LoggerWrapper.Instance); }
		}

		public NetworkModelGDA()
		{
			proxyFactory = new ProxyFactory();
		}

		public List<ResourceDescription> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			int iteratorId;

			using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
			{
				if (gdaQueryProxy == null)
				{
					string message = "GetExtentValues() => NetworkModelGDAProxy is null.";
					Logger.LogError(message);
					throw new NullReferenceException(message);
				}

				try
				{
					iteratorId = gdaQueryProxy.GetExtentValues(entityType, propIds);
				}
				catch (Exception e)
				{
					string message = $"Failed to get extent values for dms type {entityType}.";
					Logger.LogError(message, e);
					throw e;
				}
			}

			return ProcessIterator(iteratorId);
		}

		public List<ResourceDescription> GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			int iteratorId;

			using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
			{
				if (gdaQueryProxy == null)
				{
					string message = "GetRelatedValues() => NetworkModelGDAProxy is null.";
					Logger.LogError(message);
					throw new NullReferenceException(message);
				}

				try
				{
					iteratorId = gdaQueryProxy.GetRelatedValues(source, propIds, association);
				}
				catch (Exception e)
				{
					string message = $"Failed to get related values for element with GID {source}.";
					Logger.LogError(message, e);
					throw e;
				}
			}

			return ProcessIterator(iteratorId);
		}

		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
		{
			ResourceDescription resource;

			using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
			{
				if (gdaQueryProxy == null)
				{
					string message = "GetValues() => NetworkModelGDAProxy is null.";
					Logger.LogError(message);
					throw new NullReferenceException(message);
				}

				try
				{
					resource = gdaQueryProxy.GetValues(resourceId, propIds);
				}
				catch (Exception e)
				{
					string message = $"Failed to get values for elemnt with GID {resourceId}.";
					Logger.LogError(message, e);
					throw e;
				}
			}

			return resource;
		}

		private List<ResourceDescription> ProcessIterator(int iteratorId)
		{
			//TODO: mozda vec ovde napakovati dictionary<long, rd> ?
			int resourcesLeft;
			int numberOfResources = 10000;
			List<ResourceDescription> resourceDescriptions;

			using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
			{
				if (gdaQueryProxy == null)
				{
					string message = "ProcessIterator() => NetworkModelGDAProxy is null.";
					Logger.LogError(message);
					throw new NullReferenceException(message);
				}

				try
				{
					resourcesLeft = gdaQueryProxy.IteratorResourcesTotal(iteratorId);
					resourceDescriptions = new List<ResourceDescription>(resourcesLeft);

					while (resourcesLeft > 0)
					{
						List<ResourceDescription> rds = gdaQueryProxy.IteratorNext(numberOfResources, iteratorId);
						resourceDescriptions.AddRange(rds);

						resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);
					}

					gdaQueryProxy.IteratorClose(iteratorId);
				}
				catch (Exception e)
				{
					string message = $"Failed to retrieve all Resourse descriptions with iterator {iteratorId}.";
					Logger.LogError(message, e);
					throw e;
				}
			}

			return resourceDescriptions;
		}
	}
}
