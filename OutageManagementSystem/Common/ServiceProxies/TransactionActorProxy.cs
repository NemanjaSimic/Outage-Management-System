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
    public class TransactionActorProxy : ClientBase<ITransactionActorContract>, ITransactionActorContract
    {
        public TransactionActorProxy(string endpointName)
            : base(endpointName)
        {
        }

        public bool EnlistUpdateDelta(Delta delta)
        {
            return Channel.EnlistUpdateDelta(delta);
        }

        public void Prepare()
        {
            Channel.Prepare();
        }

        public bool Commit()
        {
            return Channel.Commit();
        }

        public bool Rollback()
        {
            return Channel.Rollback();
        }
    }
}
