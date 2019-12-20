using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService.DistributedTransaction
{
    public class CETransactionActor : TransactionActor
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
