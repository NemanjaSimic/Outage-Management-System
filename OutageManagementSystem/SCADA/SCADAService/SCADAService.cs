using EasyModbus;
using Outage.Common;
using Outage.SCADA.ModBus;
using Outage.SCADA.ModBus.Acquisitor;
using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Configuration;
using Outage.SCADA.SCADAData.Repository;
using Outage.SCADA.SCADAService.Command;
using Outage.SCADA.SCADAService.DistributedTransaction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace Outage.SCADA.SCADAService
{
    public class SCADAService : IDisposable
    {
        private ILogger logger = LoggerWrapper.Instance;

        private List<ServiceHost> hosts = null;
        private SCADAModel scadaModel = null;
        private Acquisition acquisition = null;
        //private ConfigWriter configWriter = null;
        private ISCADAConfigData configData = SCADAConfigData.Instance;

        public SCADAService()
        {
            scadaModel = SCADAModel.Instance;
            SCADAModelUpdateNotification.SCADAModel = scadaModel;
            SCADATransactionActor.SCADAModel = scadaModel;

            InitializeHosts();
        }

        #region Public Members
        public void Start()
        {
            ModbusSimulatorHandler.StartModbusSimulator();
            FunctionExecutor.Instance.StartExecutor();
            StartDataAcquisition();
            StartHosts();
        }

        public void Dispose()
        {
            ModbusSimulatorHandler.StopModbusSimulaotrs();
            CloseHosts();
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Private Members
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
                throw new Exception("SCADA Service hosts can not be opend because they are not initialized.");
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

            message = "Trace level: LEVEL NOT SPECIFIED!";
            Console.WriteLine(message);
            logger.LogWarn(message);

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

            FunctionExecutor.Instance.StopExecutor();

            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("SCADA Service hosts can not be closed because they are not initialized.");
            }

            foreach (ServiceHost host in hosts)
            {
                host.Close();
            }

            string message = "SCADA Service is gracefully closed.";
            logger.LogInfo(message);
            Console.WriteLine("\n\n{0}", message);
        }
        #endregion
    }
}