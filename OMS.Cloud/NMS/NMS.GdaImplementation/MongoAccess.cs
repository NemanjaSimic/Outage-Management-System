using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using OMS.Common.NmsContracts.GDA;
using NMS.DataModel;
using NMS.GdaImplementation.DbModel;
using System.Threading.Tasks;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud;

namespace NMS.GdaImplementation
{
    public class MongoAccess
    {
        private readonly string baseLogString;
        private readonly IMongoDatabase db;

        #region Private Properties
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public MongoAccess()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            try
            {
                MongoClient dbClient = new MongoClient(Config.GetInstance().DbConnectionString);
                db = dbClient.GetDatabase(MongoStrings.NMSDatabase);
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} Ctor => Error on database Init.", e);
            }

            InitializeBsonSerializer();
        }

        private void InitializeBsonSerializer()
        {
            try
            {
                BsonSerializer.RegisterSerializer(new EnumSerializer<DMSType>(BsonType.String));
                BsonSerializer.RegisterSerializer(new Int64Serializer(BsonType.String));

                BsonClassMap.RegisterClassMap<BaseVoltage>();
                BsonClassMap.RegisterClassMap<Terminal>();
                BsonClassMap.RegisterClassMap<ConnectivityNode>();
                BsonClassMap.RegisterClassMap<PowerTransformer>();
                BsonClassMap.RegisterClassMap<EnergySource>();
                BsonClassMap.RegisterClassMap<EnergyConsumer>();
                BsonClassMap.RegisterClassMap<TransformerWinding>();
                BsonClassMap.RegisterClassMap<Fuse>();
                BsonClassMap.RegisterClassMap<Disconnector>();
                BsonClassMap.RegisterClassMap<Breaker>();
                BsonClassMap.RegisterClassMap<LoadBreakSwitch>();
                BsonClassMap.RegisterClassMap<ACLineSegment>();
                BsonClassMap.RegisterClassMap<Discrete>();
                BsonClassMap.RegisterClassMap<Analog>();
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} InitializeBsonSerializer => Exception: {e.Message}.";
                Logger.LogError(errorMessage, e);
            }
        }

        public void SaveNetworkModel(Dictionary<DMSType, Container> networkModel, long version)
        {
            long latestNetworkModelVersion = GetLatestNetworkModelVersions();
            long latestDeltaVersion = GetLatestDeltaVersions();

            if (latestNetworkModelVersion < 0 || latestDeltaVersion < 0)
            {
                string errorMessage = $"{baseLogString} SaveNetworkModel => latest version has a negative value.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            long latestVersion = latestDeltaVersion > latestNetworkModelVersion ? latestDeltaVersion : latestNetworkModelVersion;

            if (version <= latestVersion)
            {
                string warnMessage = $"{baseLogString} SaveNetworkModel => NetworkModel version is lower then the latest version. NetworkModel version: {version}, LatestVersion: {latestVersion}";
                Logger.LogWarning(warnMessage);

                version = latestDeltaVersion + 1;

                warnMessage = $"{baseLogString} SaveNetworkModel => NetworkModel version is set to a new value: {version}";
                Logger.LogWarning(warnMessage);
            }

            var latestNetworkModelVersionDocument = new LatestVersionsDocument 
            {
                Id = MongoStrings.LatestVersions_NetworkModelVersion,
                Version = version,
            };

            var latestVersionsCollection = db.GetCollection<LatestVersionsDocument>(MongoStrings.LatestVersionsCollection);
            latestVersionsCollection.ReplaceOne(new BsonDocument("_id", MongoStrings.LatestVersions_NetworkModelVersion), latestNetworkModelVersionDocument, new ReplaceOptions { IsUpsert = true });

            var networkModelDocument = new NetworkModelsDocument()
            {
                Id = version,
                NetworkModel = networkModel,
            };

            var networkModelsCollection = db.GetCollection<NetworkModelsDocument>(MongoStrings.NetworkModelsCollection);
            networkModelsCollection.InsertOne(networkModelDocument);
        }

        public void SaveDelta(Delta delta)
        {
            long latestNetworkModelVersion = GetLatestNetworkModelVersions();
            long latestDeltaVersion = GetLatestDeltaVersions();

            if (latestNetworkModelVersion < 0 || latestDeltaVersion < 0)
            {
                string errorMessage = $"{baseLogString} SaveDelta => latest version has a negative value.";
                throw new Exception(errorMessage);
            }

            long latestVersion = latestDeltaVersion > latestNetworkModelVersion ? latestDeltaVersion : latestNetworkModelVersion;

            if (delta.Id <= latestVersion)
            {
                string warnMessage = $"{baseLogString} SaveDelta => Delta version is lower then the latest version. NetworkModel version: {delta.Id}, LatestVersion: {latestVersion}";
                Logger.LogWarning(warnMessage);

                delta.Id = latestDeltaVersion + 1;

                warnMessage = $"{baseLogString} SaveDelta => Delta version is set to a new value: {delta.Id}";
                Logger.LogWarning(warnMessage);
            }

            var newLatestDeltaVersionDocument = new LatestVersionsDocument()
            {
                Id = MongoStrings.LatestVersions_DeltaVersion,
                Version = delta.Id,
            };

            try
            { 
                var latestVersionsCollection = db.GetCollection<LatestVersionsDocument>(MongoStrings.LatestVersionsCollection);
                latestVersionsCollection.ReplaceOne(new BsonDocument("_id", MongoStrings.LatestVersions_DeltaVersion), newLatestDeltaVersionDocument, new ReplaceOptions { IsUpsert = true });

                var deltasCollection = db.GetCollection<Delta>(MongoStrings.DeltasCollection);
                delta.DeltaOrigin = DeltaOriginType.DatabaseDelta;
                deltasCollection.InsertOne(delta);
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} SaveDelta => Error: {e.Message}.";
                Logger.LogError(errorMessage, e);
            }
        }

        public long GetLatestNetworkModelVersions(bool isRetry = false)
        {
            long networkModelVersion = 0;

            try
            {
                var latestVersionsCollection = db.GetCollection<LatestVersionsDocument>(MongoStrings.LatestVersionsCollection);
                var networkModelVersionFilter = Builders<LatestVersionsDocument>.Filter.Eq("_id", "networkModelVersion");

                var findNetworkModelVersionResult = latestVersionsCollection.Find(networkModelVersionFilter);
                if (findNetworkModelVersionResult.CountDocuments() > 0)
                {
                    networkModelVersion = findNetworkModelVersionResult.First().Version;
                }
            }
            catch (TimeoutException toe)
            {
                string errorMessage = $"{baseLogString} GetLatestNetworkModelVersions => Exception: {toe.Message}";
                Logger.LogError(errorMessage, toe);

                if (!isRetry)
                {
                    Task.Delay(60000).Wait();
                    GetLatestNetworkModelVersions(true);
                }
            }

            return networkModelVersion;
        }

        public long GetLatestDeltaVersions(bool isRetry = false)
        {
            long latestDeltaVersion = 0;

            try
            {
                var latestVersionsCollection = db.GetCollection<LatestVersionsDocument>(MongoStrings.LatestVersionsCollection);
                var deltaVersionFilter = Builders<LatestVersionsDocument>.Filter.Eq("_id", MongoStrings.LatestVersions_DeltaVersion);

                var findDeltaVersionResult = latestVersionsCollection.Find(deltaVersionFilter);
                if (findDeltaVersionResult.CountDocuments() > 0)
                {
                    latestDeltaVersion = findDeltaVersionResult.First().Version;
                }
            }
            catch (TimeoutException toe)
            {
                string errorMessage = $"{baseLogString} GetLatestDeltaVersions => Exception: {toe.Message}";
                Logger.LogError(errorMessage, toe);

                if (!isRetry)
                {
                    Task.Delay(60000).Wait();
                    GetLatestDeltaVersions(true);
                }
            }

            return latestDeltaVersion;
        }

        public Dictionary<DMSType, Container> GetNetworkModel(long networkModelVersion)
        {
            var networkDataModel = new Dictionary<DMSType, Container>();
            var networkModelFilter = Builders<NetworkModelsDocument>.Filter.Eq("_id", networkModelVersion);

            if (networkModelVersion > 0)
            {
                var networkModelsCollection = db.GetCollection<NetworkModelsDocument>(MongoStrings.NetworkModelsCollection);

                if (networkModelsCollection.CountDocuments(networkModelFilter) > 0)
                {
                    networkDataModel = networkModelsCollection.Find(networkModelFilter).First().NetworkModel;
                }
            }

            return networkDataModel;
        }

        public List<Delta> GetAllDeltasFromVersionRange(long firstDelta, long lastDelta)
        {
            var deltas = new List<Delta>();
            var deltasCollection = db.GetCollection<Delta>(MongoStrings.DeltasCollection);

            for (long currentDeltaVersion = firstDelta; currentDeltaVersion <= lastDelta; currentDeltaVersion++)
            {
                var deltaFilter = Builders<Delta>.Filter.Eq("_id", currentDeltaVersion);
                deltas.Add(deltasCollection.Find(deltaFilter).First());
            }

            return deltas;
        }
    }
}
