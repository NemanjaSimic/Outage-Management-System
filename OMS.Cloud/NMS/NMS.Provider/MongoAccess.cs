using OMS.Cloud.NMS.GdaProvider.DbModel;
using Microsoft.ServiceFabric.Data.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Outage.Common;
using Outage.Common.GDA;
using Outage.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Cloud.NMS.GdaProvider
{
    public class MongoAccess
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private IMongoDatabase db;

        public MongoAccess()
        {
            try
            {
                MongoClient dbClient = new MongoClient(Config.GetInstance().DbConnectionString);
                db = dbClient.GetDatabase("NMSDatabase");
            }
            catch (Exception e)
            {
                Logger.LogError("Error on database Init.", e);
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
            catch (Exception)
            {
                //log...
            }
            
        }

        public Dictionary<DMSType, Container> GetLatesNetworkModel(long networkModelVersion)
        {
            Dictionary<DMSType, Container> networkDataModel = new Dictionary<DMSType, Container>();

            var networkModelFilter = Builders<NetworkDataModelDocument>.Filter.Eq("_id", networkModelVersion);
            if (networkModelVersion > 0)
            {
                var networkDataModelCollection = db.GetCollection<NetworkDataModelDocument>("networkModels");
                networkDataModel = networkDataModelCollection.Find(networkModelFilter).First().NetworkModel;
            }

            return networkDataModel;
        }

        public void SaveDelta(Delta delta)
        {
            long deltaVersion = 0, networkModelVersion = 0, newestVersion = 0;

            GetVersions(ref networkModelVersion, ref deltaVersion);

            newestVersion = deltaVersion > networkModelVersion ? deltaVersion : networkModelVersion;
            delta.Id = ++newestVersion;

            try
            {
                var counterCollection = db.GetCollection<ModelVersionDocument>("versions");
                counterCollection.ReplaceOne(new BsonDocument("_id", "deltaVersion"), new ModelVersionDocument { Id = "deltaVersion", Version = delta.Id }, new UpdateOptions { IsUpsert = true });
                
                var deltaCollection = db.GetCollection<Delta>("deltas");
                deltaCollection.InsertOne(delta);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error on database: {e.Message}.", e);
            }
        }

        public void SaveNetworkModel(Dictionary<DMSType, Container> networkDataModel)
        {
            long networkModelVersion = 0, deltaVersion = 0;

            GetVersions(ref networkModelVersion, ref deltaVersion);

            if ((networkModelVersion == 0 && deltaVersion == 0) || (networkModelVersion > deltaVersion)) //there is no model and deltas or model in use is already saved, so there is no need for datamodel storing
            {
                return;
            }
            else if (deltaVersion > networkModelVersion) //there is new deltas since startup, so store current dataModel
            {

                IMongoCollection<NetworkDataModelDocument> networkModelCollection = db.GetCollection<NetworkDataModelDocument>("networkModels");
                networkModelCollection.InsertOne(new NetworkDataModelDocument { Id = deltaVersion + 1, NetworkModel = networkDataModel });

                IMongoCollection<ModelVersionDocument> versionsCollection = db.GetCollection<ModelVersionDocument>("versions");
                versionsCollection.ReplaceOne(new BsonDocument("_id", "networkModelVersion"), new ModelVersionDocument { Id = "networkModelVersion", Version = deltaVersion + 1 }, new UpdateOptions { IsUpsert = true });
            }
            else
            {
                throw new Exception("SaveNetwrokModel error!");  //better message needed :((
            }
        }

        public void GetVersions(ref long networkModelVersion, ref long deltaVersion)
        {
            IMongoCollection<ModelVersionDocument> versionsCollection = db.GetCollection<ModelVersionDocument>("versions");

            var networkModelVersionFilter = Builders<ModelVersionDocument>.Filter.Eq("_id", "networkModelVersion");
            var deltaVersionFilter = Builders<ModelVersionDocument>.Filter.Eq("_id", "deltaVersion");

            if (versionsCollection.Find(networkModelVersionFilter).CountDocuments() > 0)
            {
                networkModelVersion = versionsCollection.Find(networkModelVersionFilter).First().Version;
            }

            if (versionsCollection.Find(deltaVersionFilter).CountDocuments() > 0)
            {
                deltaVersion = versionsCollection.Find(deltaVersionFilter).First().Version;
            }
        }

        public List<Delta> GetAllDeltas(long deltaVersion, long networkModelVersion)
        {

            List<Delta> deltasFromDb = new List<Delta>();

            var collection = db.GetCollection<Delta>("deltas");

            for (long deltaV = networkModelVersion + 1; deltaV <= deltaVersion; deltaV++)
            {
                var deltaFilter = Builders<Delta>.Filter.Eq("_id", deltaV);
                deltasFromDb.Add(collection.Find(deltaFilter).First());
            }

            return deltasFromDb;
        }
    }
}
