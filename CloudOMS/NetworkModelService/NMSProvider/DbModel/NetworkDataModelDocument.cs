using Outage.Common;
using System.Collections.Generic;

namespace CloudOMS.NetworkModelService.NMSProvider.DbModel
{
    public class NetworkDataModelDocument
    {
        public long Id { get; set; }

        public Dictionary<DMSType, Container> NetworkModel { get; set; }
    }
}
