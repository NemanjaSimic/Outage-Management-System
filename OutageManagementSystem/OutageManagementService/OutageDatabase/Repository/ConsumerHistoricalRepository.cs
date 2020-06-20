using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using OMSCommon.OutageDatabaseModel;

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
