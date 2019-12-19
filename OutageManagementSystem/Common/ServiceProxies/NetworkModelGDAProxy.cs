using System.Collections.Generic;
using System.ServiceModel;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts;

namespace Outage.Common.ServiceProxies
{
	public class NetworkModelGDAProxy : ClientBase<INetworkModelGDAContract>, INetworkModelGDAContract
	{
		public NetworkModelGDAProxy(string endpointName)
			: base(endpointName)
		{
		}

		public UpdateResult ApplyUpdate(Delta delta)
		{
			return Channel.ApplyUpdate(delta);
		}

		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
		{
			return Channel.GetValues(resourceId, propIds);
		}

		public int GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			return Channel.GetExtentValues(entityType, propIds); 
		}	

		public int GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			return Channel.GetRelatedValues(source, propIds, association);
		}

		
		public bool IteratorClose(int id)
		{
			return Channel.IteratorClose(id);
		}

		public List<ResourceDescription> IteratorNext(int n, int id)
		{
			return Channel.IteratorNext(n, id);
		}

		public int IteratorResourcesLeft(int id)
		{
			return Channel.IteratorResourcesLeft(id);
		}

		public int IteratorResourcesTotal(int id)
		{
			return Channel.IteratorResourcesTotal(id);
		}

		public bool IteratorRewind(int id)
		{
			return Channel.IteratorRewind(id);
		}
	}
}
