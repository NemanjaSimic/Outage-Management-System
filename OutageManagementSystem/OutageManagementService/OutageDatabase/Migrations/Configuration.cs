namespace OutageDatabase.Migrations
{
    using OMSCommon.OutageDatabaseModel;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

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
                    ReportTime = DateTime.UtcNow,
                    IsolatedTime = DateTime.UtcNow,
                    ResolvedTime = DateTime.UtcNow,
                    ArchiveTime = DateTime.UtcNow,
                    DefaultIsolationPoints = string.Empty,
                    OptimumIsolationPoints = string.Empty,
                    AffectedConsumers = new List<Consumer>(),
                };

                archivedOutage = context.ArchivedOutages.Add(archivedOutage);
            }

            context.SaveChanges();

            context.DeleteAllData();
            //context.Dispose();
        }
    }
}
