using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.OMS;
using OutageDatabase.Initializers;
using OutageDatabase.Migrations;
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
            Database.SetInitializer(new OutageInitializer());
        }

        public DbSet<ActiveOutage> ActiveOutages { get; set; }
        public DbSet<ArchivedOutage> ArchivedOutages { get; set; }
        public DbSet<Consumer> Consumers { get; set; }
        
        public void DeleteAllData()
        {
            foreach(ActiveOutage activeOutage in ActiveOutages)
            {
                ActiveOutages.Remove(activeOutage);
            }

            foreach(Consumer consumer in Consumers) //TODO: restauration...
            {
                Consumers.Remove(consumer);
            }

            SaveChanges();
        }
        public ActiveOutage GetActiveOutage(long elementGid)
        {
            var outage = from active in ActiveOutages
                         where active.OutageElementGid == elementGid
                         select active;


            return outage.First();
        }
    }
}
