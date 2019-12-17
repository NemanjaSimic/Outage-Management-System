using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
using Outage.Common.ServiceProxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.DistributedTransactionActor
{
    public abstract class TransactionActor : ITransactionActorContract
    {
        private TransactionCoordinatorProxy coordinatorProxy = null;

        public string EndpointName { get; private set; }

        public string ActorName { get; private set; }

        public Delta UpdateDelta { get; private set; }

        public TransactionCoordinatorProxy CoordinatorProxy
        {
            get
            {
                if(coordinatorProxy != null)
                {
                    coordinatorProxy.Abort();
                    coordinatorProxy = null;
                }

                coordinatorProxy = new TransactionCoordinatorProxy(EndpointName);
                coordinatorProxy.Open();

                return coordinatorProxy;
            }
        }

        public TransactionActor(string endpointName, string actorName)
        {
            EndpointName = endpointName;
            ActorName = actorName;
        }

        public virtual bool EnlistUpdateDelta(Delta delta)
        {
            UpdateDelta = delta;
            return true;
        }

        public virtual void Prepare()
        {
            using (CoordinatorProxy)
            {
                CoordinatorProxy.FinishDistributedUpdate(ActorName, true);
            }
        }

        public virtual bool Commit()
        {
            return true;
        }

        public virtual bool Rollback()
        {
            return true;
        }
    }
}
