using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.Calling
{
    public class TrackingAlgorithm
    {
        private OutageModel outageModel;

        public TrackingAlgorithm(OutageModel outageModel)
        {
            this.outageModel = outageModel;
        }

        public void Start(ConcurrentQueue<long> calls)
        {

        }
    }
}
