using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
using Outage.Common.ServiceProxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.TransactionManagerService
{
    public class DistributedTransactionCoordinator : ITransactionCoordinatorContract
    {
        //TODO: get from config
        private static readonly HashSet<string> distributedTransactionActors = new HashSet<string>()
        {
            ServiceHostNames.NetworkModelService,
            ServiceHostNames.SCADAService,
            ServiceHostNames.CalculationEngineService,
        };

        //TODO: get from config
        private static readonly HashSet<string> distibutedTransactionInitiators = new HashSet<string>()
        {
            ServiceHostNames.NetworkModelService,
        };

        private static Dictionary<string, bool> transactionLedger = null;

        private static Dictionary<string, bool> TransactionLedger
        {
            get
            {
                return transactionLedger ?? (transactionLedger = new Dictionary<string, bool>());
            }
        }

        public void StartDistributedUpdate(Delta delta, string actorName)
        {
            if(!distibutedTransactionInitiators.Contains(actorName))
            {
                return;
            }

            transactionLedger = new Dictionary<string, bool>(distributedTransactionActors.Count);

            foreach (string actor in distributedTransactionActors)
            {
                TransactionLedger.Add(actor, false);
            }

            if(!EnlistDeltaToActors(delta))
            {
                InvokeRollbackActors();
            }

            if(!InvokePreparationOnActors())
            {
                InvokeRollbackActors();
            }
        }
        
        public void FinishDistributedUpdate(string actorName, bool success)
        {
            if(TransactionLedger.ContainsKey(actorName))
            {
                TransactionLedger[actorName] = success;
            }

            bool transactionFinished = true;
            foreach(string actor in TransactionLedger.Keys)
            {
                if(!TransactionLedger[actor])
                {
                    transactionFinished = false;
                    break;
                }
            }

            if(transactionFinished)
            {
                InvokeCommitOnActors();
            }
        }

        #region Private Members
        private bool EnlistDeltaToActors(Delta delta)
        {
            foreach(string actor in TransactionLedger.Keys)
            {

            }

            //todo: finish
            return true;
        }

        private bool InvokePreparationOnActors()
        {
            throw new NotImplementedException();
        }

        private void InvokeCommitOnActors()
        {
            throw new NotImplementedException();
        }

        private void InvokeRollbackActors()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
