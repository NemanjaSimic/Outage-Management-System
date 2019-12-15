using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Outage.Common;
using Outage.NetworkModelService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.DBModel.NetworkModelService
{
    public class NetworkDataModelDocument
    {
        public long Id { get; set; }

        public Dictionary<DMSType, Container> NetworkModel { get; set; }
    }
}
