namespace OutageDatabase.Migrations
{
    using Outage.Common.PubSub.OutageDataContract;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<OutageContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(OutageContext context)
        {
            ArchivedOutage archivedOutage;

            long archivedId = 1;
            archivedOutage = context.ArchivedOutages.Find(archivedId);

            if (archivedOutage == null)
            {
                archivedOutage = new ArchivedOutage()
                {
                    OutageId = archivedId,
                    OutageElementGid = 0x0000000a00000007,
                    ReportTime = DateTime.Now,
                    AffectedConsumers = new List<Consumer>(),
                    ArchiveTime = DateTime.Now,
                };

                archivedOutage = context.ArchivedOutages.Add(archivedOutage);
            }


            context.SaveChanges();

            context.DeleteAllData();
            //context.Dispose();
        }
    }
}
