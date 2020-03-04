using OMSCommon.OutageDatabaseModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;


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
            return context.Set<Equipment>().Include(e => e.ActiveOutages)
                                          .Include(e => e.ArchivedOutages)
                                          .Where(e => e.EquipmentId == id)
                                          .FirstOrDefault();
        }

        public override IEnumerable<Equipment> GetAll()
        {
            return context.Set<Equipment>().Include(c => c.ActiveOutages)
                                          .Include(c => c.ArchivedOutages);
        }
    }
}
