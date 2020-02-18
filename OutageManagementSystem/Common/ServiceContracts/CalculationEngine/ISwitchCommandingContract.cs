using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts.CalculationEngine
{
    [ServiceContract]
    public interface ISwitchStatusCommandingContract
    {
        [OperationContract]
        void SendCommand(long guid, int value);

        //TOOD:
        //[OperationContract]
        //void SendOpenCommand(long guid);

        //TOOD:
        //[OperationContract]
        //void SendCloseCommand(long guid);
    }
}
