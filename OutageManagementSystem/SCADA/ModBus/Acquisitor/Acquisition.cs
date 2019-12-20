using SCADA_Common;
using ModBus.Connection;
using ModBus.FunctionParameters;
using ModBus.ModbusFuntions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SCADA_Config_Data.Repository;
using SCADA_Config_Data.Configuration;

namespace ModBus.Acquisitor
{
    public class Acquisition
    {

        private FunctionExecutor commandExecutor = new FunctionExecutor(DataModelRepository.Instance.TcpPort);
        private Thread acquisitionWorker;

        public Acquisition()
        {

            this.InitializeAcquisitionThread();
            this.StartAcquisitionThread();
        }


        private void InitializeAcquisitionThread()
        {
            this.acquisitionWorker = new Thread(Acquire);
            this.acquisitionWorker.Name = "Acquisition thread";
        }

        private void StartAcquisitionThread()
        {
            acquisitionWorker.Start();
        }




        public Acquisition(FunctionExecutor fe)
        {
            commandExecutor = fe;
        }

        public void Acquire()
        {
            try
            {
                while (1 > 0)
                {

                    Thread.Sleep(DataModelRepository.Instance.Interval);

                    foreach (var point in DataModelRepository.Instance.Points)
                    {
                        ushort adresa = point.Value.Address;
                        ushort kvantitet = 1;

                        //DIGITALNI IZLAZI
                        if (point.Value.RegistarType == PointType.DIGITAL_OUTPUT)
                        {
                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters
                            (6, (byte)ModbusFunctionCode.READ_COILS, adresa, kvantitet);

                            ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_read);
                            this.commandExecutor.EnqueueCommand(fn);
                        }
                        //DIGITALNI ULAZI
                        else if (point.Value.RegistarType == PointType.DIGITAL_INPUT)
                        {
                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters
                            (6, (byte)ModbusFunctionCode.READ_DISCRETE_INPUTS, adresa, kvantitet);

                            ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_read);
                            this.commandExecutor.EnqueueCommand(fn);
                        }
                        //ANALOGNI IZLAZI
                        else if (point.Value.RegistarType == PointType.ANALOG_OUTPUT)
                        {
                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters
                            (6, (byte)ModbusFunctionCode.READ_HOLDING_REGISTERS, adresa, kvantitet);

                            ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_read);
                            this.commandExecutor.EnqueueCommand(fn);
                        }
                        //ANALOGNI ULAZI
                        else if (point.Value.RegistarType == PointType.ANALOG_INPUT)
                        {
                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters
                            (6, (byte)ModbusFunctionCode.READ_INPUT_REGISTERS, adresa, kvantitet);

                            ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_read);
                            this.commandExecutor.EnqueueCommand(fn);
                        }
                    }


                    //ushort adresa1 = 00040;
                    //ushort kvantitet1 = 1;
                    //ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters
                    //(6, (byte)ModbusFunctionCode.READ_COILS, adresa1, kvantitet1);

                    //ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_read);
                    //this.commandExecutor.EnqueueCommand(fn);
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}

