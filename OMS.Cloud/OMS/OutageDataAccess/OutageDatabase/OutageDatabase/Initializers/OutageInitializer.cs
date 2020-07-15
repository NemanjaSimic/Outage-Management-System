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
        //private ILogger logger;

        //private ILogger Logger
        //{
        //    get { return logger ?? (logger = LoggerWrapper.Instance); }
        //}

        public override void InitializeDatabase(OutageContext context)
        {
            //LoggerWrapper.Instance.LogDebug("InitializeDatabase called.");
            //base.InitializeDatabase(context);
            //Seed(context);
        }
    }
}
