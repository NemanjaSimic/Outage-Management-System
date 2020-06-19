using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NMS.GdaImplementation.GDA
{
    public class GenericDataAccess : INetworkModelGDAContract
    {
        private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private static Dictionary<int, ResourceIterator> resourceIterators = new Dictionary<int, ResourceIterator>();
        private static int resourceItId = 0;

        private readonly NetworkModel networkModel;

        public GenericDataAccess(NetworkModel networkModel)
        {
            this.networkModel = networkModel;
        }

        public async Task<UpdateResult> ApplyUpdate(Delta delta)
        {
            return await networkModel.ApplyDelta(delta);
        }

        public async Task<ResourceDescription> GetValues(long resourceId, List<ModelCode> propIds)
        {
            try
            {
                ResourceDescription retVal = await networkModel.GetValues(resourceId, propIds);
                return retVal;
            }
            catch (Exception ex)
            {
                string message = string.Format("Getting values for resource with ID: 0x{0:X16} failed. {1}", resourceId, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        public async Task<int> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            try
            {
                ResourceIterator ri = await networkModel.GetExtentValues(entityType, propIds);
                int retVal = AddIterator(ri);
                return retVal;
            }
            catch (Exception ex)
            {
                string message = string.Format("Getting extent values for ModelCode = {0} failed. ", entityType, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        public async Task<int> GetRelatedValues(long source, List<ModelCode> propIds, Association association)
        {
            try
            {
                ResourceIterator ri = await networkModel.GetRelatedValues(source, propIds, association);
                int retVal =  AddIterator(ri);
                return retVal;
            }
            catch (Exception ex)
            {
                string message = string.Format("Getting related values for resource with ID: 0x{0:X16} failed. {1}", source, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        public async Task<List<ResourceDescription>> IteratorNext(int n, int id)
        {
            try
            {
                ResourceIterator iterator = GetIterator(id);
                return await iterator.Next(n);
            }
            catch (Exception ex)
            {
                string message = string.Format("IteratorNext failed. Iterator ID: {0}. Resources to fetch count = {1}. {2} ", id, n, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        public async Task<bool> IteratorRewind(int id)
        {
            try
            {
                ResourceIterator iterator = GetIterator(id);
                iterator.Rewind();
                return true;
            }
            catch (Exception ex)
            {
                string message = string.Format("IteratorRewind failed. Iterator ID: {0}. {1}", id, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        public async Task<int> IteratorResourcesTotal(int id)
        {
            try
            {
                ResourceIterator iterator = GetIterator(id);
                return iterator.ResourcesTotal();
            }
            catch (Exception ex)
            {
                string message = string.Format("IteratorResourcesTotal failed. Iterator ID: {0}. {1}", id, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        public async Task<int> IteratorResourcesLeft(int id)
        {
            try
            {
                ResourceIterator iterator = GetIterator(id);
                return iterator.ResourcesLeft();
            }
            catch (Exception ex)
            {
                string message = string.Format("IteratorResourcesLeft failed. Iterator ID: {0}. {1}", id, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        public async Task<bool> IteratorClose(int id)
        {
            try
            {
                bool retVal = RemoveIterator(id);
                return retVal;
            }
            catch (Exception ex)
            {
                string message = string.Format("IteratorClose failed. Iterator ID: {0}. {1}", id, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        private int AddIterator(ResourceIterator iterator)
        {
            lock (resourceIterators)
            {
                int iteratorId = ++resourceItId;
                resourceIterators.Add(iteratorId, iterator);
                return iteratorId;
            }
        }

        private ResourceIterator GetIterator(int iteratorId)
        {
            lock (resourceIterators)
            {
                if (resourceIterators.ContainsKey(iteratorId))
                {
                    return resourceIterators[iteratorId];
                }
                else
                {
                    throw new Exception(string.Format("Iterator with given ID: {0} doesn't exist.", iteratorId));
                }
            }
        }

        private bool RemoveIterator(int iteratorId)
        {
            lock (resourceIterators)
            {
                return resourceIterators.Remove(iteratorId);
            }
        }
    }
}
