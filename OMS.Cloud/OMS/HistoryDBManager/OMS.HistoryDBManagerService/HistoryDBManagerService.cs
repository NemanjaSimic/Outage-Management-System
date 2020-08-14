using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Common.OmsContracts.HistoryDBManager;
using Common.OmsContracts.ModelAccess;
using Common.OmsContracts.Report;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OMS.Common.Cloud.Names;
using OMS.HistoryDBManagerServiceImplementation;
using OMS.HistoryDBManagerServiceImplementation.ModelAccess;

namespace OMS.HistoryDBManagerService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class HistoryDBManagerService : StatefulService
    {
        private HistoryDBManager historyDBManager;
        private ReportService reportService;
        private OutageModelAccess outageModelAccess;
        private ConsumerAccess consumerAccess;
        private EquipmentAccess equipmentAccess;
        
        public HistoryDBManagerService(StatefulServiceContext context)
            : base(context)
        {
            historyDBManager = new HistoryDBManager(this.StateManager);
            reportService = new ReportService();
            outageModelAccess = new OutageModelAccess();
            consumerAccess = new ConsumerAccess();
            equipmentAccess = new EquipmentAccess();
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
                    return new WcfCommunicationListener<IHistoryDBManagerContract>(context,
                                                                                    historyDBManager,
                                                                                    WcfUtility.CreateTcpListenerBinding(),
                                                                                    EndpointNames.OmsHistoryDBManagerEndpoint);
                }, EndpointNames.OmsHistoryDBManagerEndpoint),
                new ServiceReplicaListener(context =>
				{
                        return new WcfCommunicationListener<IReportingContract>(context,
                                                                                reportService,
                                                                                WcfUtility.CreateTcpClientBinding(),
                                                                                EndpointNames.OmsReportingEndpoint);
			    }, EndpointNames.OmsReportingEndpoint),
                new ServiceReplicaListener(context =>
                {
                        return new WcfCommunicationListener<IOutageAccessContract>(context,
                                                                                outageModelAccess,
                                                                                WcfUtility.CreateTcpClientBinding(),
                                                                                EndpointNames.OmsOutageAccessEndpoint);
                }, EndpointNames.OmsOutageAccessEndpoint),
                new ServiceReplicaListener(context =>
                {
                        return new WcfCommunicationListener<IConsumerAccessContract>(context,
                                                                                consumerAccess,
                                                                                WcfUtility.CreateTcpClientBinding(),
                                                                                EndpointNames.OmsConsumerAccessEndpoint);
                }, EndpointNames.OmsConsumerAccessEndpoint),
                new ServiceReplicaListener(context =>
                {
                        return new WcfCommunicationListener<IEquipmentAccessContract>(context,
                                                                                equipmentAccess,
                                                                                WcfUtility.CreateTcpClientBinding(),
                                                                                EndpointNames.OmsEquipmentAccessEndpoint);
                }, EndpointNames.OmsEquipmentAccessEndpoint),
            };
            
        }

    }
}
