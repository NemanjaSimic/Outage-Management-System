using System.Fabric.Health;

namespace OMS.Common.Cloud.Names
{
    public static class EndpointNames
    {
        //TM - 0
        public const string TmsTransactionCoordinatorEndpoint = "TmsTransactionCoordinatorEndpoint";
        public const string TmsTransactionEnlistmentEndpoint = "TmsTransactionEnlistmentEndpoint";
        public const string TmsNotifyNetworkModelUpdateEndpoint = "TmsNotifyNetworkModelUpdateEndpoint";
        public const string TmsTransactionActorEndpoint = "TmsTransactionActorEndpoint";

        //SCADA - 1
        public const string ScadaCommandingEndpoint = "ScadaCommandingEndpoint";
        public const string ScadaIntegrityUpdateEndpoint = "ScadaIntegrityUpdateEndpoint";
        public const string ScadaModelReadAccessEndpoint = "ScadaModelReadAccessEndpoint";
        public const string ScadaModelUpdateAccessEndpoint = "ScadaModelUpdateAccessEndpoint";
        public const string ScadaReadCommandEnqueuerEndpoint = "ScadaReadCommandEnqueuerEndpoint";
        public const string ScadaWriteCommandEnqueuerEndpoint = "ScadaWriteCommandEnqueuerEndpoint";
        public const string ScadaModelUpdateCommandEnqueueurEndpoint = "ScadaModelUpdateCommandEnqueueurEndpoint";

        //NMS - 2
        public const string NmsGdaEndpoint = "NmsGdaEndpoint";

        //CE - 3
        public const string CeTopologyServiceEndpoint = "CeTopologyServiceEndpoint";
        public const string CeTopologyProviderServiceEndpoint = "CeTopologyProviderServiceEndpoint";
        public const string CeTopologyBuilderServiceEndpoint = "CeTopologyBuilderServiceEndpoint";
        public const string CeTopologyConverterServiceEndpoint = "CeTopologyConverterServiceEndpoint";
        public const string CeTopologyOMSServiceEndpoint = "CeTopologyOMSServiceEndpoint";
        public const string CeSwitchStatusCommandingEndpoint = "CeSwitchStatusCommandingEndpoint";
        public const string CeMeasurementProviderEndpoint = "CeMeasurementProviderEndpoint";
        public const string CeMeasurementMapEndpoint = "CeMeasurementMapEndpoint";
        public const string CeModelProviderServiceEndpoint = "CeModelProviderServiceEndpoint";
        public const string CeLoadFlowServiceEndpoint = "CeLoadFlowServiceEndpoint";

        //PUBSUB - 4
        public const string PubSubPublisherEndpoint = "PubSubPublisherEndpoint";
        public const string PubSubRegisterSubscriberEndpoint = "PubSubRegisterSubscriberEndpoint";
        public const string PubSubNotifySubscriberEndpoint = "PubSubNotifySubscriberEndpoint";

        //OMS - 5
        public const string OmsCallingEndpoint = "OmsCallingEndpoint";

        public const string OmsReportingEndpoint = "OmsReportingEndpoint";
        public const string OmsHistoryDBManagerEndpoint = "OmsHistoryDBManagerEndpoint";
        
        public const string OmsOutageAccessEndpoint = "OmsOutageAccessEndpoint";
        public const string OmsEquipmentAccessEndpoint = "OmsEquipmentAccessEndpoint";
        public const string OmsConsumerAccessEndpoint = "OmsConsumerAccessEndpoint";
        
        public const string OmsModelReadAccessEndpoint = "OmsModelReadAccessEndpoint";
        public const string OmsModelUpdateAccessEndpoint = "OmsModelUpdateAccessEndpoint";

        public const string OmsTracingAlgorithmEndpoint = "OmsTracingAlgorithmEndpoint";

        public const string OmsPotentialOutageReportingEndpoint = "OmsPotentialOutageReportingEndpoint";
        public const string OmsCrewSendingEndpoint = "OmsCrewSendingEndpoint";
        public const string OmsOutageIsolationEndpoint = "OmsOutageIsolationEndpoint";
        public const string OmsOutageResolutionEndpoint = "OmsOutageResolutionEndpoint";

        public const string OmsOutageSimulatorEndpoint = "OmsOutageSimulatorEndpoint";
        public const string OmsOutageSimulatorUIEndpoint = "OmsOutageSimulatorUIEndpoint";

        //WEB ADAPTER
        public const string WebAdapterEndpoint = "WebAdapterEndpoint";
    }
}

