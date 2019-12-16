﻿using Outage.Common;
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
        private NetworkModel networkModel = null;
        private List<ServiceHost> hosts = null;

        public NetworkModelService()
        {
            networkModel = new NetworkModel();
            GenericDataAccess.NetworkModel = networkModel;
            ResourceIterator.NetworkModel = networkModel;
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
                new ServiceHost(typeof(GenericDataAccess))
            };
        }

        private void StartHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Network Model Services can not be opend because they are not initialized.");
            }

            string message = string.Empty;
            foreach (ServiceHost host in hosts)
            {
                host.Open();

                message = string.Format("The WCF service {0} is ready.", host.Description.Name);
                Console.WriteLine(message);
                LoggerWrapper.Instance.LogInfo(message);

                message = "Endpoints:";
                Console.WriteLine(message);
                LoggerWrapper.Instance.LogInfo(message);

                foreach (Uri uri in host.BaseAddresses)
                {
                    Console.WriteLine(uri);
                    LoggerWrapper.Instance.LogInfo(uri.ToString());
                }

                Console.WriteLine("\n");
            }

            message = string.Format("Connection string: {0}", Config.Instance.ConnectionString);
            Console.WriteLine(message);
            LoggerWrapper.Instance.LogInfo(message);

            message = string.Format("Trace level: {0}", CommonTrace.TraceLevel);
            Console.WriteLine(message);
            LoggerWrapper.Instance.LogInfo(message);


            message = "The Network Model Service is started.";
            Console.WriteLine("\n{0}", message);
            LoggerWrapper.Instance.LogInfo(message);
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

            string message = "The Network Model Service are gracefully closed.";
            LoggerWrapper.Instance.LogInfo(message);
            Console.WriteLine("\n\n{0}", message);
        }
    }
}