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

            dbContext.OutageRepository.RemoveAll();
            dbContext.ConsumerRepository.RemoveAll();
            dbContext.EquipmentRepository.RemoveAll();

            long archivedId = 1;
            OutageEntity archivedOutage = dbContext.OutageRepository.Get(archivedId);

            if (archivedOutage == null)
            {
                archivedOutage = new OutageEntity()
                {
                    OutageId = archivedId,
                    OutageState = OutageState.ARCHIVED,
                    OutageElementGid = 0x0000000C00000007,
                    IsResolveConditionValidated = true,
                    ReportTime = DateTime.UtcNow,
                    IsolatedTime = DateTime.UtcNow,
                    RepairedTime = DateTime.UtcNow,
                    ArchivedTime = DateTime.UtcNow,
                    DefaultIsolationPoints = new List<Equipment>(),
                    OptimumIsolationPoints = new List<Equipment>(),
                    AffectedConsumers = new List<Consumer>(),
                };

                archivedOutage = dbContext.OutageRepository.Add(archivedOutage);
            }

            //long defaultIsolationId = 0x0000000a00000001;
            //Equipment defaultIsolation = dbContext.EquipmentRepository.Get(defaultIsolationId);

            //if (defaultIsolation == null)
            //{
            //    defaultIsolation = new Equipment()
            //    {
            //        EquipmentId = defaultIsolationId,
            //        EquipmentMRID = "BR_NESTO",
            //        OutagesAsDefaultIsolation = new List<OutageEntity>() { archivedOutage },
            //    };

            //    defaultIsolation = dbContext.EquipmentRepository.Add(defaultIsolation);
            //}

            //long optimumIsolationId = 0x0000000a00000002;
            //Equipment optimumIsolation = dbContext.EquipmentRepository.Get(optimumIsolationId);

            //if (optimumIsolation == null)
            //{
            //    optimumIsolation = new Equipment()
            //    {
            //        EquipmentId = defaultIsolationId,
            //        EquipmentMRID = "BR_NESTO",
            //        OutagesAsOptimumIsolation = new List<OutageEntity>() { archivedOutage },
            //    };

            //    optimumIsolation = dbContext.EquipmentRepository.Add(defaultIsolation);
            //}

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
