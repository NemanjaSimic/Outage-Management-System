using Outage.Common;

namespace Outage.SCADA.SCADACommon
{
    public class CommandValue
    {
        public ushort Address { get; set; }
        public int Value { get; set; }
        public CommandOriginType CommandOrigin { get; set; }
    }
}
