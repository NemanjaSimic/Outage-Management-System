using Common.OmsContracts.ModelProvider;
using Common.PubSubContracts.DataContracts.EMAIL;
using Microsoft.ServiceFabric.Data;
using OMS.Common.Cloud;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.NmsContracts;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.Interfaces;
using OMS.Common.WcfClient.OMS;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace OMS.CallTrackingServiceImplementation
{
    public class CallTracker : INotifySubscriberContract
	{
		//TODO: Queue, for now Dictionary (gid, gid)
		private ReliableDictionaryAccess<long, long> calls;

		public ReliableDictionaryAccess<long, long> Calls
		{
			get
			{
				return calls ?? (calls = ReliableDictionaryAccess<long, long>.Create(stateManager, "CallsDictionary").Result);
			}
		}

		private Timer timer;
		private readonly IReliableStateManager stateManager;
		private int expectedCalls;
		private int timerInterval;
		private string subscriberName;

		private ModelResourcesDesc modelResourcesDesc;

		private IOutageModelReadAccessContract outageModelReadAccessClient;
		private TrackingAlgorithm trackingAlgorithm;

		public Task<bool> IsAlive()
		{
			return Task.Run(() => { return true; });
		}
		public CallTracker(IReliableStateManager stateManager)
		{
			this.stateManager = stateManager;

			trackingAlgorithm = new TrackingAlgorithm();

			subscriberName = "CallTrackingService"; //TODO: ServiceDefines

			modelResourcesDesc = new ModelResourcesDesc();

			outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();

			//timer initialization
			timer = new Timer();
			timer.Interval = timerInterval;
			timer.Elapsed += TimerElapsedMethod;
			timer.AutoReset = false;

			//timer interval and expected calls initialization
			try
			{
				timerInterval = Int32.Parse(ConfigurationManager.AppSettings["TimerInterval"]);
				//Logger.LogInfo($"TIme interval is set to: {timerInterval}.");

			}
			catch (Exception e)
			{
				//Logger.LogWarn("String in config file is not in valid format. Default values for timeInterval will be set.", e);
				timerInterval = 60000;
			}

			try
			{
				expectedCalls = Int32.Parse(ConfigurationManager.AppSettings["ExpectedCalls"]);
				//Logger.LogInfo($"Expected calls is set to: {expectedCalls}.");
			}
			catch (Exception e)
			{
				//Logger.LogWarn("String in config file is not in valid format. Default values for expected calls will be set.", e);
				expectedCalls = 3;
			}
		}

		#region INotifySubscriberContract
		public async Task<string> GetSubscriberName()
		{
			return subscriberName;
		}

		public async Task Notify(IPublishableMessage message, string publisherName)
		{
			if (message is EmailToOutageMessage emailMessage)
			{
				if (emailMessage.Gid == 0)
				{
					//Logger.LogError("Invalid email received.");
					return;
				}

				//Logger.LogInfo($"Received call from Energy Consumer with GID: 0x{emailMessage.Gid:X16}.");

				if (!modelResourcesDesc.GetModelCodeFromId(emailMessage.Gid).Equals(ModelCode.ENERGYCONSUMER))
				{
					//Logger.LogWarn("Received GID is not id of energy consumer.");
				}
				else if (await outageModelReadAccessClient.GetElementById(emailMessage.Gid) == null/*!outageModel.TopologyModel.OutageTopology.ContainsKey(emailMessage.Gid) && outageModel.TopologyModel.FirstNode != emailMessage.Gid*/)
				{
					//Logger.LogWarn("Received GID is not part of topology");
				}
				else
				{
					if (!timer.Enabled)
					{
						timer.Start();
					}

					await Calls.SetAsync(emailMessage.Gid, emailMessage.Gid);
					//Logger.LogInfo($"Current number of calls is: {Calls.Count}.");
					
					if ((await Calls.GetCountAsync()) >= expectedCalls)
					{
						await trackingAlgorithm.Start((await Calls.GetDataCopyAsync()).Keys.ToList());

						await Calls.ClearAsync();
						timer.Stop();
					}
				}
			}
		}
		#endregion


		private void TimerElapsedMethod(object sender, ElapsedEventArgs e)
		{
			if ((Calls.GetCountAsync().Result) < expectedCalls)
			{
				//Logger.LogInfo($"Timer elapsed (timer interval is {timerInterval}) and there is no enough calls to start tracing algorithm.");
			}
			else
			{
				trackingAlgorithm.Start((Calls.GetDataCopyAsync().Result).Keys.ToList()).Wait();
			}

			Calls.ClearAsync().Wait();
		}

	}
}
