using System;
using System.Data.Entity;
using System.Linq;

namespace OutageDatabase.Repository
{
    public sealed class UnitOfWork : IDisposable
    {
        private readonly OutageContext context;

        public OutageRepository OutageRepository { get; set; }
        public ConsumerRepository ConsumerRepository { get; set; }
        public EquipmentRepository EquipmentRepository { get; set; }
        public ConsumerHistoricalRepository ConsumerHistoricalRepository { get; set; }
        public EquipmentHistoricalRepository EquipmentHistoricalRepository { get; set; }

        public UnitOfWork()
        {
            context = new OutageContext();

            OutageRepository = new OutageRepository(context);
            ConsumerRepository = new ConsumerRepository(context);
            EquipmentRepository = new EquipmentRepository(context);
            ConsumerHistoricalRepository = new ConsumerHistoricalRepository(context);
            EquipmentHistoricalRepository = new EquipmentHistoricalRepository(context);
        }

        public UnitOfWork(OutageContext context)
        {
            this.context = context;

            OutageRepository = new OutageRepository(context);
            ConsumerRepository = new ConsumerRepository(context);
            EquipmentRepository = new EquipmentRepository(context);
            ConsumerHistoricalRepository = new ConsumerHistoricalRepository(context);
            EquipmentHistoricalRepository = new EquipmentHistoricalRepository(context);
        }

        public int Complete()
        {
            var added = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
            var deleted = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted);
            var modified = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);

            return context.SaveChanges();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
