﻿using Outage.Common;
using Outage.Common.PubSub.OutageDataContract;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageDatabase.Initializers
{
    public class OutageInitializer : DropCreateDatabaseIfModelChanges<OutageContext>
    {
        public override void InitializeDatabase(OutageContext context)
        {
            LoggerWrapper.Instance.LogDebug("InitializeDatabase called.");
            base.InitializeDatabase(context);
            Seed(context);
        }

        protected override void Seed(OutageContext context)
        {
            base.Seed(context);


            //TODO: rethink
            context.DeleteAllData();

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


            context.SaveChanges();
        }
    }
}