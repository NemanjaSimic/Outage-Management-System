using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common
{
    public static class EndpointNames
    {
        //NMS
        public static readonly string NetworkModelGDAEndpoint = "NetworkModelGDAEndpoint";
        public static readonly string NetworkModelTransactionActorServiceHost = "NetworkModelServiceTransactionActorEndpoint";

        //TM
        public static readonly string TransactionCoordinatorEndpoint = "TransactionCoordinatorEndpoint";
        public static readonly string TransactionCoordinatorEnlistmentEndpoint = "TransactionCoordinatorEnlistmentEndpoint";

        //CE
        public static readonly string CalculationEngineTransactionActorEndpoint = "CalculationEngineTransactionActorEndpoint";

        //SCADA
        public static readonly string SCADATransactionActorEndpoint = "SCADATransactionActorEndpoint";
    }
}
