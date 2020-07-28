using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.GdaImplementation.DbModel
{
    public static class MongoStrings
    {
        //databases
        public const string NMSDatabase = "NMSDatabase";

        //collections
        public const string LatestVersionsCollection = "LatestVersionsCollection";
        public const string NetworkModelsCollection = "NetworkModelsCollection";
        public const string DeltasCollection = "DeltasCollection";

        //LatestVersionsCollection
        public const string LatestVersions_NetworkModelVersion = "NetworkModelVersion";
        public const string LatestVersions_DeltaVersion = "DeltaVersion";

        //DeltasCollection
        //NetworkModelsCollection
    }
}
