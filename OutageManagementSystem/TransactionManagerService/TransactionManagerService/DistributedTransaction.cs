using Outage.Common;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.DistributedTransaction;
using System;
using System.Collections.Generic;

namespace Outage.TransactionManagerService
{
    public class DistributedTransaction : ITransactionCoordinatorContract, ITransactionEnlistmentContract

    {
        #region Static Members

        //TODO: get from config
        private static readonly Dictionary<string, string> distributedTransactionActors = new Dictionary<string, string>()
        {
            {   ServiceNames.NetworkModelService,       EndpointNames.NetworkModelTransactionActorEndpoint          },
            {   ServiceNames.SCADAService,              EndpointNames.SCADATransactionActorEndpoint                 },
            {   ServiceNames.CalculationEngineService,  EndpointNames.CalculationEngineTransactionActorEndpoint     },
            {   ServiceNames.OutageManagementService,   EndpointNames.OutageTransactionActorEndpoint                },
        };

        private static Dictionary<string, bool> transactionLedger = null;

        protected static Dictionary<string, bool> TransactionLedger
        {
            get
            {
                return transactionLedger ?? (transactionLedger = new Dictionary<string, bool>(distributedTransactionActors.Count));
            }
        }

        #endregion Static Members

        private ProxyFactory proxyFactory;

        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }


        //#region Proxies

        //private TransactionActorProxy transactionActorProxy = null;

        //private TransactionActorProxy GetTransactionActorProxy(string endpointName)
        //{
        //    int numberOfTries = 0;
        //    int sleepInterval = 500;  

        //    while (numberOfTries <= int.MaxValue)
        //    {
        //        try
        //        {
        //            if (transactionActorProxy != null)
        //            {
        //                transactionActorProxy.Abort();
        //                transactionActorProxy = null;
        //            }

        //            transactionActorProxy = new TransactionActorProxy(endpointName);
        //            transactionActorProxy.Open();

        //            if (transactionActorProxy.State == CommunicationState.Opened)
        //            {
        //                break;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string message = $"Exception on TransactionActorProxy initialization. Message: {ex.Message}";
        //            Logger.LogWarn(message, ex);
        //            transactionActorProxy = null;
        //        }
        //        finally
        //        {
        //            numberOfTries++;
        //            Logger.LogDebug($"DistributedTransaction: GetTransactionActorProxy(), EndpointName: {endpointName}, try number: {numberOfTries}.");

        //            if (numberOfTries >= 100)
        //            {
        //                sleepInterval = 1000;
        //            }

        //            Thread.Sleep(sleepInterval);
        //        }
        //    }

        //    return transactionActorProxy;
        //}

        //#endregion Proxies

        public DistributedTransaction()
        {
            proxyFactory = new ProxyFactory();
        }

        #region ITransactionCoordinatorContract

        public void StartDistributedUpdate()
        {
            transactionLedger = new Dictionary<string, bool>(distributedTransactionActors.Count);

            foreach (string actor in distributedTransactionActors.Keys)
            {
                if (!TransactionLedger.ContainsKey(actor))
                {
                    TransactionLedger.Add(actor, false);
                }
            }

            Logger.LogInfo("Distributed transaction started. Waiting for transaction actors to enlist...");
            //TODO: start timer...
        }

        public void FinishDistributedUpdate(bool success)
        {
            try
            {
                if (success)
                {
                    if (InvokePreparationOnActors())
                    {
                        InvokeCommitOnActors();
                    }
                    else
                    {
                        InvokeRollbackOnActors();
                    }

                    Logger.LogInfo("Distributed transaction finsihed SUCCESSFULLY.");
                }
                else
                {
                    transactionLedger = null;
                    Logger.LogInfo("Distributed transaction finsihed UNSUCCESSFULLY.");
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Exception in FinishDistributedUpdate().", e);
            }
        }

        #endregion ITransactionCoordinatorContract

        #region ITransactionEnlistmentContract

        public bool Enlist(string actorName)
        {
            bool success = false;

            if (TransactionLedger.ContainsKey(actorName))
            {
                TransactionLedger[actorName] = true;
                success = true;
                Logger.LogInfo($"Transaction actor: {actorName} enlisted for transaction.");
            }

            return success;
        }

        #endregion ITransactionEnlistmentContract

        #region Private Members

        private bool InvokePreparationOnActors()
        {
            bool success = false;

            foreach (string actor in TransactionLedger.Keys)
            {
                if (TransactionLedger[actor] && distributedTransactionActors.ContainsKey(actor))
                {
                    string endpointName = distributedTransactionActors[actor];

                    using (TransactionActorProxy transactionActorProxy = proxyFactory.CreateProxy<TransactionActorProxy, ITransactionActorContract>(endpointName))
                    {
                        if (transactionActorProxy != null)
                        {
                            success = transactionActorProxy.Prepare();
                        }
                        else
                        {
                            success = false;
                            string message = "TransactionActorProxy is null.";
                            Logger.LogError(message);
                            throw new NullReferenceException(message);
                        }
                    }

                    if (success)
                    {
                        Logger.LogInfo($"Preparation on Transaction actor: {actor} finsihed SUCCESSFULLY.");
                    }
                    else
                    {
                        Logger.LogInfo($"Preparation on Transaction actor: {actor} finsihed UNSUCCESSFULLY.");
                        break;
                    }
                }
                else
                {
                    success = false;
                    Logger.LogError($"Preparation failed either because Transaction actor: {actor} was not enlisted or do not belong to distributed transaction.");
                    break;
                }
            }

            return success;
        }

        private void InvokeCommitOnActors()
        {
            foreach (string actor in TransactionLedger.Keys)
            {
                if (distributedTransactionActors.ContainsKey(actor))
                {
                    string endpointName = distributedTransactionActors[actor];

                    using (TransactionActorProxy transactionActorProxy = proxyFactory.CreateProxy<TransactionActorProxy, ITransactionActorContract>(endpointName))
                    {
                        if (transactionActorProxy != null)
                        {
                            transactionActorProxy.Commit();
                            Logger.LogInfo($"Commit invoked on Transaction actor: {actor}.");
                        }
                        else
                        {
                            string message = "TransactionActorProxy is null.";
                            Logger.LogError(message);
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
                if (distributedTransactionActors.ContainsKey(actor))
                {
                    string endpointName = distributedTransactionActors[actor];

                    using (TransactionActorProxy transactionActorProxy = proxyFactory.CreateProxy<TransactionActorProxy, ITransactionActorContract>(endpointName))
                    {
                        if (transactionActorProxy != null)
                        {
                            transactionActorProxy.Rollback();
                            Logger.LogInfo($"Rollback invoked on Transaction actor: {actor}.");
                        }
                        else
                        {
                            string message = "TransactionActorProxy is null.";
                            Logger.LogError(message);
                            throw new NullReferenceException(message);
                        }
                    }
                }
            }
        }

        #endregion Private Members
    }
}