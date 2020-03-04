using System;
using System.Data.Entity;
using System.Linq;

namespace OutageDatabase.Repository
{
    public sealed class UnitOfWork : IDisposable
    {
        private readonly OutageContext context;

        public ActiveOutageRepository ActiveOutageRepository { get; set; }
        public ArchivedOutageRepository ArchivedOutageRepository { get; set; }
        public ConsumerRepository ConsumerRepository { get; set; }
        public EquipmentRepository EquipmentRepository { get; set; }

        public UnitOfWork()
        {
            context = new OutageContext();

            ActiveOutageRepository = new ActiveOutageRepository(context);
            ArchivedOutageRepository = new ArchivedOutageRepository(context);
            ConsumerRepository = new ConsumerRepository(context);
            EquipmentRepository = new EquipmentRepository(context);
        }

        public UnitOfWork(OutageContext context)
        {
            this.context = context;

            ActiveOutageRepository = new ActiveOutageRepository(context);
            ArchivedOutageRepository = new ArchivedOutageRepository(context);
            ConsumerRepository = new ConsumerRepository(context);
            EquipmentRepository = new EquipmentRepository(context);
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
