using Outage.Common;
using OutageManagementService.Calling;
using OutageManagementService.Outage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService
{
    public class OutageManagementService : IDisposable
    {
        #region Private Fields

        private ILogger logger;
        private List<ServiceHost> hosts = null;
        private OutageModel outageModel;
        private CallingService callingService;
        #endregion


        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public OutageManagementService()
        {
            //TODO: Initialize what is needed
            callingService = new CallingService("OutageService");
            callingService.outageModel = outageModel;

            outageModel = new OutageModel();
            InitializeHosts();

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
                new ServiceHost(typeof(OutageService))
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
