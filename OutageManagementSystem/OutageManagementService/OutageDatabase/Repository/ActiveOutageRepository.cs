using OMSCommon.OutageDatabaseModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OutageDatabase.Repository
{
    public class ActiveOutageRepository : Repository<ActiveOutage, long>
    {
        public ActiveOutageRepository(OutageContext context)
            : base(context)
        {
        }

        public override ActiveOutage Get(long id)
        {
            return context.Set<ActiveOutage>().Include(a => a.AffectedConsumers)
                                               .Where(a => a.OutageId == id)
                                               .FirstOrDefault();
        }

        public override IEnumerable<ActiveOutage> GetAll()
        {
            return context.Set<ActiveOutage>().Include(a => a.AffectedConsumers);
        }
    }
}
