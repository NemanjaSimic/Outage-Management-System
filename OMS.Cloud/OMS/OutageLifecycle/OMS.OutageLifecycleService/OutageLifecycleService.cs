using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.OmsContracts.OutageLifecycle;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Names;
using OMS.OutageLifecycleServiceImplementation;

namespace OMS.OutageLifecycleService
{
	/// <summary>
	/// An instance of this class is created for each service instance by the Service Fabric runtime.
	/// </summary>
	internal sealed class OutageLifecycleService : StatelessService
	{
		public OutageLifecycleService(StatelessServiceContext context)
			: base(context)
		{ }

		/// <summary>
		/// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
		/// </summary>
		/// <returns>A collection of listeners.</returns>
		protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
		{
			return new List<ServiceInstanceListener>
			{
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<IIsolateOutageContract>(context,
																				new IsolateOutageService(),
																				WcfUtility.CreateTcpListenerBinding(),
																				EndpointNames.IsolateOutageEndpoint);
				}, EndpointNames.IsolateOutageEndpoint),
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<IReportOutageContract>(context,
																			   new ReportOutageService(),
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.ReportOutageEndpoint);
				}, EndpointNames.ReportOutageEndpoint),
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<IResolveOutageContract>(context,
																			   new ResolveOutageService(),
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.ResolveOutageEndpoint);
				}, EndpointNames.ResolveOutageEndpoint),
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<ISendLocationIsolationCrewContract>(context,
																			   new SendLocationIsolationCrewService(),
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.SendLocationIsolationCrewEndpoint);
				}, EndpointNames.SendLocationIsolationCrewEndpoint),
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<ISendRepairCrewContract>(context,
																			   new SendRepairCrewService(),
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.SendRepairCrewEndpoint);
				}, EndpointNames.SendRepairCrewEndpoint),
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<IValidateResolveConditionsContract>(context,
																			   new ValidateResolveConditionsService(),
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.ValidateResolveConditionsEndpoint);
				}, EndpointNames.ValidateResolveConditionsEndpoint),
			};
		}

	}
}
