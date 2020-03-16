using CECommon.Providers;
using Outage.Common.GDA;
using System.Collections.Generic;

namespace CalculationEngineService
{
    public class TransactionManager
    {
        private Dictionary<DeltaOpType, List<long>> delta;

        private TransactionManager() {}

        #region Singleton
        private static TransactionManager instance;
        private static object syncObj = new object();

        public static TransactionManager Intance
        {
            get
            {
                lock (syncObj)
                {
                    if (instance == null)
                    {
                        instance = new TransactionManager();
                    }
                }
                return instance;
            }
        }
        #endregion

        public bool UpdateNotify(Dictionary<DeltaOpType, List<long>> newDelta)
        {
            delta = new Dictionary<DeltaOpType, List<long>>(newDelta);
            return true;
        }

        public bool Prepare()
        {
            bool success;

            if(Provider.Instance.ModelProvider.PrepareForTransaction())
            {
                success = true;
            }
            else
            {
                success = false;
            }

            return success;
        }

        public void CommitTransaction()
        {
            Provider.Instance.ModelProvider.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            Provider.Instance.ModelProvider.RollbackTransaction();
        }
    }
}
