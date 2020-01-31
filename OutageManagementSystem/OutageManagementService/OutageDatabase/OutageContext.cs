using Outage.Common.ServiceContracts.OMS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageDatabase
{
    public class OutageContext : DbContext
    {
        public OutageContext() : base("OutageContext")
        {

        }
        public DbSet<ActiveOutage> ActiveOutages { get; set; }
        public DbSet<ArchivedOutage> ArchivedOutages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ArchivedOutage>().Map(m =>
            {
                m.MapInheritedProperties();
                m.ToTable("ArchivedOutages");
            });

        }
    }
}
