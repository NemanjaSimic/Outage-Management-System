using System.Threading;

namespace OutageManagementService.OutageIsolationAlgorithm
{
    public class OutageIsolationAlgorithmParameters
    {
        public long HeadBreakerId { get; set; }
        public long RecloserId { get; set; }
        public AutoResetEvent AutoResetEvent { get; set; }
        public OutageIsolationAlgorithmParameters(long headBreakerId, long recloserId, AutoResetEvent autoResetEvent)
        {
            HeadBreakerId = headBreakerId;
            RecloserId = recloserId;
            AutoResetEvent = autoResetEvent;
        }
    }
}
