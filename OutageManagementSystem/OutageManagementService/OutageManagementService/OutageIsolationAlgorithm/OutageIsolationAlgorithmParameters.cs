using System.Threading;

namespace OutageManagementService.OutageIsolationAlgorithm
{
    public class OutageIsolationAlgorithmParameters
    {
        public long HeadBreakerMeasurementId { get; set; }
        public long RecloserMeasurementId { get; set; }
        public AutoResetEvent AutoResetEvent { get; set; }
        public OutageIsolationAlgorithmParameters(long headBreakerMeasurementId, long recloserMeasurementId, AutoResetEvent autoResetEvent)
        {
            HeadBreakerMeasurementId = headBreakerMeasurementId;
            RecloserMeasurementId = recloserMeasurementId;
            AutoResetEvent = autoResetEvent;
        }
    }
}
