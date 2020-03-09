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
    }
}
