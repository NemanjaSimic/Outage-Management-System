using Outage.Common;
using System;
using System.Collections.Generic;

namespace OMSCommon.OutageDatabaseModel
{
    public class Outage
    {
        public DateTime ReportTime { get; set; }
        public DateTime? IsolatedTime { get; set; }
        public DateTime? ResolvedTime { get; set; }

        public long OutageElementGid { get; set; }
        //CSV, separator '|'
        public string DefaultIsolationPoints { get; set; }
        //CSV, separator '|'
        public string OptimumIsolationPoints { get; set; }

        public List<Consumer> AffectedConsumers { get; set; }

        public Outage()
        {
            DefaultIsolationPoints = string.Empty;
            OptimumIsolationPoints = string.Empty;
            AffectedConsumers = new List<Consumer>();
        }
    }
}
