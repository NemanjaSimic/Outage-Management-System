using Outage.Common;
using System.Collections.Generic;

namespace OMS.Cloud.NMS.Provider.DbModel
{
    public class NetworkDataModelDocument
    {
        public long Id { get; set; }

        public Dictionary<DMSType, Container> NetworkModel { get; set; }
    }
}
