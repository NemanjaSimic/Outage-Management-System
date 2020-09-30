namespace Common.OMS
{
    public static class ReliableDictionaryNames
    {
        //OMS.CallTrackingService
        public const string CallsDictionary = "CallsDictionary";

        //OMS.HistoryDBManagerService
        public const string OpenedSwitches = "OpenedSwitches";
        public const string UnenergizedConsumers = "UnenergizedConsumers";
        public const string HistoryModelChanges = "HistoryModelChanges";
        public const string ActiveOutages = "ActiveOutages";

        //OMS.ModelProviderService
        public const string OutageTopologyModel = "OutageTopologyModel";
        public const string CommandedElements = "CommandedElements";
        public const string OptimumIsolationPoints = "OptimumIsolatioPoints";

        //OMS.OutageLifecycleService
        public const string StartedIsolationAlgorithms = "StartedIsolationAlgorithms";
        public const string MonitoredHeadBreakerMeasurements = "MonitoredHeadBreakerMeasurements";
        public const string RecloserOutageMap = "RecloserOutageMap";
        public const string ElementsToBeIgnoredInReportPotentialOutage = "ElementsToBeIgnoredInReportPotentialOutage";

        //OMS.OutageSimulatorService
        public const string SimulatedOutages = "SimulatedOutages";
        public const string MonitoredIsolationPoints = "MonitoredIsolationPoints";
        public const string CommandedValues = "CommandedValues";
    }
}
