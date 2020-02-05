using Outage.Common.PubSub.OutageDataContract;
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

        public void DeleteAllData()
        {
            foreach(ActiveOutage activeOutage in ActiveOutages)
            {
                ActiveOutages.Remove(activeOutage);
            }

            SaveChanges();
        }
        public DbSet<ActiveOutage> ActiveOutages { get; set; }
        public DbSet<ArchivedOutage> ArchivedOutages { get; set; }

        

    }
}
