namespace OMS.Common.Cloud.Names
{
    public static class EndpointNames
    {
        //TM - 0
        public const string TransactionCoordinatorEndpoint = "TransactionCoordinatorEndpoint";
        public const string TransactionEnlistmentEndpoint = "TransactionEnlistmentEndpoint";

        //SCADA - 1
        public const string ScadaCommandService = "ScadaCommandService";
        public const string ScadaTransactionActorEndpoint = "ScadaTransactionActorEndpoint";
        public const string ScadaModelUpdateNotifierEndpoint = "ScadaModelUpdateNotifierEndpoint";
        public const string ScadaIntegrityUpdateEndpoint = "ScadaIntegrityUpdateEndpoint";
        public const string ScadaModelReadAccessEndpoint = "ScadaModelReadAccessEndpoint";
        public const string ScadaModelUpdateAccessEndpoint = "ScadaModelUpdateAccessEndpoint";
        public const string ScadaReadCommandEnqueuerEndpoint = "ScadaReadCommandEnqueuerEndpoint";
        public const string ScadaWriteCommandEnqueuerEndpoint = "ScadaWriteCommandEnqueuerEndpoint";
        public const string ScadaModelUpdateCommandEnqueueurEndpoint = "ScadaModelUpdateCommandEnqueueurEndpoint";

        //NMS - 2
        public const string NetworkModelGDAEndpoint = "NetworkModelGDAEndpoint";
        public const string NetworkModelTransactionActorEndpoint = "NetworkModelServiceTransactionActorEndpoint";

        //CE - 3
        public const string CalculationEngineTransactionActorEndpoint = "CalculationEngineTransactionActorEndpoint";
        public const string CalculationEngineModelUpdateNotifierEndpoint = "CalculationEngineModelUpdateNotifierEndpoint";
        public const string TopologyServiceEndpoint = "TopologyServiceEndpoint";
        public const string TopologyOMSServiceEndpoint = "TopologyOMSServiceEndpoint";
        public const string SwitchStatusCommandingEndpoint = "SwitchStatusCommandingEndpoint";
        public const string MeasurementMapEndpoint = "MeasurementMapEndpoint";

        //PUBSUB - 4
        public const string PublisherEndpoint = "PublisherEndpoint";
        public const string SubscriberEndpoint = "SubscriberEndpoint";
        public const string NotifySubscriberEndpoint = "NotifySubscriberEndpoint";

        //OMS - 5
        public const string CallingEndpoint = "CallingEndpoint";
        public const string OutageAccessEndpoint = "OutageAccessEndpoint";
        public const string OutageTransactionActorEndpoint = "OutageTransactionActorEndpoint";
        public const string OutageModelUpdateNotifierEndpoint = "OutageModelUpdateNotifierEndpoint";
        public const string OutageLifecycleUICommandingEndpoint = "OutageLifecycleUICommandingEndpoint";
        public const string ReportPotentialOutageEndpoint = "ReportPotentialOutageEndpoint";
        public const string OutageManagementServiceModelReadAccessEndpoint = "OutageManagementServiceModelReadAccessEndpoint";
        public const string TracingAlgorithmEndpoint = "TracingAlgorithmEndpoint";
        public const string OutageSimulatorServiceEndpoint = "OutageSimulatorServiceEndpoint";
        public const string HistoryDBManagerEndpoint = "HistoryDBManagerEndpoint";

    }
}

