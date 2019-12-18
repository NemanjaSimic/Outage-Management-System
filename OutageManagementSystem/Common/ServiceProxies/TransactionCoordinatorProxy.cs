using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies
{
    public class TransactionCoordinatorProxy : ClientBase<ITransactionCoordinatorContract>, ITransactionCoordinatorContract
    {
        public TransactionCoordinatorProxy(string endpointName)
            : base(endpointName)
        {
        }
        public void StartDistributedUpdate()
        {
            Channel.StartDistributedUpdate();
        }

        public void FinishDistributedUpdate(string actorName, bool success)
        {
            Channel.FinishDistributedUpdate(actorName, success);
        }
    }
}
