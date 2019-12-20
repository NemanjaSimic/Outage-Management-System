using Outage.DistributedTransactionActor;
using System;

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
