using OMSCommon.OutageDatabaseModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OutageDatabase.Repository
{
    public class ArchivedOutageRepository : Repository<ArchivedOutage, long>
    {
        public ArchivedOutageRepository(OutageContext context)
            : base(context)
        {
        }

        public override ArchivedOutage Get(long id)
        {
            return context.Set<ArchivedOutage>().Include(a => a.AffectedConsumers)
                                                .Where(a => a.OutageId == id)
                                                .ToList()
                                                .FirstOrDefault();
        }

        public override IEnumerable<ArchivedOutage> GetAll()
        {
            return context.Set<ArchivedOutage>().Include(a => a.AffectedConsumers);
        }
    }
}
