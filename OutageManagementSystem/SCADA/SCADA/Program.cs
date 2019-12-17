using SCADA_Common;
using System;
using System.Collections.ObjectModel;
using ModBus.PointModels;
using SCADA.Command;
using SCADA_Config_Data.Repository;
using SCADA_Config_Data.Configuration;
using System.Linq;

namespace SCADA
{
    class Program
    {
        public static ObservableCollection<BasePointItem> Points { get; set; }
        private static WcfService<ICommandService, CommandService> wcfService;

        static void Main(string[] args)
        {

            wcfService = new WcfService<ICommandService, CommandService>(DataModelRepository.Instance.ServiceAddress);
            Console.WriteLine(DataModelRepository.Instance.ImportModel());
            ConfigWriter configWriter = new ConfigWriter();
            configWriter.GenerateConfigFile();
            /*  wcfService.Open();
              Console.WriteLine("Press any key to terminate services..");
              Console.ReadKey();
              wcfService.Close();
              Console.WriteLine("Press any key to exit the application.. "); */
            CommandService commandService = new CommandService();
            commandService.RecvCommand(DataModelRepository.Instance.Points.Values.First().Gid, PointType.ANALOG_OUTPUT, 100);
            Console.ReadKey();

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
