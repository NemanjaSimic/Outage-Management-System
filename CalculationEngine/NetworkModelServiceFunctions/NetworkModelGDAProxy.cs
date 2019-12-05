using Outage.Common;
using Outage.Common.Contracts;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace NetworkModelServiceFunctions
{
	class NetworkModelGDAProxy : ClientBase<INetworkModelGDAContract>, INetworkModelGDAContract , IDisposable
	{
		private INetworkModelGDAContract proxy;
		public NetworkModelGDAProxy(string endpointName) : base (endpointName)
		{
			proxy = CreateChannel();
		}

		public UpdateResult ApplyUpdate(Delta delta)
		{
			throw new NotImplementedException();
		}

		public int GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			return proxy.GetExtentValues(entityType, propIds);
		}

		public int GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			return proxy.GetRelatedValues(source, propIds, association);
		}

		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
		{
			return proxy.GetValues(resourceId, propIds);
		}

		public bool IteratorClose(int id)
		{
			return proxy.IteratorClose(id);
		}

		public List<ResourceDescription> IteratorNext(int n, int id)
		{
			return proxy.IteratorNext(n, id);
		}

		public int IteratorResourcesLeft(int id)
		{
			return proxy.IteratorResourcesLeft(id);
		}

		public int IteratorResourcesTotal(int id)
		{
			return proxy.IteratorResourcesTotal(id);
		}

		public bool IteratorRewind(int id)
		{
			return proxy.IteratorRewind(id);
		}

		public void Dispose()
		{
			if (proxy != null)
			{
				proxy = null;
			}

			this.Close();

		}
	}
}
