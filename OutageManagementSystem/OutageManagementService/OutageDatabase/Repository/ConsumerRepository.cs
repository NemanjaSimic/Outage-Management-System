using OMSCommon.OutageDatabaseModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OutageDatabase.Repository
{
    public class ConsumerRepository : Repository<Consumer, long>
    {
        public ConsumerRepository(OutageContext context)
            : base(context)
        {
        }

        public override Consumer Get(long id)
        {
            return context.Set<Consumer>().Include(c => c.Outages)
                                          .Where(c => c.ConsumerId == id)
                                          .FirstOrDefault();
        }

        public override IEnumerable<Consumer> GetAll()
        {
            return context.Set<Consumer>().Include(c => c.Outages);
        }
    }
}
