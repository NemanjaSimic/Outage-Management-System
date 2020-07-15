using Common.OMS.OutageDatabaseModel;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using OMS.Common.Cloud;

namespace OutageDatabase.Repository
{
	public class OutageRepository : Repository<OutageEntity, long>
    {
        public OutageRepository(OutageContext context)
            : base(context)
        {
        }

        public override OutageEntity Get(long id)
        {
            return context.Set<OutageEntity>().Include(o => o.AffectedConsumers)
                                              .Include(o => o.DefaultIsolationPoints)
                                              .Include(o => o.OptimumIsolationPoints)
                                              .Where(o => o.OutageId == id).FirstOrDefault();
        }

        public IEnumerable<OutageEntity> GetAllActive()
        {
            return context.Set<OutageEntity>().Include(o => o.AffectedConsumers)
                                              .Include(o => o.DefaultIsolationPoints)
                                              .Include(o => o.OptimumIsolationPoints)
                                              .Where(o => o.OutageState != OutageState.ARCHIVED);
        }

        public IEnumerable<OutageEntity> GetAllArchived()
        {
            return context.Set<OutageEntity>().Include(o => o.AffectedConsumers)
                                              .Include(o => o.DefaultIsolationPoints)
                                              .Include(o => o.OptimumIsolationPoints)
                                              .Where(o => o.OutageState == OutageState.ARCHIVED);
        }

        public override IEnumerable<OutageEntity> GetAll()
        {
            return context.Set<OutageEntity>().Include(o => o.AffectedConsumers)
                                              .Include(o => o.DefaultIsolationPoints)
                                              .Include(o => o.OptimumIsolationPoints);
        }
    }
}
