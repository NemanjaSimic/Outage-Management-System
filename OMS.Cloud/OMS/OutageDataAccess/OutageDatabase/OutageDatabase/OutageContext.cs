using Common.OMS.OutageDatabaseModel;
using OutageDatabase.Initializers;
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

        public DbSet<OutageEntity> OutageEntities { get; set; }
        public DbSet<Consumer> Consumers { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<EquipmentHistorical> EquipmentsHistorical { get; set; }
        public DbSet<ConsumerHistorical> ConsumersHistorical { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OutageEntity>().HasKey(o => o.OutageId).ToTable("OutageEntities");
            modelBuilder.Entity<Consumer>().HasKey(c => c.ConsumerId).ToTable("Consumers");
            modelBuilder.Entity<Equipment>().HasKey(e => e.EquipmentId).ToTable("Equipments");
            modelBuilder.Entity<EquipmentHistorical>().HasKey(eh => eh.Id).ToTable("EquipmentsHistorical");
            modelBuilder.Entity<ConsumerHistorical>().HasKey(ch => ch.Id).ToTable("ConsumersHistorical");

            modelBuilder.Entity<OutageEntity>().HasMany(o => o.AffectedConsumers)
                                               .WithMany(c => c.Outages)
                                               .Map(oc =>
                                               {
                                                   oc.MapLeftKey("OutageRefId");
                                                   oc.MapRightKey("ConsumerRefId");
                                                   oc.ToTable("OutageConsumers");
                                               });

            modelBuilder.Entity<OutageEntity>().HasMany(o => o.DefaultIsolationPoints)
                                               .WithMany(e => e.OutagesAsDefaultIsolation)
                                               .Map(oe =>
                                               {
                                                   oe.MapLeftKey("OutageRefId");
                                                   oe.MapRightKey("DefaultEquipmentRefId");
                                                   oe.ToTable("OutageDefaultEquipments");
                                               });

            modelBuilder.Entity<OutageEntity>().HasMany(o => o.OptimumIsolationPoints)
                                               .WithMany(e => e.OutagesAsOptimumIsolation)
                                               .Map(oe =>
                                               {
                                                   oe.MapLeftKey("OutageRefId");
                                                   oe.MapRightKey("OptimumEquipmentRefId");
                                                   oe.ToTable("OutageOptimumEquipments");
                                               });
        }
    }
}
