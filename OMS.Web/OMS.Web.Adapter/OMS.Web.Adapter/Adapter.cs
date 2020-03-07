using OMS.Web.Adapter.Outage;
using OMS.Web.Adapter.SCADA;
using OMS.Web.Adapter.Topology;
using OMS.Web.Common.Mappers;
using Outage.Common;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.PubSub;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace OMS.Web.Adapter
{
    public class Adapter : IDisposable
    {
        private ILogger _logger;

        protected ILogger Logger
        {
            get { return _logger ?? (_logger = LoggerWrapper.Instance); }
        }

        private ProxyFactory _proxyFactory = null;

        private List<ServiceHost> _hosts = null;
        private List<SubscriberProxy> _subscribers = null;

        public Adapter()
        {
            _proxyFactory = new ProxyFactory();

            InitializeSubscribers();
            InitializeHosts();
        }

        public void Start()
        {
            try
            {
                StartHosts();
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
            CloseSubscribers();
            GC.SuppressFinalize(this);
        }

        private void InitializeSubscribers()
        {
            _subscribers = new List<SubscriberProxy>();

            //TOPOLOGY_SUBSCRIBER
            TopologyNotification topologyNotifciation = new TopologyNotification("TOPOLOGY_ADAPTER_SUBSCRIBER", new GraphMapper());
            SubscriberProxy topologySubscriberProxy = _proxyFactory.CreateProxy<SubscriberProxy, ISubscriber>(topologyNotifciation, EndpointNames.SubscriberEndpoint);

            if(topologySubscriberProxy == null)
            {
                string message = "InitializeSubscribers() => SubscriberProxy[TOPOLOGY_SUBSCRIBER] is null.";
                Logger.LogError(message);
                Console.WriteLine(message);
            }

            try
            {
                topologySubscriberProxy.Subscribe(Topic.TOPOLOGY);
                _subscribers.Add(topologySubscriberProxy);
            }
            catch (Exception ex)
            {
                string message = $"Failed to subscribe to Topology Topics.";
                Logger.LogError(message, ex);
                Console.WriteLine(message);
            }

            //SCADA_SUBSCRIBER
            SCADANotification scadaNotification = new SCADANotification("SCADA_ADAPTER_SUBSCRIBER");
            SubscriberProxy scadaSubscriberProxy =  _proxyFactory.CreateProxy<SubscriberProxy, ISubscriber>(scadaNotification, EndpointNames.SubscriberEndpoint);

            if (scadaSubscriberProxy == null)
            {
                string message = "InitializeSubscribers() => SubscriberProxy[SCADA_ADAPTER_SUBSCRIBER] is null.";
                Logger.LogError(message);
                Console.WriteLine(message);
            }

            try
            {
                scadaSubscriberProxy.Subscribe(Topic.MEASUREMENT);
                _subscribers.Add(scadaSubscriberProxy);
            }
            catch (Exception ex)
            {
                string message = $"Failed to subscribe to SCADA Topics.";
                Logger.LogError(message, ex);
                Console.WriteLine(message);
            }


            //OUTAGE_SUBSCRIBER
            OutageNotification outageNotification = new OutageNotification("OUTAGE_ADAPTER_SUBSCRIBER", new OutageMapper(new ConsumerMapper(), new EquipmentMapper()));
            SubscriberProxy outageSubscriberProxy = _proxyFactory.CreateProxy<SubscriberProxy, ISubscriber>(outageNotification, EndpointNames.SubscriberEndpoint);

            if (outageSubscriberProxy == null)
            {
                string message = "InitializeSubscribers() => SubscriberProxy[OUTAGE_ADAPTER_SUBSCRIBER] is null.";
                Logger.LogError(message);
                Console.WriteLine(message);
            }

            try
            {
                outageSubscriberProxy.Subscribe(Topic.ACTIVE_OUTAGE);
                outageSubscriberProxy.Subscribe(Topic.ARCHIVED_OUTAGE);
                _subscribers.Add(outageSubscriberProxy);
            }
            catch (Exception ex)
            {
                string message = $"Failed to subscribe to Outage Topics.";
                Logger.LogError(message, ex);
                Console.WriteLine(message);
            }
        }

        private void InitializeHosts()
        {
            _hosts = new List<ServiceHost>
            {
                new ServiceHost(typeof(WebService.WebService)),
            };
        }

        private void StartHosts()
        {
            if (_hosts == null || _hosts.Count == 0)
            {
                throw new Exception("Adapter hosts can not be opend because they are not initialized.");
            }

            string message;
            StringBuilder sb = new StringBuilder();

            foreach (ServiceHost host in _hosts)
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

            message = "Trace level: LEVEL NOT SPECIFIED.";
            Console.WriteLine(message);
            Logger.LogWarn(message);


            message = "Adapter is started.";
            Console.WriteLine("\n{0}", message);
            Logger.LogInfo(message);
        }

        private void CloseHosts()
        {
            if (_hosts == null || _hosts.Count == 0)
            {
                throw new Exception("Adapter can not be closed because it is not initialized.");
            }

            foreach (ServiceHost host in _hosts)
            {
                host.Close();
            }

            string message = "Adapter's hosts are gracefully closed.";
            Logger.LogInfo(message);
            Console.WriteLine("\n\n{0}", message);
        }

        private void CloseSubscribers()
        {
            if (_subscribers == null || _subscribers.Count == 0)
            {
                throw new Exception("Adapter do not have any initialized subcribers.");
            }

            foreach (SubscriberProxy subscriber in _subscribers)
            {
                subscriber.Close();
            }

            string message = "Adapter's subscribers are gracefully closed.";
            Logger.LogInfo(message);
            Console.WriteLine("\n\n{0}", message);
        }
    }
}
