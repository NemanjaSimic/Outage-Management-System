using Outage.Common;

namespace OMS.Common.SCADA
{
    public class CommandValue
    {
        public ushort Address { get; set; }
        public int Value { get; set; }
        public CommandOriginType CommandOrigin { get; set; }
    }
}
