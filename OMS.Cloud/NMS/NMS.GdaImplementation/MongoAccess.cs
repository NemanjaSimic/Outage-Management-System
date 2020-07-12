﻿using MongoDB.Bson;
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
            catch (Exception e)
            {
                string errorMessage = $"InitializeBsonSerializer => Exception: {e.Message}.";
                Logger.LogError(errorMessage, e);
            }
        }

        public Dictionary<DMSType, Container> GetLatesNetworkModel(long networkModelVersion)
        {
            Dictionary<DMSType, Container> networkDataModel = new Dictionary<DMSType, Container>();

            var networkModelFilter = Builders<NetworkDataModelDocument>.Filter.Eq("_id", networkModelVersion);
            if (networkModelVersion > 0)
            {
                var networkDataModelCollection = db.GetCollection<NetworkDataModelDocument>("networkModels");
                
                if(networkDataModelCollection.CountDocuments(networkModelFilter) > 0)
                {
                    networkDataModel = networkDataModelCollection.Find(networkModelFilter).First().NetworkModel;
                }
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
                //counterCollection.ReplaceOne(new BsonDocument("_id", "deltaVersion"), new ModelVersionDocument { Id = "deltaVersion", Version = delta.Id }, new UpdateOptions { IsUpsert = true });
                counterCollection.ReplaceOne(new BsonDocument("_id", "deltaVersion"), new ModelVersionDocument { Id = "deltaVersion", Version = delta.Id }, new ReplaceOptions { IsUpsert = true });

                var deltaCollection = db.GetCollection<Delta>("deltas");
                deltaCollection.InsertOne(delta);
            }
            catch (Exception e)
            {
                string errorMessage = $"SaveDelta => Error on database: {e.Message}.";
                Logger.LogError(errorMessage, e);
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
                //versionsCollection.ReplaceOne(new BsonDocument("_id", "networkModelVersion"), new ModelVersionDocument { Id = "networkModelVersion", Version = deltaVersion + 1 }, new UpdateOptions { IsUpsert = true });
                versionsCollection.ReplaceOne(new BsonDocument("_id", "networkModelVersion"), new ModelVersionDocument { Id = "networkModelVersion", Version = deltaVersion + 1 }, new ReplaceOptions { IsUpsert = true });
            }
            else
            {
                throw new Exception("SaveNetwrokModel error!");  //better message needed :((
            }
        }

        public void GetVersions(ref long networkModelVersion, ref long deltaVersion, bool isRetry=false)
        {
            try
            {
                //TODO: bug fix, runtime faliure
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
            catch (TimeoutException toe)
            {
                string errorMessage = $"GetVersions => Exception: {toe.Message}";
                Logger.LogError(errorMessage, toe);

                if (!isRetry)
                {
                    Task.Delay(60000).Wait();
                    GetVersions(ref networkModelVersion, ref deltaVersion, true);
                }
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