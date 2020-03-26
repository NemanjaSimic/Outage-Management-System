using Outage.Common;

namespace OMS.Cloud.SCADA.Common
{
    public class CommandValue
    {
        public ushort Address { get; set; }
        public int Value { get; set; }
        public CommandOriginType CommandOrigin { get; set; }
    }
}
