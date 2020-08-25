using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using System.Collections.Generic;
using System.Linq;

namespace OutageDatabase.Repository
{
    public class ConsumerHistoricalRepository : Repository<ConsumerHistorical, long>
    {
        public ConsumerHistoricalRepository(OutageContext context)
        : base(context)
        {
        }
        public override ConsumerHistorical Get(long id)
        {
            return context.Set<ConsumerHistorical>().Where(ch => ch.Id == id)
                                                    .FirstOrDefault();
        }

        public override IEnumerable<ConsumerHistorical> GetAll()
        {
            return context.Set<ConsumerHistorical>();
        }
    }
}
