using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using System.Collections.Generic;

namespace OMS.Common.NMS
{
    public class NetorkModelUpdateTransaction
    {
        #region Instance
        private static object lockSync = new object();

        private static NetorkModelUpdateTransaction instance;
        public static NetorkModelUpdateTransaction Instance
        {
            get
            {
                if(instance == null)
                {
                    lock(lockSync)
                    {
                        if(instance == null)
                        {
                            instance = new NetorkModelUpdateTransaction();
                        }
                    }
                }
                
                return instance;
            }
        }
        #endregion Instance

        #region Public Properties
        private HashSet<string> transactionActorsNames;
        public HashSet<string> TransactionActorsNames
        {
            //preventing the outside modification - getter is not called many times 
            get { return new HashSet<string>(transactionActorsNames); }
        }
        #endregion Public Properties;

        private NetorkModelUpdateTransaction()
        {
            transactionActorsNames = new HashSet<string>
            {
                { MicroserviceNames.NmsGdaService               },
                { MicroserviceNames.ScadaModelProviderService   },
                //TODO: CE Transaction Actor
                //TODO: OMS Transaction Actor
            };
        }
    }
}
