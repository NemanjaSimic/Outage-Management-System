using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.DistributedTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.TransactionManagerService
{
    public class DistributedTransaction : ITransactionCoordinatorContract, ITransactionEnlistmentContract
    {
        //TODO: get from config
        private static readonly Dictionary<string, string> distributedTransactionActors = new Dictionary<string, string>()
        {
            {   ServiceNames.NetworkModelService,       EndpointNames.NetworkModelTransactionActorServiceHost       },
            {   ServiceNames.SCADAService,              EndpointNames.SCADATransactionActorEndpoint                 },
            {   ServiceNames.CalculationEngineService,  EndpointNames.CalculationEngineTransactionActorEndpoint     },
        };

        private static Dictionary<string, bool> transactionLedger = null;

        private static Dictionary<string, bool> TransactionLedger
        {
            get
            { 
                return transactionLedger ?? (transactionLedger = new Dictionary<string, bool>(distributedTransactionActors.Count));
            }
        }

        private TransactionActorProxy transactionActorProxy = null;

        private TransactionActorProxy GetTransactionActorProxy(string endpoint)
        {
            if (transactionActorProxy != null)
            {
                transactionActorProxy.Abort();
                transactionActorProxy = null;
            }

            transactionActorProxy = new TransactionActorProxy(endpoint);
            transactionActorProxy.Open();

            return transactionActorProxy;
        }

        public void StartDistributedUpdate()
        {
            transactionLedger = new Dictionary<string, bool>(distributedTransactionActors.Count);

            foreach (string actor in distributedTransactionActors.Keys)
            {
                if(!TransactionLedger.ContainsKey(actor))
                {
                    TransactionLedger.Add(actor, false);
                }
            }

            //TODO: start timer...
        }
        
        public void FinishDistributedUpdate(bool success)
        {
            if(success)
            {
                if(InvokePreparationOnActors())
                {
                    InvokeCommitOnActors();
                }
                else
                {
                    InvokeRollbackOnActors();
                }
            }
            else
            {
                transactionLedger = null;
            }
        }

        public bool Enlist(string actorName)
        {
            bool success = false;

            if (TransactionLedger.ContainsKey(actorName))
            {
                TransactionLedger[actorName] = true;
                success = true;
            }

            return success;
        }

        #region Private Members
        private bool InvokePreparationOnActors()
        {
            bool success = false;

            foreach(string actor in TransactionLedger.Keys)
            {
                if(TransactionLedger[actor] && distributedTransactionActors.ContainsKey(actor))
                {
                    string endpointName = distributedTransactionActors[actor];
                    using (TransactionActorProxy transactionActorProxy = GetTransactionActorProxy(endpointName))
                    {
                        success = transactionActorProxy.Prepare();
                    }
                }
                else
                {
                    success = false;
                    break;
                }
            }

            return success;
        }

        private void InvokeCommitOnActors()
        {
            foreach (string actor in TransactionLedger.Keys)
            {
                //TODO: call Commit for actor -> find actor in a map <actorName, endpoint>
                if(distributedTransactionActors.ContainsKey(actor))
                {
                    string endpointName = distributedTransactionActors[actor];
                    using (TransactionActorProxy transactionActorProxy = GetTransactionActorProxy(endpointName))
                    {
                        transactionActorProxy.Commit();
                    }
                }
            }
        }

        private void InvokeRollbackOnActors()
        {
            foreach (string actor in TransactionLedger.Keys)
            {
                //TODO: call Rollback for actor -> find actor in a map <actorName, endpoint>
                if(distributedTransactionActors.ContainsKey(actor))
                {
                    string endpointName = distributedTransactionActors[actor];
                    using (TransactionActorProxy transactionActorProxy = GetTransactionActorProxy(endpointName))
                    {
                        transactionActorProxy.Rollback();
                    }
                }
            }
        }
        #endregion
    }
}
