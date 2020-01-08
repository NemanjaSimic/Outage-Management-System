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
        #region Static Members
        //TODO: get from config
        private static readonly Dictionary<string, string> distributedTransactionActors = new Dictionary<string, string>()
        {
            {   ServiceNames.NetworkModelService,       EndpointNames.NetworkModelTransactionActorServiceHost       },
            {   ServiceNames.SCADAService,              EndpointNames.SCADATransactionActorEndpoint                 },
            {   ServiceNames.CalculationEngineService,  EndpointNames.CalculationEngineTransactionActorEndpoint     },
        };

        private static Dictionary<string, bool> transactionLedger = null;

        protected static Dictionary<string, bool> TransactionLedger
        {
            get
            { 
                return transactionLedger ?? (transactionLedger = new Dictionary<string, bool>(distributedTransactionActors.Count));
            }
        }
        #endregion

        private ILogger logger = LoggerWrapper.Instance;
        private TransactionActorProxy transactionActorProxy = null;

        #region Proxies
        protected TransactionActorProxy GetTransactionActorProxy(string endpoint)
        {
            try
            {
                if (transactionActorProxy != null)
                {
                    transactionActorProxy.Abort();
                    transactionActorProxy = null;
                }

                transactionActorProxy = new TransactionActorProxy(endpoint);
                transactionActorProxy.Open();
            }
            catch (Exception ex)
            {
                string message = $"Exception on TransactionActorProxy initialization. Message: {ex.Message}";
                logger.LogError(message, ex);
                transactionActorProxy = null;
            }

            return transactionActorProxy;
        }
        #endregion

        #region ITransactionCoordinatorContract
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

            logger.LogInfo("Distributed transaction started. Waiting for transaction actors to enlist...");
            //TODO: start timer...
        }
        
        public void FinishDistributedUpdate(bool success)
        {
            try
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

                    logger.LogInfo("Distributed transaction finsihed SUCCESSFULLY.");
                }
                else
                {
                    transactionLedger = null;
                    logger.LogInfo("Distributed transaction finsihed UNSUCCESSFULLY.");
                }
            }
            catch (Exception e)
            {
                logger.LogError("Exception in FinishDistributedUpdate().", e);
            }

        }
        #endregion

        #region ITransactionEnlistmentContract
        public bool Enlist(string actorName)
        {
            bool success = false;

            if (TransactionLedger.ContainsKey(actorName))
            {
                TransactionLedger[actorName] = true;
                success = true;
                logger.LogInfo($"Transaction actor: {actorName} enlisted for transaction.");
            }

            return success;
        }
        #endregion

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
                        if(transactionActorProxy != null)
                        {
                            success = transactionActorProxy.Prepare();
                        }
                        else
                        {
                            success = false;
                            string message = "TransactionActorProxy is null.";
                            logger.LogError(message);
                            throw new NullReferenceException(message);
                        }
                    }

                    if(success)
                    {
                        logger.LogInfo($"Preparation on Transaction actor: {actor} finsihed SUCCESSFULLY.");
                    }
                    else
                    {
                        logger.LogInfo($"Preparation on Transaction actor: {actor} finsihed UNSUCCESSFULLY.");
                        break;
                    }
                }
                else
                {
                    success = false;
                    logger.LogError($"Preparation failed either because Transaction actor: {actor} was not enlisted or do not belong to distributed transaction.");
                    break;
                }
            }

            return success;
        }

        private void InvokeCommitOnActors()
        {
            foreach (string actor in TransactionLedger.Keys)
            {
                if(distributedTransactionActors.ContainsKey(actor))
                {
                    string endpointName = distributedTransactionActors[actor];
                    using (TransactionActorProxy transactionActorProxy = GetTransactionActorProxy(endpointName))
                    {
                        if (transactionActorProxy != null)
                        {
                            transactionActorProxy.Commit();
                            logger.LogInfo($"Commit invoked on Transaction actor: {actor}.");
                        }
                        else
                        {
                            string message = "TransactionActorProxy is null.";
                            logger.LogError(message);
                            throw new NullReferenceException(message);
                        }
                    }
                }
            }
        }

        private void InvokeRollbackOnActors()
        {
            foreach (string actor in TransactionLedger.Keys)
            {
                if(distributedTransactionActors.ContainsKey(actor))
                {
                    string endpointName = distributedTransactionActors[actor];
                    using (TransactionActorProxy transactionActorProxy = GetTransactionActorProxy(endpointName))
                    {
                        if (transactionActorProxy != null)
                        {
                            transactionActorProxy.Rollback();
                            logger.LogInfo($"Rollback invoked on Transaction actor: {actor}.");
                        }
                        else
                        {
                            string message = "TransactionActorProxy is null.";
                            logger.LogError(message);
                            throw new NullReferenceException(message);
                        }
                    }
                }
            }
        }
        #endregion
    }

    //public class DistributedTransactionEnlistment : ITransactionEnlistmentContract
    //{
    //    private ILogger logger = LoggerWrapper.Instance;

    //    #region ITransactionEnlistmentContract
    //    public bool Enlist(string actorName)
    //    {
    //        bool success = false;

    //        if (TransactionLedger.ContainsKey(actorName))
    //        {
    //            TransactionLedger[actorName] = true;
    //            success = true;
    //            logger.LogInfo($"Transaction actor: {actorName} enlisted for transaction.");
    //        }

    //        return success;
    //    }
    //    #endregion
    //}
}