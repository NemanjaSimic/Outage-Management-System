using Outage.Common;
using Outage.SCADA.ModBus.Acquisitor;
using Outage.SCADA.SCADAService.Command;
using Outage.SCADA.SCADAService.DistributedTransaction;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.SCADA.SCADAService
{
    public class SCADAService : IDisposable
    {
        private ILogger logger = LoggerWrapper.Instance;

        private List<ServiceHost> hosts = null;
        private SCADAModel scadaModel = null;
        private Acquisition acquisition = null;

        public SCADAService()
        {
            scadaModel = new SCADAModel();
            //CommandService.SCADAModel = scadaModel;
            SCADAModelUpdateNotification.SCADAModel = scadaModel;
            SCADATransactionActor.SCADAModel = scadaModel;

            InitializeHosts();
        }

        public void Start()
        {
            //TODO: init aquisition i slicno,,....
            StartDataAcquisition();


            StartHosts();
        }

        public void Dispose()
        {
            CloseHosts();
            GC.SuppressFinalize(this);
        }

        private void InitializeHosts()
        {
            hosts = new List<ServiceHost>()
            {
                new ServiceHost(typeof(CommandService)),
                new ServiceHost(typeof(SCADATransactionActor)),
                new ServiceHost(typeof(SCADAModelUpdateNotification))
            };
        }

        private void StartDataAcquisition()
        {
            acquisition = new Acquisition();
            acquisition.StartAcquisitionThread();
        }

        private void StartHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("SCADA Services can not be opend because they are not initialized.");
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

            message = string.Format("Trace level: {0}", CommonTrace.TraceLevel);
            Console.WriteLine(message);
            logger.LogInfo(message);

            message = "The SCADA" +
                " Service is started.";
            Console.WriteLine("\n{0}", message);
            logger.LogInfo(message);
        }

        private void CloseHosts()
        {
            if(acquisition != null)
            {
                acquisition.StopAcquisitionThread();
            }

            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Network Model Services can not be closed because it is not initialized.");
            }

            foreach (ServiceHost host in hosts)
            {
                host.Close();
            }

            string message = "The Network Model Service is gracefully closed.";
            logger.LogInfo(message);
            Console.WriteLine("\n\n{0}", message);
        }
    }
}