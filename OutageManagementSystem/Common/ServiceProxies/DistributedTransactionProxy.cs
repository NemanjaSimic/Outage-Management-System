using Outage.Common.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies
{
    public class DistributedTransactionProxy : ClientBase<IDistributedTransactionContract>, IDistributedTransactionContract
    {
        public DistributedTransactionProxy(string endpointName)
            : base(endpointName)
        {
        }
    }
}
