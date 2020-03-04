using Outage.Common;
using System;
using System.Collections.Generic;

namespace OMSCommon.OutageDatabaseModel
{
    public class Outage
    {
        public DateTime ReportTime { get; set; }
        public DateTime? IsolatedTime { get; set; }
        public DateTime? RepairedTime { get; set; }

        public long OutageElementGid { get; set; }

        public List<Equipment> DefaultIsolationPoints { get; set; }
        public List<Equipment> OptimumIsolationPoints { get; set; }
        public List<Consumer> AffectedConsumers { get; set; }

        public Outage()
        {
            DefaultIsolationPoints = new List<Equipment>();
            OptimumIsolationPoints = new List<Equipment>();
            AffectedConsumers = new List<Consumer>();
        }
    }
}
