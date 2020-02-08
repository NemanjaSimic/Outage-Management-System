namespace OutageDatabase
{
    using Outage.Common.PubSub.OutageDataContract;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public class Configuration : DbMigrationsConfiguration<OutageContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "Context";
        }

        protected override void Seed(OutageContext context)
        {
            Consumer consumer1, consumer2;
            ActiveOutage activeOutage;
            ArchivedOutage archivedOutage;

            long archivedId = 1;
            archivedOutage = context.ArchivedOutages.Find(archivedId);

            if (archivedOutage == null)
            {
                archivedOutage = new ArchivedOutage()
                {
                    OutageId = archivedId,
                    ElementGid = 0x0000000a00000007,
                    ReportTime = DateTime.Now,
                    AffectedConsumers = new List<Consumer>(),
                    ArchiveTime = DateTime.Now,
                };

                archivedOutage = context.ArchivedOutages.Add(archivedOutage);
            }

            long activeId = 2;
            activeOutage = context.ActiveOutages.Find(activeId);

            if (activeOutage == null)
            {
                activeOutage = new ActiveOutage()
                {
                    OutageId = activeId,
                    ElementGid = 0x0000000a00000001,
                    ReportTime = DateTime.Now,
                    AffectedConsumers = new List<Consumer>(),
                };

                activeOutage = context.ActiveOutages.Add(activeOutage);
            }

            long gid1 = 0x0000000600000001;
            consumer1 = context.Consumers.Find(gid1);

            if (consumer1 == null)
            {
                consumer1 = new Consumer()
                {
                    ConsumerId = gid1,
                    ConsumerMRID = "EC_1",
                    FirstName = "Joe",
                    LastName = "Doe",
                    ActiveOutages = new List<ActiveOutage>() { activeOutage },
                    ArchivedOutages = new List<ArchivedOutage>() { archivedOutage },
                };

                context.Consumers.Add(consumer1);
            }

            long gid2 = 0x000000060000000c;
            consumer2 = context.Consumers.Find(gid2);

            if (consumer2 == null)
            {
                consumer2 = new Consumer()
                {
                    ConsumerId = gid2,
                    ConsumerMRID = "EC_2",
                    FirstName = "Joe",
                    LastName = "Doe",
                    ActiveOutages = new List<ActiveOutage>() { activeOutage },
                    ArchivedOutages = new List<ArchivedOutage>() { archivedOutage },
                };

                context.Consumers.Add(consumer2);
            }

            context.SaveChanges();
            //context.Dispose();
        }
    }
}
