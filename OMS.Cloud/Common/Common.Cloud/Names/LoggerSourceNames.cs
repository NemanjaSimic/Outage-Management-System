namespace OMS.Common.Cloud.Names
{
    public static class LoggerSourceNames
    {
        //NMS
        public const string NmsGdaService = "NMS.GdaService";
        public const string NmsTestClientUI = "NMS.TestClientUI";
        public const string NmsModelLapsApp = "NMS.ModelLapsApp";
        public const string NmsCimProfilCreator = "NMS.CimProfilCreator";

        //SCADA
        public const string ScadaCommandingService = "SCADA.CommandingService";
        public const string ScadaModelProviderService = "SCADA.ModelProviderService";
        public const string ScadaFunctionExecutorService = "SCADA.FunctionExecutorService";
        public const string ScadaAcquisitionService = "SCADA.AcquisitionService";

        //TMS
        public const string TransactionManagerService = "TMS.TransactionManagerService";

        //PUB_SUB
        public const string PubSubService = "PubSubService";

        //CE
        public const string CeLoadFlowService = "CE.LoadFlowService";
        public const string CeModelProviderService = "CE.ModelProviderService";
        public const string CeMeasurementProviderService = "CE.MeasurementProviderService";
        public const string CeTopologyProviderService = "CE.TopologyProviderService";
        public const string CeTopologyBuilderService = "CE.TopologyBuilderService";
    }
}
