using CalculationEngineService.DistributedTransaction;
using CECommon.Interfaces;
using CECommon.Providers;
using NetworkModelServiceFunctions;
using Outage.Common;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceProxies.PubSub;
using SCADACommanding;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using Topology;
using TopologyBuilder;

namespace CalculationEngineService
{
    public class CalculationEngineService : IDisposable
    {
        private ILogger logger;
        private ISubscriber proxy;
        private IModelTopologyServis modelTopologyServis;
        private IWebTopologyBuilder webTopologyBuilder;
        private IModelManager modelManager;
        private ITopologyBuilder topologyBuilder;
        private ICacheProvider cacheProvider;
        
        private SCADAResultHandler sCADAResultProvider;
        private TopologyProvider topologyProvider;
        private ModelProvider modelProvider;
        private WebTopologyModelProvider webTopologyModelProvider;
        private TopologyPublisher topologyPublisher;
        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private List<ServiceHost> hosts = null;

        public CalculationEngineService()
        {
            topologyBuilder = new GraphBuilder();
            modelTopologyServis = new TopologyManager(topologyBuilder);
            webTopologyBuilder = new WebTopologyBuilder();
            modelManager = new NMSManager();

            sCADAResultProvider = new SCADAResultHandler();
            cacheProvider = new CacheProvider();
            modelProvider = new ModelProvider(modelManager);
            topologyProvider = new TopologyProvider(modelTopologyServis);
            webTopologyModelProvider = new WebTopologyModelProvider(webTopologyBuilder);
            topologyPublisher = new TopologyPublisher();
            InitializeHosts();
        }

        public void Start()
        {
            try
            {
                StartHosts();
                SubscribeToSCADA();
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
            GC.SuppressFinalize(this);
        }

        private void InitializeHosts()
        {
            hosts = new List<ServiceHost>
            {
                new ServiceHost(typeof(CEModelUpdateNotification)),
                new ServiceHost(typeof(CETransactionActor)),
                new ServiceHost(typeof(TopologyService)),
                new ServiceHost(typeof(SCADACommandingService))
            };
        }

        private void StartHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Calculation Engine Service hosts can not be opend because they are not initialized.");
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

            message = string.Format("Trace level: LEVEL NOT SPECIFIED!");
            Console.WriteLine(message);
            Logger.LogWarn(message);


            message = "Calculation Engine is started.";
            Console.WriteLine("\n{0}", message);
            Logger.LogInfo(message);
        }

        private void CloseHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Calculation Engine Service hosts can not be closed because they are not initialized.");
            }

            foreach (ServiceHost host in hosts)
            {
                host.Close();
            }

            string message = "Calculation Engine Service is gracefully closed.";
            Logger.LogInfo(message);
            Console.WriteLine("\n\n{0}", message);
        }

        private void SubscribeToSCADA()
        {
            Logger.LogDebug("Subcribing on SCADA measurements.");
            proxy = new SubscriberProxy(new SCADASubscriber(), EndpointNames.SubscriberEndpoint);
            proxy.Subscribe(Topic.MEASUREMENT);
            proxy.Subscribe(Topic.SWITCH_STATUS);
        }
    }
}
