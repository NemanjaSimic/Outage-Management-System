using System;
using System.Collections.Generic;
using System.ServiceModel;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.GDA;

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
            UpdateResult result;

            try
            {
                result = Channel.ApplyUpdate(delta);
            }
            catch (Exception e)
            {
                string message = "Exception in ApplyUpdate() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}

		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
		{
            ResourceDescription resource;

            try
            {
                resource = Channel.GetValues(resourceId, propIds);
            }
            catch (Exception e)
            {
                string message = "Exception in GetValues() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return resource;
		}

		public int GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
            int result;

            try
            {
                result = Channel.GetExtentValues(entityType, propIds);
            }
            catch (Exception e)
            {
                string message = "Exception in GetExtentValues() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}	

		public int GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
            int result;

            try
            {
                result = Channel.GetRelatedValues(source, propIds, association);
            }
            catch (Exception e)
            {
                string message = "Exception in GetRelatedValues() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}

		
		public bool IteratorClose(int id)
		{
            bool success;

            try
            {
                success = Channel.IteratorClose(id);
            }
            catch (Exception e)
            {
                string message = "Exception in IteratorClose() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
		}

		public List<ResourceDescription> IteratorNext(int n, int id)
		{
            List<ResourceDescription> result;

            try
            {
                result = Channel.IteratorNext(n, id);
            }
            catch (Exception e)
            {
                string message = "Exception in IteratorNext() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}

		public int IteratorResourcesLeft(int id)
		{
            int result;

            try
            {
                result = Channel.IteratorResourcesLeft(id);
            }
            catch (Exception e)
            {
                string message = "Exception in IteratorResourcesLeft() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}

		public int IteratorResourcesTotal(int id)
		{
            int result;

            try
            {
                result = Channel.IteratorResourcesTotal(id);
            }
            catch (Exception e)
            {
                string message = "Exception in IteratorResourcesTotal() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return result;
		}

		public bool IteratorRewind(int id)
		{
            bool success;

            try
            {
                success = Channel.IteratorRewind(id);
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
