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
    public interface ITransactionCoordinatorContract
    {
        [OperationContract]
        void StartDistributedUpdate(Delta delta, string actorName);
        [OperationContract]
        void FinishDistributedUpdate(string actorName, bool success);
    }
}
