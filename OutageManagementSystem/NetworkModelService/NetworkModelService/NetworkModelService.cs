using Outage.Common;
using Outage.NetworkModelService.DistributedTransaction;
using Outage.NetworkModelService.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.NetworkModelService
{
    public class NetworkModelService : IDisposable
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private NetworkModel networkModel = null;
        private List<ServiceHost> hosts = null;

        public NetworkModelService()
        {
            networkModel = new NetworkModel();
            GenericDataAccess.NetworkModel = networkModel;
            NMSTransactionActor.NetworkModel = networkModel;
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
            GC.SuppressFinalize(this);
        }

        private void InitializeHosts()
        {
            hosts = new List<ServiceHost>
            {
                new ServiceHost(typeof(GenericDataAccess)),
                new ServiceHost(typeof(NMSTransactionActor))
            };
        }

        private void StartHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Network Model Service hosts can not be opend because they are not initialized.");
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

            message = $"Database connection string: {Config.Instance.DbConnectionString}";
            Console.WriteLine(message);
            Logger.LogInfo(message);

            message = "Trace level: LEVEL NOT SPECIFIED.";
            Console.WriteLine(message);
            Logger.LogWarn(message);


            message = "Network Model Service is started.";
            Console.WriteLine("\n{0}", message);
            Logger.LogInfo(message);
        }

        private void CloseHosts()
        {
            networkModel.SaveNetworkModel();
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Network Model Services can not be closed because it is not initialized.");
            }

            foreach (ServiceHost host in hosts)
            {
                host.Close();
            }

            string message = "The Network Model Service is gracefully closed.";
            Logger.LogInfo(message);
            Console.WriteLine("\n\n{0}", message);
        }
    }
}
