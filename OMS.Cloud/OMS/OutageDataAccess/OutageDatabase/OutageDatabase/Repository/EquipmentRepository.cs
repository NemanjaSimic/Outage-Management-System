using Common.OMS.OutageDatabaseModel;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace OutageDatabase.Repository
{
	public class EquipmentRepository : Repository<Equipment, long>
    {
        public EquipmentRepository(OutageContext context)
            : base(context)
        {
        }

        public override Equipment Get(long id)
        {
            return context.Set<Equipment>().Include(e => e.OutagesAsDefaultIsolation)
                                           .Include(e => e.OutagesAsOptimumIsolation)
                                           .Where(e => e.EquipmentId == id)
                                           .FirstOrDefault();
        }

        public override IEnumerable<Equipment> GetAll()
        {
            return context.Set<Equipment>().Include(c => c.OutagesAsDefaultIsolation)
                                           .Include(e => e.OutagesAsOptimumIsolation);
        }
    }
}
