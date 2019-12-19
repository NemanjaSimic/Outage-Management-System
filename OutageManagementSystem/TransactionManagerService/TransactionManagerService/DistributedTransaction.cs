using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies;
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
        private static readonly HashSet<string> distributedTransactionActors = new HashSet<string>()
        {
            ServiceNames.NetworkModelService,
            ServiceNames.SCADAService,
            ServiceNames.CalculationEngineService,
        };

        private static Dictionary<string, bool> transactionLedger = null;

        private static Dictionary<string, bool> TransactionLedger
        {
            get
            {
                return transactionLedger ?? (transactionLedger = new Dictionary<string, bool>(distributedTransactionActors.Count));
            }
        }

        public void StartDistributedUpdate()
        {
            transactionLedger = new Dictionary<string, bool>(distributedTransactionActors.Count);

            foreach (string actor in distributedTransactionActors)
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
                if(TransactionLedger[actor])
                {
                    //TODO: call Prepare for actor -> find actor in a map <actorName, endpoint>
                    throw new NotImplementedException();
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
                throw new NotImplementedException();   
            }
        }

        private void InvokeRollbackOnActors()
        {
            foreach (string actor in TransactionLedger.Keys)
            {
                //TODO: call Rollback for actor -> find actor in a map <actorName, endpoint>
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}
