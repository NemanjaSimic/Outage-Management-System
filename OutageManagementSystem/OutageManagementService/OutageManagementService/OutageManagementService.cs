using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.PubSub.OutageDataContract;
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
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OutageManagementService
{
    public class OutageManagementService : IDisposable
    {
        #region Private Fields

        private ILogger logger;
        private List<ServiceHost> hosts = null;
        private OutageModel outageModel;
        private ISubscriber subscriber;
        private CallTracker callTracker;
        private ModelResourcesDesc modelResourcesDesc;
        private ProxyFactory proxyFactory;
        #endregion

        //#region Proxies

        //private NetworkModelGDAProxy gdaQueryProxy = null;

        //private NetworkModelGDAProxy GetGdaQueryProxy()
        //{
        //    int numberOfTries = 0;
        //    int sleepInterval = 500;

        //    while (numberOfTries <= int.MaxValue)
        //    {
        //        try
        //        {
        //            if (gdaQueryProxy != null)
        //            {
        //                gdaQueryProxy.Abort();
        //                gdaQueryProxy = null;
        //            }

        //            gdaQueryProxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint);
        //            gdaQueryProxy.Open();

        //            if (gdaQueryProxy.State == CommunicationState.Opened)
        //            {
        //                break;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string message = $"Exception on NetworkModelGDAProxy initialization. Message: {ex.Message}";
        //            Logger.LogWarn(message, ex);
        //            gdaQueryProxy = null;
        //        }
        //        finally
        //        {
        //            numberOfTries++;
        //            Logger.LogDebug($"NetworkModelGDA: GdaQueryProxy getter, try number: {numberOfTries}.");

        //            if (numberOfTries >= 100)
        //            {
        //                sleepInterval = 1000;
        //            }

        //            Thread.Sleep(sleepInterval);
        //        }
        //    }

        //    return gdaQueryProxy;
        //}

        //#endregion Proxies


        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public OutageManagementService()
        {
            proxyFactory = new ProxyFactory();

            //TODO: Initialize what is needed
            //Delete database(TODO: restauration of data...)
            modelResourcesDesc = new ModelResourcesDesc();
            using (OutageContext db = new OutageContext())
            {
                db.DeleteAllData();
                InitializeEnergyConsumers(db);
            }
           
            outageModel = new OutageModel();
            OutageService.outageModel = outageModel;
            OutageTransactionActor.OutageModel = outageModel;
            OutageModelUpdateNotification.OutageModel = outageModel;
            callTracker = new CallTracker("CallTrackerSubscriber", outageModel);
            SubscribeOnEmailService();
            
            InitializeHosts();

        }

        #region GDAHelper
        private List<ResourceDescription> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            int iteratorId = 0;
            int numberOfTries = 0;
            while (numberOfTries < 5)
            {
                try
                {
                    numberOfTries++;
                    using (NetworkModelGDAProxy proxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
                    {
                        iteratorId = proxy.GetExtentValues(entityType, propIds);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to get extent values for entity type {entityType.ToString()}. Exception message: " + ex.Message);
                    logger.LogWarn($"Retrying to connect to NMSProxy. Number of tries: {numberOfTries}.");
                }
            }

            return ProcessIterator(iteratorId);
        }

        private List<ResourceDescription> ProcessIterator(int iteratorId)
        {
            //TODO: mozda vec ovde napakovati dictionary<long, rd> ?
            int numberOfResources = 10000, resourcesLeft = 0;
            List<ResourceDescription> resourceDescriptions = new List<ResourceDescription>();

            try
            {
                using (NetworkModelGDAProxy gdaProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
                {
                    if (gdaProxy != null)
                    {
                        do
                        {
                            List<ResourceDescription> rds = gdaProxy.IteratorNext(numberOfResources, iteratorId);
                            resourceDescriptions.AddRange(rds);

                            resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);

                        } while (resourcesLeft > 0);

                        gdaProxy.IteratorClose(iteratorId);
                    }
                    else
                    {
                        string message = "From method ProcessIterator(): NetworkModelGDAProxy is null.";
                        logger.LogError(message);
                        throw new NullReferenceException(message);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = $"Failed to retrieve all Resourse descriptions with iterator {iteratorId}. Exception message: " + ex.Message;
                logger.LogError(message);
            }
            return resourceDescriptions;
        }
        #endregion

        private void SubscribeOnEmailService()
        {
            //ProxyFactory proxyFactory = new ProxyFactory();
            //proxy = proxyFactory.CreatePRoxy<SubscriberProxy, ISubscriber>(new SCADASubscriber(), EndpointNames.SubscriberEndpoint);

            subscriber = new SubscriberProxy(callTracker, EndpointNames.SubscriberEndpoint);
            subscriber.Subscribe(Topic.OUTAGE_EMAIL);

        }

        private void InitializeEnergyConsumers(OutageContext db)
        {
            List<ResourceDescription> energyConsumers = GetExtentValues(ModelCode.ENERGYCONSUMER, modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONSUMER));

            int i = 0;
            foreach(ResourceDescription energyConsumer in energyConsumers)
            {
                Consumer consumer = new Consumer();
                consumer.ConsumerId = energyConsumer.GetProperty(ModelCode.IDOBJ_GID).AsLong();
                consumer.ConsumerMRID = energyConsumer.GetProperty(ModelCode.IDOBJ_MRID).AsString();
                consumer.FirstName = $"FirstName{i}";
                consumer.LastName = $"LastName{i}";
                i++;

                db.Consumers.Add(consumer);
            }

            db.SaveChanges();
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

        #endregion
    }
}
