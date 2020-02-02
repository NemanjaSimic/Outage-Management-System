using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.EmailDataContract;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OutageManagementService.Calling
{
    public class CallTracker : ISubscriberCallback
    {
        private ILogger logger;
        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private Timer timer;
        private TrackingAlgorithm trackingAlgorithm;
        private ConcurrentQueue<long> calls;
        private string subscriberName;
        private int expectedCalls;
        private int timeInterval;
        private OutageModel outageModel;

        private ModelResourcesDesc resourcesDesc;

        public CallTracker(string subscriberName, OutageModel outageModel)
        {
           

            this.trackingAlgorithm = new TrackingAlgorithm(outageModel);
            this.calls = new ConcurrentQueue<long>();
            this.subscriberName = subscriberName;
            this.outageModel = outageModel;
            this.resourcesDesc = new ModelResourcesDesc();

            try
            {
                timeInterval = Int32.Parse(ConfigurationManager.AppSettings["TimerInterval"]);
                expectedCalls = Int32.Parse(ConfigurationManager.AppSettings["ExpectedCalls"]);
            }
            catch(Exception e)
            {
                Logger.LogError("String in config file is not in valid format.", e);
                throw e;
            }

            this.timer = new Timer();
            this.timer.Interval = timeInterval;
            this.timer.Elapsed += TimerElapsedMethod;
            this.timer.AutoReset = false;
        }

        public string GetSubscriberName()
        {
            return subscriberName;
        }

        public void Notify(IPublishableMessage message)
        {
            if (message is EmailToOutageMessage emailMessage)
            {
                if (!resourcesDesc.GetModelCodeFromId(emailMessage.Gid).Equals(ModelCode.ENERGYCONSUMER))
                {
                    Logger.LogWarn("Received GID is not id of energy consumer.");
                }
                else if (!outageModel.topology.Nodes.ContainsKey(emailMessage.Gid) && outageModel.topology.FirstNode != emailMessage.Gid)
                {
                    Logger.LogWarn("Received GID is not part of topology");
                }
                else
                {
                    if (!timer.Enabled) //first message
                    {
                        StartTimer();
                    }

                    calls.Enqueue(emailMessage.Gid);
                    if (calls.Count >= expectedCalls)
                    {
                        trackingAlgorithm.Start(calls);
                        
                        StopTimer();
                    }
                }
            }
            else
            {
                Logger.LogWarn("Received message is not EmailToOutageMessage.");
            }
        }

        private void StartTimer()
        {
            timer.Start();
        }

        private void TimerElapsedMethod(object sender, ElapsedEventArgs e)
        {
            if (calls.Count < expectedCalls)
            {
                Logger.LogInfo($"Timer elapsed (timer interval is {timeInterval}) and there is no enough calls to start tracing algorithm.");
            }
            else
            {
                trackingAlgorithm.Start(calls);
            }

            calls = new ConcurrentQueue<long>();
        }


        //method for manual stopping of timer
        private void StopTimer()
        {
            calls = new ConcurrentQueue<long>();
            timer.Stop();
        }
    }
}
