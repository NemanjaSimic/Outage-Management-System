using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.PubSub;
using OutageDatabase;
using OutageManagementService.Calling;
using OutageManagementService.DistribuedTransaction;
using OutageManagementService.Outage;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService
{
    public sealed class OutageManagementService : IDisposable
    {
        #region Private Fields

        private ILogger logger;
        private List<ServiceHost> hosts = null;
        private OutageModel outageModel;
        private SubscriberProxy subscriberProxy;
        private CallTracker callTracker;
        private ModelResourcesDesc modelResourcesDesc;
        private ProxyFactory proxyFactory;
        #endregion

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public OutageManagementService()
        {
            proxyFactory = new ProxyFactory();

            //TODO: Initialize what is needed
            //TODO: restauration of data...
            modelResourcesDesc = new ModelResourcesDesc();
            
            Task.Run(InitializeEnergyConsumers);

            outageModel = new OutageModel();
            OutageService.outageModel = outageModel;
            OutageTransactionActor.OutageModel = outageModel;
            OutageModelUpdateNotification.OutageModel = outageModel;

            callTracker = new CallTracker("CallTrackerSubscriber", outageModel);
            SubscribeOnEmailService();
            
            InitializeHosts();
        }

        #region GDAHelper
        public async Task<List<ResourceDescription>> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            int iteratorId;

            using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
            {
                if (gdaQueryProxy == null)
                {
                    string message = "GetExtentValues() => NetworkModelGDAProxy is null.";
                    Logger.LogError(message);
                    throw new NullReferenceException(message);
                }

                try
                {
                    iteratorId = gdaQueryProxy.GetExtentValues(entityType, propIds);
                }
                catch (Exception e)
                {
                    string message = $"Failed to get extent values for dms type {entityType}.";
                    Logger.LogError(message, e);
                    throw e;
                }
            }

            return await ProcessIterator(iteratorId);
        }

        private async Task<List<ResourceDescription>> ProcessIterator(int iteratorId)
        {
            //TODO: mozda vec ovde napakovati dictionary<long, rd> ?
            int resourcesLeft;
            int numberOfResources = 10000;
            List<ResourceDescription> resourceDescriptions;

            using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
            {
                if (gdaQueryProxy == null)
                {
                    string message = "ProcessIterator() => NetworkModelGDAProxy is null.";
                    Logger.LogError(message);
                    throw new NullReferenceException(message);
                }

                try
                {
                    resourcesLeft = gdaQueryProxy.IteratorResourcesTotal(iteratorId);
                    resourceDescriptions = new List<ResourceDescription>(resourcesLeft);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = gdaQueryProxy.IteratorNext(numberOfResources, iteratorId);
                        resourceDescriptions.AddRange(rds);

                        resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);
                    }

                    gdaQueryProxy.IteratorClose(iteratorId);
                }
                catch (Exception e)
                {
                    string message = $"Failed to retrieve all Resourse descriptions with iterator {iteratorId}.";
                    Logger.LogError(message, e);
                    throw e;
                }
            }

            return resourceDescriptions;
        }

        #endregion

        private void SubscribeOnEmailService()
        {
            ProxyFactory proxyFactory = new ProxyFactory();
            this.subscriberProxy = proxyFactory.CreateProxy<SubscriberProxy, ISubscriber>(callTracker, EndpointNames.SubscriberEndpoint);

            if (subscriberProxy == null)
            {
                string message = "SubscribeOnEmailService() => SubscriberProxy is null.";
                Logger.LogError(message);
                throw new NullReferenceException(message);
            }

            subscriberProxy.Subscribe(Topic.OUTAGE_EMAIL);
        }

        private async Task InitializeEnergyConsumers()
        {
            using (OutageContext db = new OutageContext())
            {
                List<ResourceDescription> energyConsumers = await GetExtentValues(ModelCode.ENERGYCONSUMER, modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONSUMER));

                int i = 0; //TODO: delete, for first/last name placeholder

                foreach(ResourceDescription energyConsumer in energyConsumers)
                {
                    Consumer consumer = new Consumer
                    {
                        ConsumerId = energyConsumer.GetProperty(ModelCode.IDOBJ_GID).AsLong(),
                        ConsumerMRID = energyConsumer.GetProperty(ModelCode.IDOBJ_MRID).AsString(),
                        FirstName = $"FirstName{i}", //TODO: energyConsumer.GetProperty(ModelCode.ENERGYCONSUMER_FIRSTNAME).AsString(); 
                        LastName = $"LastName{i}"   //TODO: energyConsumer.GetProperty(ModelCode.ENERGYCONSUMER_LASTNAME).AsString();
                    };

                    i++;

                    db.Consumers.Add(consumer);
                    Logger.LogDebug($"Add consumer: {consumer.ConsumerMRID}");
                }

                db.SaveChanges();
                Logger.LogDebug("Init energy consumers: SaveChanges()");
            }

        }


        #region Public Members
        public void Start()
        {
            try
            {
                StartHosts();
                //TODO: Start what is needed
            }
            catch (Exception e)
            {
                Logger.LogError("Exception in Start()", e);
                Console.WriteLine(e.Message);
            }
        }

        public void Dispose()
        {
            CloseHosts();
            //TODO: Stop what is needed
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Members
        private void InitializeHosts()
        {
            hosts = new List<ServiceHost>()
            {
                new ServiceHost(typeof(OutageService)),
                new ServiceHost(typeof(OutageTransactionActor)),
                new ServiceHost(typeof(OutageModelUpdateNotification)),
            };
        }

        private void StartHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Outage Management Service hosts can not be opend because they are not initialized.");
            }

            string message;
            StringBuilder sb = new StringBuilder();

            foreach (ServiceHost host in hosts)
            {
                host.Open();

                message = string.Format("The WCF service {0} is ready.", host.Description.Name);
                Console.WriteLine(message);
                sb.AppendLine(message);

                message = "Endpoints:";
                Console.WriteLine(message);
                sb.AppendLine(message);

                foreach (Uri uri in host.BaseAddresses)
                {
                    Console.WriteLine(uri);
                    sb.AppendLine(uri.ToString());
                }

                Console.WriteLine("\n");
                sb.AppendLine();
            }

            Logger.LogInfo(sb.ToString());

            message = "Trace level: LEVEL NOT SPECIFIED!";
            Console.WriteLine(message);
            Logger.LogWarn(message);

            message = "The Outage Management Service is started.";
            Console.WriteLine("\n{0}", message);
            Logger.LogInfo(message);
        }
        
        private void CloseHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Outage Management Service hosts can not be closed because they are not initialized.");
            }

            foreach (ServiceHost host in hosts)
            {
                host.Close();
            }

            string message = "Outage Management Service is gracefully closed.";
            Logger.LogInfo(message);
            Console.WriteLine("\n\n{0}", message);
        }

        #endregion
    }
}
