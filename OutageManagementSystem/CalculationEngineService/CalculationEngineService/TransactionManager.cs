using NetworkModelServiceFunctions;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topology;
using Logger = Outage.Common.LoggerWrapper;

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

            if(NMSManager.Instance.PrepareForTransaction(delta) && TopologyManager.Instance.PrepareForTransaction())
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
            NMSManager.Instance.CommitTransaction();
            TopologyManager.Instance.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            NMSManager.Instance.RollbackTransaction();
            TopologyManager.Instance.RollbackTransaction();
        }
    }
}
