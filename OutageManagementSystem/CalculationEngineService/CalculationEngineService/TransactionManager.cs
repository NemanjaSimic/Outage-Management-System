using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class TransactionManager
    {
        private Dictionary<DeltaOpType, List<long>> delta;

        private TransactionManager() {}

        #region Singleton
        private static TransactionManager instance;
        private static object syncObj = new object();

        public static TransactionManager Instance
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

        public void UpdateNotify(Dictionary<DeltaOpType, List<long>> newDelta)
        {
            delta = newDelta;
        }
    }
}
