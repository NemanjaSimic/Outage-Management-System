using OMS.Common.Cloud;

namespace Common.CE.Interfaces
{
    public interface ISCADACommanding
    {
        void SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOrigin);

        void SendDiscreteCommand(long guid, int value, CommandOriginType commandOrigin);
       
    }
}
