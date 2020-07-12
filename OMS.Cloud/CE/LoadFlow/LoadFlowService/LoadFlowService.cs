using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.CeContracts.LoadFlow;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using Topology;

namespace LoadFlowService
{
	/// <summary>
	/// An instance of this class is created for each service instance by the Service Fabric runtime.
	/// </summary>
	internal sealed class LoadFlowService : StatelessService
	{
		private readonly string baseLogString;
		private readonly LoadFlow loadFlowEngine;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public LoadFlowService(StatelessServiceContext context)
			: base(context)
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

			try
			{
				this.loadFlowEngine = new LoadFlow();

				string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
				Logger.LogInformation(infoMessage);
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[LoadFlowService | Information] {infoMessage}");
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} Ctor => exception {e.Message}";
				Logger.LogError(errorMessage, e);
				ServiceEventSource.Current.ServiceMessage(this.Context, $"[LoadFlowService | Error] {errorMessage}");
			}
		}

		/// <summary>
		/// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
		/// </summary>
		/// <returns>A collection of listeners.</returns>
		protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
		{
			return new List<ServiceInstanceListener>(1)
			{
				new ServiceInstanceListener(context =>
				{
					 return new WcfCommunicationListener<ILoadFlowService>(context,
																			   this.loadFlowEngine,
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.LoadFlowServiceEndpoint);
				}, EndpointNames.LoadFlowServiceEndpoint)
			};
		}

		/// <summary>
		/// This is the main entry point for your service instance.
		/// </summary>
		/// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
		protected override async Task RunAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();


			long iterations = 0;

			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();

				ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

				await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
			}
		}
	}
}
