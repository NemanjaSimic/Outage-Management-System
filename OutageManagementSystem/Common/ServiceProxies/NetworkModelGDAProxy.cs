using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.GDA;

namespace Outage.Common.ServiceProxies
{
	public class NetworkModelGDAProxy : BaseProxy<INetworkModelGDAContract>, INetworkModelGDAContract
	{
		public NetworkModelGDAProxy(string endpointName)
			: base(endpointName)
		{
		}

		public async Task<UpdateResult> ApplyUpdate(Delta delta)
		{
            UpdateResult result;

            try
            {
                result = await Channel.ApplyUpdate(delta);
            }
            catch (Exception e)
            {
                string message = "Exception in ApplyUpdate() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}

		public async Task<ResourceDescription> GetValues(long resourceId, List<ModelCode> propIds)
		{
            ResourceDescription resource;

            try
            {
                resource = await Channel.GetValues(resourceId, propIds);
            }
            catch (Exception e)
            {
                string message = "Exception in GetValues() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return resource;
		}

		public async Task<int> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
            int result;

            try
            {
                result = await Channel.GetExtentValues(entityType, propIds);
            }
            catch (Exception e)
            {
                string message = "Exception in GetExtentValues() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}	

		public async Task<int> GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
            int result;

            try
            {
                result = await Channel.GetRelatedValues(source, propIds, association);
            }
            catch (Exception e)
            {
                string message = "Exception in GetRelatedValues() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}

		
		public async Task<bool> IteratorClose(int id)
		{
            bool success;

            try
            {
                success = await Channel.IteratorClose(id);
            }
            catch (Exception e)
            {
                string message = "Exception in IteratorClose() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
		}

		public async Task<List<ResourceDescription>> IteratorNext(int n, int id)
		{
            List<ResourceDescription> result;

            try
            {
                result = await Channel.IteratorNext(n, id);
            }
            catch (Exception e)
            {
                string message = "Exception in IteratorNext() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}

		public async Task<int> IteratorResourcesLeft(int id)
		{
            int result;

            try
            {
                result = await Channel.IteratorResourcesLeft(id);
            }
            catch (Exception e)
            {
                string message = "Exception in IteratorResourcesLeft() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}

		public async Task<int> IteratorResourcesTotal(int id)
		{
            int result;

            try
            {
                result = await Channel.IteratorResourcesTotal(id);
            }
            catch (Exception e)
            {
                string message = "Exception in IteratorResourcesTotal() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}

		public async Task<bool> IteratorRewind(int id)
		{
            bool success;

            try
            {
                success = await Channel.IteratorRewind(id);
            }
            catch (Exception e)
            {
                string message = "Exception in IteratorRewind() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
		}

	}
}
