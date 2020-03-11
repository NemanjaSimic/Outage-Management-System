using Outage.Common;
using System.Collections.Generic;

namespace CloudOMS.NetworkModelService.NMS.Provider.DbModel
{
    public class NetworkDataModelDocument
    {
        public long Id { get; set; }

        public Dictionary<DMSType, Container> NetworkModel { get; set; }
    }
}
