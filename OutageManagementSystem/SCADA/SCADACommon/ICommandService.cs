using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
