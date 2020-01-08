using Outage.Common;
using Outage.SCADA.ModBus.Acquisitor;
using Outage.SCADA.ModBus.PointModels;
using Outage.SCADA.SCADA_Config_Data.Configuration;
using Outage.SCADA.SCADA_Config_Data.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Outage.SCADA.SCADA
{
    [Obsolete]
    internal class Program
    {
        public static ObservableCollection<BasePointItem> Points { get; set; }
        //private static WcfService<ICommandService, CommandService> wcfService;

        private static void Main(string[] args)
        {
            //wcfService = new WcfService<ICommandService, CommandService>(DataModelRepository.Instance.ServiceAddress);
            Console.WriteLine(DataModelRepository.Instance.ImportModel());
            ConfigWriter configWriter = new ConfigWriter(DataModelRepository.Instance.ConfigFileName, DataModelRepository.Instance.Points.Values.ToList());

            configWriter.GenerateConfigFile();

            Acquisition A = new Acquisition();
            //  wcfService.Open();
            //  Console.WriteLine("Press any key to terminate services..");
            //  Console.ReadKey();
            //  wcfService.Close();
            //  Console.WriteLine("Press any key to exit the application.. ");
            //CommandService commandService = new CommandService();
            //commandService.RecvCommand(DataModelRepository.Instance.Points.Values.First().Gid, PointType.ANALOG_OUTPUT, 100);

            ILogger logger = LoggerWrapper.Instance;

            try
            {
                string message = "Starting SCADA Service...";
                logger.LogInfo(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine("\n{0}\n", message);

                using (SCADA_Service.SCADAService scadaService = new SCADA_Service.SCADAService())
                {
                    scadaService.Start();

                    message = "Press <Enter> to stop the service.";
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                    Console.WriteLine(message);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("SCADAService failed.");
                Console.WriteLine(ex.StackTrace);
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, "SCADAService failed.");
                CommonTrace.WriteTrace(CommonTrace.TraceError, ex.StackTrace);
                logger.LogError($"SCADAService failed.{Environment.NewLine}Message: {ex.Message} ", ex);
                Console.ReadLine();
            }

            // PREPRAVITI DA NE BUDE HARDCODE

            /*ConfigReader configReader = new ConfigReader();
            configReader.TcpPort = 502;
            configReader.unitAddress = 1;
            FunctionExecutor functionExecutor = new FunctionExecutor(configReader);
            InitializePointCollection(functionExecutor);

            ushort adresa1 = 00040;
            ushort kvantitet1 = 10;
            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters
            (6, (byte)ModbusFunctionCode.READ_COILS, adresa1, kvantitet1);
            */
            /* ConfigReader configReader = new ConfigReader();
             configReader.TcpPort = 502;
             configReader.unitAddress = 1;
             FunctionExecutor functionExecutor = new FunctionExecutor(configReader);

             ushort adresa1 = 00040;r
             ushort kvantitet1 = 10;
             ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters
             (6, (byte)ModbusFunctionCode.READ_COILS, adresa1, kvantitet1);

             ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_read);
             Console.ReadLine();

             functionExecutor.EnqueueCommand(fn);
             Console.ReadLine();

             Console.ReadLine();
            ConfigWriter configReader = new ConfigWriter();
            configReader.ImportModel();
             */
        }

        /* private static void InitializePointCollection(FunctionExecutor functionExecutor)
         {
             Points = new ObservableCollection<BasePointItem>();
             foreach (var c in ConfigWriter.Instance.GetConfigurationItems())
             {
                 for (int i = 0; i < c.NumberOfRegisters; i++)
                 {
                     BasePointItem pi = CreatePoint(c, i, functionExecutor);
                     if (pi != null)
                         Points.Add(pi);
                 }
             }
         }

         private static BasePointItem CreatePoint(ConfigItem c, int i, FunctionExecutor commandExecutor)
         {
             switch (c.RegistryType)
             {
                 case PointType.DIGITAL_INPUT:
                     return new DigitalInput(c.RegistryType, (ushort)(c.StartAddress + i), c.DefaultValue, commandExecutor, c.MinValue, c.MaxValue);

                 case PointType.DIGITAL_OUTPUT:
                     return new DigitalOutput(c.RegistryType, (ushort)(c.StartAddress + i), c.DefaultValue, commandExecutor, c.MinValue, c.MaxValue);

                 case PointType.ANALOG_INPUT:
                     return new AnalogInput(c.RegistryType, (ushort)(c.StartAddress + i), c.DefaultValue, commandExecutor, c.MinValue, c.MaxValue);

                 case PointType.ANALOG_OUTPUT:
                     return new AnalogOutput(c.RegistryType, (ushort)(c.StartAddress + i), c.DefaultValue, commandExecutor, c.MinValue, c.MaxValue);

                 default:
                     return null;
             }
         }
     */
    }
}