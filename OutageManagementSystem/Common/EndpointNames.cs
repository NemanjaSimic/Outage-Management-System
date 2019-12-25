using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //SCADA
        public static readonly string SCADATransactionActorEndpoint = "SCADATransactionActorEndpoint";
        public static readonly string SCADAModelUpdateNotifierEndpoint = "SCADAModelUpdateNotifierEndpoint";

        //PUBSUB
        public static readonly string PublisherEndpoint = "PublisherEndpoint";
        public static readonly string SubscriberEndpoint = "SubscriberEndpoint";
    }
}
