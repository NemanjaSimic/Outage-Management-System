using CECommon.Interfaces;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon
{
    public class SmartCache
    {
        public ITopology Topology { get; set; }
        public Dictionary<long, ResourceDescription> ModelEntities { get; set; }
        public Dictionary<long, ResourceDescription> TransactionModelEntities { get; set; }


    }
}
