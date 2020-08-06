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
        public const string CallingEndpoint = "CallingEndpoint";
        public const string OutageAccessEndpoint = "OutageAccessEndpoint";
        public const string OutageLifecycleUICommandingEndpoint = "OutageLifecycleUICommandingEndpoint";
        public const string ReportPotentialOutageEndpoint = "ReportPotentialOutageEndpoint";
        public const string OutageSimulatorServiceEndpoint = "OutageSimulatorServiceEndpoint";
        public const string HistoryDBManagerEndpoint = "HistoryDBManagerEndpoint";
        public const string ReportingEndpoint = "ReportingEndpoint";
        public const string OutageManagementServiceModelReadAccessEndpoint = "OutageManagementServiceModelReadAccessEndpoint";
        public const string ReportOutageEndpoint = "ReportOutageEndpoint";
        public const string ResolveOutageEndpoint = "ResolveOutageEndpoint";
        public const string SendLocationIsolationCrewEndpoint = "SendLocationIsolationCrewEndpoint";
        public const string SendRepairCrewEndpoint = "SendRepairCrewEndpoint";
        public const string ValidateResolveConditionsEndpoint = "ValidateResolveConditionsEndpoint";
        public const string TracingAlgorithmEndpoint = "TracingAlgorithmEndpoint";
        public const string OutageManagmenetServiceModelUpdateAccessEndpoint = "OutageManagmenetServiceModelUpdateAccessEndpoint";
        public const string OutageSimulatorEndpoint = "OutageSimulatorEndpoint";
        public const string NotifySubscriberEndpoint = "NotifySubscriberEndpoint";
        public const string IsolateOutageEndpoint = "IsolateOutageEndpoint";
    }
}

