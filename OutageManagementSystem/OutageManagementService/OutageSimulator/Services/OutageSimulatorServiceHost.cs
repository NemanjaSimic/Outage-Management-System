using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OMS.OutageSimulator.Services
{

    public class OutageSimulatorServiceHost : IDisposable
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }


        private List<ServiceHost> hosts = null;

        public OutageSimulatorServiceHost()
        {
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
            }
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
                new ServiceHost(typeof(OutageSimulatorService)),
            };
        }

        private void StartHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Outage Simulator Service can not be opend because they are not initialized.");
            }

            string message;
            StringBuilder sb = new StringBuilder();

            foreach (ServiceHost host in hosts)
            {
                host.Open();

                message = string.Format("The WCF service {0} is ready.", host.Description.Name);
                sb.AppendLine(message);

                message = "Endpoints:";
                sb.AppendLine(message);

                foreach (Uri uri in host.BaseAddresses)
                {
                    sb.AppendLine(uri.ToString());
                }

                sb.AppendLine();
            }

            Logger.LogInfo(sb.ToString());

            message = "Trace level: LEVEL NOT SPECIFIED!";
            Logger.LogWarn(message);


            message = "Outage Simulator Service is started.";
            Logger.LogInfo(message);
        }

        private void CloseHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Outage Simulator Service can not be closed because they are not initialized.");
            }

            foreach (ServiceHost host in hosts)
            {
                host.Close();
            }

            string message = "Outage Simulator Service is gracefully closed.";
            Logger.LogInfo(message);
        }
    }
}
