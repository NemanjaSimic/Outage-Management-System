using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CE.TopologyBuilderImplementation;
using Common.CeContracts;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;

namespace CE.TopologyBuilderService
{
	/// <summary>
	/// An instance of this class is created for each service instance by the Service Fabric runtime.
	/// </summary>
	internal sealed class TopologyBuilderService : StatelessService
	{
		private readonly string baseLogString;
		private readonly TopologyBuilder topologyBuilder;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		public TopologyBuilderService(StatelessServiceContext context)
			: base(context)
		{
			this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);

			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

			try
			{
				this.topologyBuilder = new TopologyBuilder();

				string infoMessage = $"{baseLogString} Ctor => Contract providers initialized.";
				Logger.LogInformation(infoMessage);
			}
			catch (Exception e)
			{
				string errorMessage = $"{baseLogString} Ctor => exception {e.Message}";
				Logger.LogError(errorMessage, e);
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
					 return new WcfCommunicationListener<ITopologyBuilderContract>(context,
																			   this.topologyBuilder,
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.CeTopologyBuilderServiceEndpoint);
				}, EndpointNames.CeTopologyBuilderServiceEndpoint)
			};
		}
	}
}
