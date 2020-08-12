using OMS.Common.Cloud;

namespace Common.CeContracts
{
    public interface ISCADACommanding
    {
        void SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOrigin);

        void SendDiscreteCommand(long guid, int value, CommandOriginType commandOrigin);
       
    }
}
