using CalculationEngineService.DistributedTransaction;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class CalculationEngineService : IDisposable
    {
        private ILogger logger = LoggerWrapper.Instance;

        private List<ServiceHost> hosts = null;

        public CalculationEngineService()
        {
            InitializeHosts();
        }

        public void Start()
        {
            StartHosts();
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
                new ServiceHost(typeof(CETransactionActor))
            };
        }

        private void StartHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Calculation Engine Service hosts can not be opend because they are not initialized.");
            }

            string message = string.Empty;
            foreach (ServiceHost host in hosts)
            {
                host.Open();

                message = string.Format("The WCF service {0} is ready.", host.Description.Name);
                Console.WriteLine(message);
                logger.LogInfo(message);

                message = "Endpoints:";
                Console.WriteLine(message);
                logger.LogInfo(message);

                foreach (Uri uri in host.BaseAddresses)
                {
                    Console.WriteLine(uri);
                    logger.LogInfo(uri.ToString());
                }

                Console.WriteLine("\n");
            }

            message = string.Format("Trace level: LEVEL NOT SPECIFIED!");
            Console.WriteLine(message);
            logger.LogWarn(message);


            message = "Calculation Engine is started.";
            Console.WriteLine("\n{0}", message);
            logger.LogInfo(message);
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
            logger.LogInfo(message);
            Console.WriteLine("\n\n{0}", message);
        }
    }
}
