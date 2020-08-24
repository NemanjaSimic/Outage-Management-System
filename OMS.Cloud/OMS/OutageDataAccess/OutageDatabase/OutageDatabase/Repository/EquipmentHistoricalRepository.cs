using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using System.Collections.Generic;
using System.Linq;

namespace OutageDatabase.Repository
{
    public class EquipmentHistoricalRepository : Repository<EquipmentHistorical, long>
    {
        public EquipmentHistoricalRepository(OutageContext context)
        : base(context)
        {
        }
        public override EquipmentHistorical Get(long id)
        {
            return context.Set<EquipmentHistorical>().Where(eh => eh.Id == id)
                                                    .FirstOrDefault();
        }

        public override IEnumerable<EquipmentHistorical> GetAll()
        {
            return context.Set<EquipmentHistorical>();
        }
    }
}
