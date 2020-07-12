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
        public const string TopologyServiceEndpoint = "TopologyServiceEndpoint";
        public const string TopologyOMSServiceEndpoint = "TopologyOMSServiceEndpoint";
        public const string SwitchStatusCommandingEndpoint = "SwitchStatusCommandingEndpoint";
        public const string MeasurementMapEndpoint = "MeasurementMapEndpoint";

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
    }
}

