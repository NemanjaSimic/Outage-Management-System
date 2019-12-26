using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADA_Common;
using Outage.SCADA.SCADA_Config_Data.Repository;
using System;
using System.Threading;

namespace Outage.SCADA.ModBus.Acquisitor
{
    public class Acquisition
    {
        //TODO: singleton, diskusija...

        private FunctionExecutor commandExecutor = new FunctionExecutor(DataModelRepository.Instance.TcpPort);
        private Thread acquisitionWorker;
        private bool threadActiveSignal = true;

        public Acquisition()
        {
            this.InitializeAcquisitionThread();
        }

        //TODO: WHY NEVER USED
        public Acquisition(FunctionExecutor fe)
        {
            commandExecutor = fe;
            this.InitializeAcquisitionThread();
        }

        private void InitializeAcquisitionThread()
        {
            this.acquisitionWorker = new Thread(Acquire)
            {
                Name = "Acquisition thread"
            };

            //TODO: debug thread init
        }

        public void StartAcquisitionThread()
        {
            threadActiveSignal = true;
            //TODO: log debug
            acquisitionWorker.Start();
        }

        public void StopAcquisitionThread()
        {
            threadActiveSignal = false;
            //TODO: log debug
        }

        private void Acquire()
        {
            ushort quantity = 1;
            ushort length = 6;

            try
            {
                //TODO: log info thread start

                while (threadActiveSignal)
                {
                    Thread.Sleep(DataModelRepository.Instance.Interval);

                    foreach (var point in DataModelRepository.Instance.Points)
                    {
                        ushort address = point.Value.Address;
                        ModbusFunction modbusFunction = null;

                        //DIGITAL_OUTPUT
                        if (point.Value.RegistarType == PointType.DIGITAL_OUTPUT)
                        {
                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_COILS,
                                                                                                   address, quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }
                        //DIGITAL_INPUT
                        else if (point.Value.RegistarType == PointType.DIGITAL_INPUT)
                        {
                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_DISCRETE_INPUTS,
                                                                                                   address, quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }
                        //ANALOG_OUTPUT
                        else if (point.Value.RegistarType == PointType.ANALOG_OUTPUT)
                        {
                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_HOLDING_REGISTERS,
                                                                                                   address, quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }
                        //ANALOG_INPUT
                        else if (point.Value.RegistarType == PointType.ANALOG_INPUT)
                        {
                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_INPUT_REGISTERS,
                                                                                                   address, quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }

                        if(modbusFunction != null)
                        {
                            this.commandExecutor.EnqueueCommand(modbusFunction);
                            //TODO: log debug
                        }
                    }
                }

                //TODO: log info thred stop
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //TODO: log err
            }
        }
    }
}