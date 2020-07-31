using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NMS.GdaImplementation.GDA
{
    public class ResourceIterator
    {
        private const int maxReturnNo = 5000;
        private readonly NetworkModel networkModel = null;

        private int lastReadIndex = 0; // index of the last read resource description
        private List<long> globalDs = new List<long>();
        private Dictionary<DMSType, List<ModelCode>> class2PropertyIDs = new Dictionary<DMSType, List<ModelCode>>();

        #region Private Properties
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public ResourceIterator(NetworkModel networkModel)
        {
            this.networkModel = networkModel;
        }

        public ResourceIterator(List<long> globalIDs, Dictionary<DMSType, List<ModelCode>> class2PropertyIDs, NetworkModel networkModel)
        {
            this.globalDs = globalIDs;
            this.class2PropertyIDs = class2PropertyIDs;
            this.networkModel = networkModel;
        }

        public int ResourcesLeft()
        {
            return globalDs.Count - lastReadIndex;
        }

        public int ResourcesTotal()
        {
            return globalDs.Count;
        }

        public async Task<List<ResourceDescription>> Next(int n)
        {
            try
            {
                if (n < 0)
                {
                    return null;
                }

                if (n > maxReturnNo)
                {
                    n = maxReturnNo;
                }

                List<long> resultIDs;

                if (ResourcesLeft() < n)
                {
                    resultIDs = globalDs.GetRange(lastReadIndex, globalDs.Count - lastReadIndex);
                    lastReadIndex = globalDs.Count;
                }
                else
                {
                    resultIDs = globalDs.GetRange(lastReadIndex, n);
                    lastReadIndex += n;
                }

                List<ResourceDescription> result = await CollectData(resultIDs);

                return result;
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to get next set of ResourceDescription iterators. {0}", ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        public async Task<List<ResourceDescription>> GetRange(int index, int n)
        {
            try
            {
                if (n > maxReturnNo)
                {
                    n = maxReturnNo;
                }

                List<long> resultIDs = globalDs.GetRange(index, n);

                List<ResourceDescription> result = await CollectData(resultIDs);

                return result;
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to get range of ResourceDescription iterators. index:{0}, count:{1}. {2}", index, n, ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        public void Rewind()
        {
            lastReadIndex = 0;
        }

        private async Task<List<ResourceDescription>> CollectData(List<long> resultIDs)
        {
            try
            {
                List<ResourceDescription> result = new List<ResourceDescription>();

                List<ModelCode> propertyIds = null;
                foreach (long globalId in resultIDs)
                {
                    propertyIds = class2PropertyIDs[(DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId)];
                    result.Add(await networkModel.GetValues(globalId, propertyIds));
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Collecting ResourceDescriptions failed. Exception: {ex.Message}", ex);
                throw;
            }
        }
    }
}
