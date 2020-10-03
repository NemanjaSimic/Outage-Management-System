using Common.OMS;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.EMAIL;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.NmsContracts;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.Interfaces;
using OMS.Common.WcfClient.CE;
using System;
using System.Configuration;
using System.Fabric;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;

namespace OMS.CallTrackingImplementation
{
    public class CallTracker : INotifySubscriberContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;
        private readonly ModelResourcesDesc modelResourcesDesc;
        private readonly TrackingAlgorithm trackingAlgorithm;

        private int expectedCalls;
        private int timerInterval;
        private string subscriberName;
        private Timer timer;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        #region Reliable Dictionaries
        private bool isCallsDictionaryInitialized;

        private bool ReliableDictionariesInitialized
        {
            get
            {
                return true;
            }
        }

        //TODO: Queue, for now Dictionary (gid, gid)
        private ReliableDictionaryAccess<long, long> calls;

        private ReliableDictionaryAccess<long, long> Calls
        {
            get {   return calls;   }
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs eventArgs)
        {
            try
            {
                await InitializeReliableCollections(eventArgs);
            }
            catch (FabricNotPrimaryException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => NotPrimaryException. To be ignored.");
            }
            catch (FabricObjectClosedException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => FabricObjectClosedException. To be ignored.");
            }
            catch (COMException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => {typeof(COMException)}. To be ignored.");
            }
        }

        private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.CallsDictionary)
                {
                    calls = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.CallsDictionary);
                    this.isCallsDictionaryInitialized = true;
                }
            }
        }
        #endregion Reliable Dictionaries

        public CallTracker(IReliableStateManager stateManager, string subscriberName)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            this.stateManager = stateManager;
            this.calls = new ReliableDictionaryAccess<long, long>(stateManager, ReliableDictionaryNames.CallsDictionary);

            this.subscriberName = subscriberName;

            modelResourcesDesc = new ModelResourcesDesc();
            trackingAlgorithm = new TrackingAlgorithm();

            ImportFromConfig();

            //timer initialization
            timer = new Timer();
            timer.Interval = timerInterval;
            timer.Elapsed += TimerElapsedMethod;
            timer.AutoReset = false;
        }

        private void ImportFromConfig()
        {
            //timer interval and expected calls initialization
            try
            {
                timerInterval = int.Parse(ConfigurationManager.AppSettings["TimerInterval"]);
                Logger.LogInformation($"TIme interval is set to: {timerInterval}.");

            }
            catch (Exception e)
            {
                Logger.LogWarning("String in config file is not in valid format. Default values for timeInterval will be set.", e);
                timerInterval = 60000;
            }

            try
            {
                expectedCalls = int.Parse(ConfigurationManager.AppSettings["ExpectedCalls"]);
                Logger.LogInformation($"Expected calls is set to: {expectedCalls}.");
            }
            catch (Exception e)
            {
                Logger.LogWarning("String in config file is not in valid format. Default values for expected calls will be set.", e);
                expectedCalls = 3;
            }
        }

        #region INotifySubscriberContract
        public async Task Notify(IPublishableMessage message, string publisherName)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            if (message is EmailToOutageMessage emailMessage)
            {
                if (emailMessage.Gid == 0)
                {
                    Logger.LogError("Invalid email received.");
                    return;
                }

                Logger.LogInformation($"Received call from Energy Consumer with GID: 0x{emailMessage.Gid:X16}.");

                if (!modelResourcesDesc.GetModelCodeFromId(emailMessage.Gid).Equals(ModelCode.ENERGYCONSUMER))
                {
                    Logger.LogWarning($"Received GID 0x{emailMessage.Gid:X16} is not id of energy consumer.");
                    return;
                }

                var topologyProviderClient = TopologyProviderClient.CreateClient();
                var topology = await topologyProviderClient.GetOMSModel();
                
                if (!topology.GetElementByGid(emailMessage.Gid, out OutageTopologyElement elment))
                {
                    Logger.LogWarning($"Received GID 0x{emailMessage.Gid:X16} is not part of topology.");
                    return;
                }

                if (!timer.Enabled)
                {
                    timer.Start();
                }

                await Calls.SetAsync(emailMessage.Gid, emailMessage.Gid);
                Logger.LogInformation($"Current number of calls is: {await Calls.GetCountAsync()}.");

                if (await Calls.GetCountAsync() >= expectedCalls)
                {
                    await trackingAlgorithm.Start((await Calls.GetDataCopyAsync()).Keys.ToList());

                    await Calls.ClearAsync();
                    timer.Stop();
                }
            }
        }

        public Task<string> GetSubscriberName()
        {
            return Task.Run(() => subscriberName);
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => true);
        }
        #endregion

        private void TimerElapsedMethod(object sender, ElapsedEventArgs e)
        {
            if (Calls.GetCountAsync().Result < expectedCalls)
            {
                Logger.LogInformation($"Timer elapsed (timer interval is {timerInterval}) and there is no enough calls to start tracing algorithm.");
            }
            else
            {
                trackingAlgorithm.Start(Calls.GetDataCopyAsync().Result.Keys.ToList()).Wait();
            }

            Calls.ClearAsync().Wait();
        }
    }
}
