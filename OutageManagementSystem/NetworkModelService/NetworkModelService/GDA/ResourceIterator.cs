using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.NetworkModelService.GDA
{
    public class ResourceIterator
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private readonly NetworkModel networkModel;
        private readonly List<long> globalDs = new List<long>();
        private readonly Dictionary<DMSType, List<ModelCode>> class2PropertyIDs = new Dictionary<DMSType, List<ModelCode>>();

        private int lastReadIndex = 0; // index of the last read resource description
        private readonly int maxReturnNo = 10000;

        public ResourceIterator(NetworkModel networkModel, List<long> globalIDs, Dictionary<DMSType, List<ModelCode>> class2PropertyIDs)
        {
            this.networkModel = networkModel;
            this.globalDs = globalIDs;
            this.class2PropertyIDs = class2PropertyIDs;
        }

        public int ResourcesLeft()
        {
            return globalDs.Count - lastReadIndex;
        }

        public int ResourcesTotal()
        {
            return globalDs.Count;
        }

        public List<ResourceDescription> Next(int n)
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

                List<ResourceDescription> result = CollectData(resultIDs);

                return result;
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to get next set of ResourceDescription iterators. {0}", ex.Message);
                Logger.LogError(message, ex);
                throw new Exception(message);
            }
        }

        public List<ResourceDescription> GetRange(int index, int n)
        {
            try
            {
                if (n > maxReturnNo)
                {
                    n = maxReturnNo;
                }

                List<long> resultIDs = globalDs.GetRange(index, n);

                List<ResourceDescription> result = CollectData(resultIDs);

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

        private List<ResourceDescription> CollectData(List<long> resultIDs)
        {
            try
            {
                List<ResourceDescription> result = new List<ResourceDescription>();

                List<ModelCode> propertyIds = null;
                foreach (long globalId in resultIDs)
                {
                    propertyIds = class2PropertyIDs[(DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId)];
                    result.Add(networkModel.GetValues(globalId, propertyIds));
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
