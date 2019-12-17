using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts
{
    [ServiceContract]
    public interface ITransactionActorContract
    {
        [OperationContract]
        bool EnlistUpdateDelta(Delta delta);
        [OperationContract]
        void Prepare();
        [OperationContract]
        bool Commit();
        [OperationContract]
        bool Rollback();
    }
}
