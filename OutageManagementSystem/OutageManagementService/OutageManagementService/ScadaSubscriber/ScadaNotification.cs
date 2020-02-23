using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.PubSub;
using OutageManagementService.OutageIsolationAlgorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.ScadaSubscriber
{
    public class ScadaNotification : ISubscriberCallback
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public string SubscriberName { get; set; }

        public OutageIsolationAlgorithmParameters OutageIsolationAlgorithmParameters { get; set; }

        public ScadaNotification(string subscriberName, OutageIsolationAlgorithmParameters outageIsolationAlgorithmParameters)
        {
            SubscriberName = subscriberName;
            OutageIsolationAlgorithmParameters = outageIsolationAlgorithmParameters;
        }
        public string GetSubscriberName()
        {
            return SubscriberName;
        }

        public void Notify(IPublishableMessage message)
        {
            if (message is MultipleDiscreteValueSCADAMessage multipleDiscreteValueSCADAMessage)
            {
                long headBreakerID = OutageIsolationAlgorithmParameters.HeadBreakerId;
                long recloserID = OutageIsolationAlgorithmParameters.RecloserId;

                if (multipleDiscreteValueSCADAMessage.Data.ContainsKey(headBreakerID))
                {
                    if (multipleDiscreteValueSCADAMessage.Data[headBreakerID].Value == (ushort)DiscreteCommandingType.OPEN)
                    {
                        OutageIsolationAlgorithmParameters.AutoResetEvent.Set();
                    }

                }
            }
        }
    }
}
