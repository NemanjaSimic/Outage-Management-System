using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts;
using OMS.Common.WcfClient.PubSub;
using OMS.CallTrackingImplementation;
using Microsoft.ServiceFabric.Data;
using Common.OMS;
using Microsoft.ServiceFabric.Data.Collections;

namespace OMS.CallTrackingService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class CallTrackingService : StatefulService
	{
		private readonly string baseLogString;
		private readonly CallTracker callTracker;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public CallTrackingService(StatefulServiceContext context)
			: base(context)
		{
			this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);

			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

			try
			{
				callTracker = new CallTracker(this.StateManager, MicroserviceNames.OmsCallTrackingService);

				string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
				Logger.LogInformation(infoMessage);
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} Ctor => Exception caught: {e.Message}.";
				Logger.LogError(errorMessage, e);
			}
		}

		/// <summary>
		/// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
		/// </summary>
		/// <remarks>
		/// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
		/// </remarks>
		/// <returns>A collection of listeners.</returns>
		protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
		{
			return new List<ServiceReplicaListener>()
			{
				new ServiceReplicaListener(context =>
				{
					return new WcfCommunicationListener<INotifySubscriberContract>(context,
																				   this.callTracker,
																				   WcfUtility.CreateTcpListenerBinding(),
																				   EndpointNames.PubSubNotifySubscriberEndpoint);
				}, EndpointNames.PubSubNotifySubscriberEndpoint)
			};
		}

		/// <summary>
		/// This is the main entry point for your service replica.
		/// This method executes when this replica of your service becomes primary and has write status.
		/// </summary>
		/// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
		protected override async Task RunAsync(CancellationToken cancellationToken)
		{
			try
			{
				InitializeReliableCollections();
				string debugMessage = $"{baseLogString} RunAsync => ReliableDictionaries initialized.";
				Logger.LogDebug(debugMessage);

				var registerSubscriberClient = RegisterSubscriberClient.CreateClient();
				await registerSubscriberClient.SubscribeToTopic(Topic.OUTAGE_EMAIL, MicroserviceNames.OmsCallTrackingService);
			}
			catch (Exception e)
			{
				Logger.LogError($"{baseLogString} RunAsync => Exception: {e.Message}");
			}
		}

		private void InitializeReliableCollections()
		{
			Task[] tasks = new Task[]
			{
				Task.Run(async() =>
				{
					using (ITransaction tx = this.StateManager.CreateTransaction())
					{
						var result = await StateManager.TryGetAsync<IReliableDictionary<long, long>>(ReliableDictionaryNames.CallsDictionary);
						if(result.HasValue)
						{
							var gidToPointItemMap = result.Value;
							await gidToPointItemMap.ClearAsync();
							await tx.CommitAsync();
						}
						else
						{
							await StateManager.GetOrAddAsync<IReliableDictionary<long, long>>(tx, ReliableDictionaryNames.CallsDictionary);
							await tx.CommitAsync();
						}
					}
				}),
			};

			Task.WaitAll(tasks);
		}
	}
}
