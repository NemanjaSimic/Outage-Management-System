using System;
using System.ServiceModel;

namespace Outage.SCADA.SCADA_Common
{
    [Obsolete]
    [ServiceContract]
    public interface ICommandService
    {
        [OperationContract]
        void RecvCommand(long gid, PointType pointType, object value);
    }
}