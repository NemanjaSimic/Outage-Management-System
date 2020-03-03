namespace OutageDatabase.Migrations
{
    using OMSCommon.OutageDatabaseModel;
    using Outage.Common;
    using OutageDatabase.Repository;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<OutageContext>
    {
        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(OutageContext otageContext)
        {
            UnitOfWork dbContext = new UnitOfWork(otageContext);

            ArchivedOutage archivedOutage;

            long archivedId = 1;
            archivedOutage = dbContext.ArchivedOutageRepository.Get(archivedId);

            if (archivedOutage == null)
            {
                archivedOutage = new ArchivedOutage()
                {
                    OutageId = archivedId,
                    OutageElementGid = 0x0000000a00000007,
                    ReportTime = DateTime.UtcNow,
                    IsolatedTime = DateTime.UtcNow,
                    RepairedTime = DateTime.UtcNow,
                    ArchiveTime = DateTime.UtcNow,
                    DefaultIsolationPoints = string.Empty,
                    OptimumIsolationPoints = string.Empty,
                    AffectedConsumers = new List<Consumer>(),
                };

                archivedOutage = dbContext.ArchivedOutageRepository.Add(archivedOutage);
            }

            dbContext.ConsumerRepository.RemoveAll();
            dbContext.ActiveOutageRepository.RemoveAll();

            try
            {
                dbContext.Complete();
            }
            catch (Exception e)
            {
                string message = "Configuration::Seed method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message})");
            }
            //finally
            //{
            //    dbContext.Dispose();
            //}
        }
    }
}
