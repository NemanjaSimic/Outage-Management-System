using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outage.Common;
using Outage.Common.GDA;
using System.ServiceModel;

namespace NetworkModelServiceFunctions
{
	class NetworkModelGDA
	{
		private readonly string endpointName;

		public NetworkModelGDA(string endpointName)
		{
			this.endpointName = endpointName;
		}

		public UpdateResult ApplyUpdate(Delta delta)
		{
			throw new NotImplementedException();
		}

		public List<ResourceDescription> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			int iteratorId = 0, numberOfResources = 3, resourcesLeft = 0;

			List<ResourceDescription> resourceDescriptions = new List<ResourceDescription>();

			using (var proxy = new NetworkModelGDAProxy(endpointName))
			{
				iteratorId = proxy.GetExtentValues(entityType, propIds);
				resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

				do
				{
					resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
					List<ResourceDescription> rds = proxy.IteratorNext(numberOfResources, iteratorId);

					foreach (var resourceDescription in rds)
					{
						resourceDescriptions.Add(resourceDescription);
					}
				} while (resourcesLeft > 0);

				proxy.IteratorClose(iteratorId);
			}

			return resourceDescriptions;
		}

		public int GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			using (var proxy = new NetworkModelGDAProxy(endpointName))
			{
				return proxy.GetRelatedValues(source, propIds, association);
			}
		}

		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
		{
			using (var proxy = new NetworkModelGDAProxy(endpointName))
			{
				return proxy.GetValues(resourceId, propIds);
			}
		}

		public bool IteratorClose(int id)
		{
			throw new NotImplementedException();
		}

		public List<ResourceDescription> IteratorNext(int n, int id)
		{
			throw new NotImplementedException();
		}

		public int IteratorResourcesLeft(int id)
		{
			throw new NotImplementedException();
		}

		public int IteratorResourcesTotal(int id)
		{
			throw new NotImplementedException();
		}

		public bool IteratorRewind(int id)
		{
			throw new NotImplementedException();
		}
	}
}
