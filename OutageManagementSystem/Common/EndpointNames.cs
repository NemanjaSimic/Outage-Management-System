namespace Outage.Common
{
    public static class EndpointNames
    {
        //TM
        public static readonly string TransactionCoordinatorEndpoint = "TransactionCoordinatorEndpoint";
        public static readonly string TransactionEnlistmentEndpoint = "TransactionEnlistmentEndpoint";
        
        //NMS
        public static readonly string NetworkModelGDAEndpoint = "NetworkModelGDAEndpoint";
        public static readonly string NetworkModelTransactionActorServiceHost = "NetworkModelServiceTransactionActorEndpoint";

        //CE
        public static readonly string CalculationEngineTransactionActorEndpoint = "CalculationEngineTransactionActorEndpoint";
        public static readonly string CalculationEngineModelUpdateNotifierEndpoint = "CalculationEngineModelUpdateNotifierEndpoint";
        public static readonly string TopologyServiceEndpoint = "TopologyServiceEndpoint";

        //SCADA
        public static readonly string SCADATransactionActorEndpoint = "SCADATransactionActorEndpoint";
        public static readonly string SCADAModelUpdateNotifierEndpoint = "SCADAModelUpdateNotifierEndpoint";
    }
}
