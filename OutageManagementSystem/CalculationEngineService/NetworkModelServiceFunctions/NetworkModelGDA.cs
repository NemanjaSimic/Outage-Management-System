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
	public class NetworkModelGDA
	{
		public List<ResourceDescription> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			int iteratorId;

			using (var proxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint))
			{
				iteratorId = proxy.GetExtentValues(entityType, propIds);			
			}

			return ProcessIterator(iteratorId);
		}

		public List<ResourceDescription> GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			int iteratorId;

			using (var proxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint))
			{
				iteratorId = proxy.GetRelatedValues(source, propIds, association);
			}

			return ProcessIterator(iteratorId);
		}

		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
		{
			using (var proxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint))
			{
				return proxy.GetValues(resourceId, propIds);
			}
		}

		private List<ResourceDescription> ProcessIterator(int iteratorId)
		{
            //TODO: mozda vec ovde napakovati dictionary<long, rd> ?
			int numberOfResources = 3, resourcesLeft = 0;

			List<ResourceDescription> resourceDescriptions = new List<ResourceDescription>();

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
			return resourceDescriptions;
		}
	}
}
