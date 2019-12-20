using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Service.DistributedTransaction
{
    public class SCADATransactionActor : TransactionActor
    {
        public override bool Prepare()
        {
            return base.Prepare();
        }

        public override void Commit()
        {
            throw new NotImplementedException();
        }

        public override void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
