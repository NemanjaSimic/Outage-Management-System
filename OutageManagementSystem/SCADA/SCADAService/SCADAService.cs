using EasyModbus;
using Outage.Common;
using Outage.SCADA.ModBus;
using Outage.SCADA.ModBus.Acquisitor;
using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.SCADA_Config_Data.Configuration;
using Outage.SCADA.SCADA_Config_Data.Repository;
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
        private ConfigWriter configWriter = null;
        private DataModelRepository repo = DataModelRepository.Instance;

        public SCADAService()
        {
            scadaModel = new SCADAModel();
            SCADAModelUpdateNotification.SCADAModel = scadaModel;
            SCADATransactionActor.SCADAModel = scadaModel;

            InitializeHosts();
        }

        public void Start()
        {
            InitializeModbusSimConfiguration();
            ModbusSimulatorHandler.StartModbusSimulator();

            FunctionExecutor.Instance.StartConnection();

            //TODO: config address and port
            ModbusClient modbusClient = new ModbusClient();
            StartDataAcquisition(modbusClient);

            StartHosts();
        }

        public void Dispose()
        {
            ModbusSimulatorHandler.StopModbusSimulaotrs();
            CloseHosts();
            GC.SuppressFinalize(this);
        }

        private void InitializeModbusSimConfiguration()
        {
            bool success = repo.ImportModel();

            if(success)
            {
                //todo: debug log

                configWriter = new ConfigWriter(repo.ConfigFileName, repo.Points.Values.ToList());
                configWriter.GenerateConfigFile();

                if (File.Exists(repo.CurrentConfigPath))
                {
                    File.Delete(repo.CurrentConfigPath);
                }

                File.Move(repo.ConfigFileName, repo.CurrentConfigPath);

                Console.WriteLine("ModbusSim Configuration file generated SUCCESSFULLY.");
            }
            else
            {
                //todo: debug log
                Console.WriteLine("ModbusSim Configuration file generated UNSUCCESSFULLY.");
                //toddo: retry logic
                throw new Exception("ImportModel failed.");
            }
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

        private void StartDataAcquisition(ModbusClient modbusClient)
        {
            acquisition = new Acquisition(modbusClient, FunctionExecutor.Instance);
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
    }
}