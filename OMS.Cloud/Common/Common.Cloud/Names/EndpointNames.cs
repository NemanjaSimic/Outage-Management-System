﻿using System.Fabric.Health;

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
        public const string OmsOutageAccessEndpoint = "OmsOutageAccessEndpoint";
        public const string OmsEquipmentAccessEndpoint = "OmsEquipmentAccessEndpoint";
        public const string OmsConsumerAccessEndpoint = "OmsConsumerAccessEndpoint";
        public const string OmsOutageLifecycleUICommandingEndpoint = "OmsOutageLifecycleUICommandingEndpoint";
        public const string OmsReportPotentialOutageEndpoint = "OmsReportPotentialOutageEndpoint";
        public const string OmsOutageSimulatorServiceEndpoint = "OmsOutageSimulatorServiceEndpoint";
        public const string OmsHistoryDBManagerEndpoint = "OmsHistoryDBManagerEndpoint";
        public const string OmsReportingEndpoint = "OmsReportingEndpoint";
        public const string OmsOutageManagementServiceModelReadAccessEndpoint = "OmsOutageManagementServiceModelReadAccessEndpoint";
        public const string OmsReportOutageEndpoint = "OmsReportOutageEndpoint";
        public const string OmsResolveOutageEndpoint = "OmsResolveOutageEndpoint";
        public const string OmsSendLocationIsolationCrewEndpoint = "OmsSendLocationIsolationCrewEndpoint";
        public const string OmsSendRepairCrewEndpoint = "OmsSendRepairCrewEndpoint";
        public const string OmsValidateResolveConditionsEndpoint = "OmsValidateResolveConditionsEndpoint";
        public const string OmsTracingAlgorithmEndpoint = "OmsTracingAlgorithmEndpoint";
        public const string OmsOutageManagmenetServiceModelUpdateAccessEndpoint = "OmsOutageManagmenetServiceModelUpdateAccessEndpoint";
        public const string OmsOutageSimulatorEndpoint = "OmsOutageSimulatorEndpoint";
        public const string OmsIsolateOutageEndpoint = "OmsIsolateOutageEndpoint";

        public const string WebAdapterEndpoint = "WebAdapterEndpoint";
    }
}

