
using OMS.Common.Cloud;
using System.Collections.Generic;

namespace NMS.GdaImplementation.DbModel
{
    public class NetworkModelsDocument
    {
        public long Id { get; set; }

        public Dictionary<DMSType, Container> NetworkModel { get; set; }
    }
}
