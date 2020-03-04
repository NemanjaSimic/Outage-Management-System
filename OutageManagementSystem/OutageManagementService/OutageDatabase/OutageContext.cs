using OMSCommon.OutageDatabaseModel;
using OutageDatabase.Initializers;
using System.Data.Entity;
using System.Linq;

namespace OutageDatabase
{
    public class OutageContext : DbContext
    {
        public OutageContext() : base("OutageContext")
        {
            Database.SetInitializer(new OutageInitializer());
        }

        public DbSet<ActiveOutage> ActiveOutages { get; set; }
        public DbSet<ArchivedOutage> ArchivedOutages { get; set; }
        public DbSet<Consumer> Consumers { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
    }
}
