using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common
{
    public static class ServiceHostNames
    {
        //NMS
        public static readonly string NetworkModelService = "NetworkModelService";
        public static readonly string NetworkModelGDAServiceHost = "NetworkModelGDAServiceHost";
        public static readonly string NetworkModelTransactionActorServiceHost = "NetworkModelTransactionActorServiceHost";

        //CE
        public static readonly string CalculationEngineService = "CalculationEngineService";
        public static readonly string CalculationEngineTopologyIntegrityUpdateServiceHost = "CalculationEngineTopologyIntegrityUpdateServiceHost";
        public static readonly string CalculationEngineTransactionActorServiceHost = "CalculationEngineTransactionActorServiceHost";

        //SCADA
        public static readonly string SCADAService = "SCADAService";
        public static readonly string SCADACommandingServiceHost = "SCADACommandingServiceHost";
        public static readonly string SCADATransactionActorServiceHost = "SCADATransactionActorServiceHost";

        //WebUI
        public static readonly string WebUIService = "WebUIService";

        //TM
        public static readonly string TransactionManagerService = "TransactionManagerService";
        public static readonly string TransactionCoordinatorServiceHost = "TransactionCoordinatorServiceHost";

        //OMS
        public static readonly string OutageManagementService = "OutageManagementService";
    }
}
