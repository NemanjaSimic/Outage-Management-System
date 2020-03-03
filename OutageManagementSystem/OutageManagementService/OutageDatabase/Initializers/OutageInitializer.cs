using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace OutageDatabase.Initializers
{
    public class OutageInitializer : DropCreateDatabaseIfModelChanges<OutageContext>
    {
        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public override void InitializeDatabase(OutageContext context)
        {
            LoggerWrapper.Instance.LogDebug("InitializeDatabase called.");
            base.InitializeDatabase(context);
            Seed(context);
        }

        protected override void Seed(OutageContext outageContext)
        {
            base.Seed(outageContext);

            UnitOfWork dbContext = new UnitOfWork(outageContext);

            dbContext.ActiveOutageRepository.RemoveAll();
            dbContext.ArchivedOutageRepository.RemoveAll();
            dbContext.ConsumerRepository.RemoveAll();

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

            try
            {
                dbContext.Complete();
            }
            catch (Exception e)
            {
                string message = "OutageInitializer::Seed method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message})");
            }
            finally
            {
                //dbContext.Dispose();
                //exception thrown if dispose is called...
            }
        }
    }
}
