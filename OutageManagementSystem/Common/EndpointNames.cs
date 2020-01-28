namespace Outage.Common
{
    public static class EndpointNames
    {
        //TM - 0
        public static readonly string TransactionCoordinatorEndpoint = "TransactionCoordinatorEndpoint";
        public static readonly string TransactionEnlistmentEndpoint = "TransactionEnlistmentEndpoint";

        //SCADA - 1
        public static readonly string SCADACommandService = "SCADACommandService";
        public static readonly string SCADATransactionActorEndpoint = "SCADATransactionActorEndpoint";
        public static readonly string SCADAModelUpdateNotifierEndpoint = "SCADAModelUpdateNotifierEndpoint";
        public static readonly string SCADAAnalogRecieverEndpoint = "SCADAnalogRecieverEndpoint";

        //NMS - 2
        public static readonly string NetworkModelGDAEndpoint = "NetworkModelGDAEndpoint";
        public static readonly string NetworkModelTransactionActorEndpoint = "NetworkModelServiceTransactionActorEndpoint";

        //CE - 3
        public static readonly string CalculationEngineTransactionActorEndpoint = "CalculationEngineTransactionActorEndpoint";
        public static readonly string CalculationEngineModelUpdateNotifierEndpoint = "CalculationEngineModelUpdateNotifierEndpoint";
        public static readonly string TopologyServiceEndpoint = "TopologyServiceEndpoint";

        //PUBSUB - 4
        public static readonly string PublisherEndpoint = "PublisherEndpoint";
        public static readonly string SubscriberEndpoint = "SubscriberEndpoint";

        //OMS - 5
        public static readonly string CallingEndpoint = "CallingEndpoint";
        public static readonly string OutageServiceEndpoint = "OutageServiceEndpoint";

    }
}
