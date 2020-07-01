using Common.PubSub;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using OMS.Common.PubSubContracts;
using OMS.Common.WcfClient.OMS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Configuration;
using Common.PubSubContracts.DataContracts.EMAIL;
using Common.OMS;
using OMS.Common.Cloud;

namespace OMS.CallingServiceImplementation
{
    public class CallTracker : INotifySubscriberContract
    {

        private readonly ICloudLogger Logger;
        private readonly Uri subscriberUri;
        private Timer timer;
        private ConcurrentQueue<long> calls;
        private int expectedCalls;
        private int timeInterval;
        private OutageModelReadAccessClient outageModelClient;
        private TracingAlgorithmClient tracingAlgorithmClient;
        private ModelResourcesDesc resourcesDesc;
        private IOutageTopologyModel outageTopology = null;
        public CallTracker()
        {
            subscriberUri = new Uri("fabric:/OMS.Cloud/CallingService");
            outageModelClient = OutageModelReadAccessClient.CreateClient();
            tracingAlgorithmClient = TracingAlgorithmClient.CreateClient();
            calls = new ConcurrentQueue<long>();
            resourcesDesc = new ModelResourcesDesc();
            Logger = CloudLoggerFactory.GetLogger();
            try
            {
                timeInterval = Int32.Parse(ConfigurationManager.AppSettings["TimerInterval"]);
                Logger.LogInformation($"TIme interval is set to: {timeInterval}.");

            }
            catch (Exception e)
            {
                Logger.LogWarning("String in config file is not in valid format. Default values for timeInterval will be set.", e);
                timeInterval = 60000;
            }

            try
            {
                expectedCalls = Int32.Parse(ConfigurationManager.AppSettings["ExpectedCalls"]);
                Logger.LogInformation($"Expected calls is set to: {expectedCalls}.");
            }
            catch (Exception e)
            {
                Logger.LogWarning("String in config file is not in valid format. Default values for expected calls will be set.", e);
                expectedCalls = 3;
            }

            this.timer = new Timer();
            this.timer.Interval = timeInterval;
            this.timer.Elapsed += TimerElapsedMethod;
            this.timer.AutoReset = false;

        }

        private void StartTimer()
        {
            timer.Start();
        }

        private void TimerElapsedMethod(object sender, ElapsedEventArgs e)
        {
            if (calls.Count < expectedCalls)
            {
                Logger.LogInformation($"Timer elapsed (timer interval is {timeInterval}) and there is no enough calls to start tracing algorithm.");
            }
            else
            {
                tracingAlgorithmClient.StartTracingAlgorithm(calls.ToList());
            }

            calls = new ConcurrentQueue<long>();
        }

        private void StopTimer()
        {
            calls = new ConcurrentQueue<long>();
            timer.Stop();
        }

        #region INotifySubscriberContract

        public async Task<Uri> GetSubscriberUri()
        {
            return subscriberUri;
        }

        public async Task Notify(IPublishableMessage message)
        {
            if(outageTopology == null)
            {
                outageTopology = await outageModelClient.GetTopologyModel();
            }
            if (message is EmailToOutageMessage emailMessage)
            {
                if (emailMessage.Gid == 0)
                {
                    Logger.LogError("Invalid email received.");
                    return;
                }

                Logger.LogInformation($"Received call from Energy Consumer with GID: 0x{emailMessage.Gid:X16}.");

                if (!resourcesDesc.GetModelCodeFromId(emailMessage.Gid).Equals(ModelCode.ENERGYCONSUMER))
                {
                    Logger.LogWarning("Received GID is not id of energy consumer.");
                }
                else if (!outageTopology.OutageTopology.ContainsKey(emailMessage.Gid) && outageTopology.FirstNode != emailMessage.Gid)
                {
                    Logger.LogWarning("Received GID is not part of topology");
                }
                else
                {
                    if (!timer.Enabled) //first message
                    {
                        StartTimer();
                    }

                    calls.Enqueue(emailMessage.Gid);
                    Logger.LogInformation($"Current number of calls is: {calls.Count}.");
                    if (calls.Count >= expectedCalls)
                    {
                       await tracingAlgorithmClient.StartTracingAlgorithm(calls.ToList());

                        StopTimer();
                    }
                }
            }
            else
            {
                Logger.LogWarning("Received message is not EmailToOutageMessage.");
            }
        }
        #endregion
    }
}
