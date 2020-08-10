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
using OMS.Common.PubSubContracts;
using OMS.OutageLifecycleServiceImplementation;
using OMS.OutageLifecycleServiceImplementation.ScadaSub;
using OutageDatabase.Repository;

namespace OMS.OutageLifecycleService
{
	/// <summary>
	/// An instance of this class is created for each service instance by the Service Fabric runtime.
	/// </summary>
	internal sealed class OutageLifecycleService : StatelessService
	{
		
		private ReportOutageService reportOutageService;
		private IsolateOutageService isolateOutageService;
		private ResolveOutageService resolveOutageService;
		private SendLocationIsolationCrewService sendLocationIsolationCrewService;
		private SendRepairCrewService sendRepairCrewService;
		private ValidateResolveConditionsService validateResolveConditionsService;
		private ScadaSubscriber scadaSubscriber;

		public OutageLifecycleService(StatelessServiceContext context)
			: base(context)
		{
			scadaSubscriber = new ScadaSubscriber(MicroserviceNames.OmsOutageLifecycleService);
			reportOutageService = new ReportOutageService();
			isolateOutageService = new IsolateOutageService(scadaSubscriber);
			resolveOutageService = new ResolveOutageService();
			sendLocationIsolationCrewService = new SendLocationIsolationCrewService();
			sendRepairCrewService = new SendRepairCrewService();
			validateResolveConditionsService = new ValidateResolveConditionsService();
		}

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
																				isolateOutageService,
																				WcfUtility.CreateTcpListenerBinding(),
																				EndpointNames.OmsIsolateOutageEndpoint);
				}, EndpointNames.OmsIsolateOutageEndpoint),
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<IReportOutageContract>(context,
																			   reportOutageService,
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.OmsReportOutageEndpoint);
				}, EndpointNames.OmsReportOutageEndpoint),
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<IResolveOutageContract>(context,
																			   resolveOutageService,
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.OmsResolveOutageEndpoint);
				}, EndpointNames.OmsResolveOutageEndpoint),
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<ISendLocationIsolationCrewContract>(context,
																			   sendLocationIsolationCrewService,
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.OmsSendLocationIsolationCrewEndpoint);
				}, EndpointNames.OmsSendLocationIsolationCrewEndpoint),
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<ISendRepairCrewContract>(context,
																			   sendRepairCrewService,
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.OmsSendRepairCrewEndpoint);
				}, EndpointNames.OmsSendRepairCrewEndpoint),
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<IValidateResolveConditionsContract>(context,
																			   validateResolveConditionsService,
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.OmsValidateResolveConditionsEndpoint);
				}, EndpointNames.OmsValidateResolveConditionsEndpoint),
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<INotifySubscriberContract>(context,
																			   scadaSubscriber,
																			   WcfUtility.CreateTcpListenerBinding(),
																			   EndpointNames.PubSubNotifySubscriberEndpoint);
				}, EndpointNames.PubSubNotifySubscriberEndpoint),
			};
		}

	}
}
