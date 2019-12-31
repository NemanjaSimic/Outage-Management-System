﻿using Outage.Common;
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

        ILogger logger = LoggerWrapper.Instance;
        private Thread acquisitionWorker;
        private bool threadActiveSignal = true;
        private FunctionExecutor commandExecutor = FunctionExecutor.Instance;
        public Acquisition()
        {
            this.InitializeAcquisitionThread();
        }



        private void InitializeAcquisitionThread()
        {
            this.acquisitionWorker = new Thread(Acquire);
            this.acquisitionWorker.Name = "Acquisition thread";
        }

        public void StartAcquisitionThread()
        {
            threadActiveSignal = true;
            acquisitionWorker.Start();
        }

        public void StopAcquisitionThread()
        {
            threadActiveSignal = false;
        }

        private void Acquire()
        {
            ushort quantity = 1;
            ushort length = 6;

            try
            {
                while (threadActiveSignal)
                {
                    Thread.Sleep(DataModelRepository.Instance.Interval);
                    if (commandExecutor._connectionState == ConnectionState.CONNECTED)
                    {
                        foreach (var point in DataModelRepository.Instance.Points)
                        {
                            ushort address = point.Value.Address;

                            //DIGITALNI IZLAZI
                            if (point.Value.RegistarType == PointType.DIGITAL_OUTPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_COILS,
                                                                                                       address, quantity);

                                ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_read);
                                this.commandExecutor.EnqueueCommand(fn);
                            }
                            //DIGITALNI ULAZI
                            else if (point.Value.RegistarType == PointType.DIGITAL_INPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_DISCRETE_INPUTS,
                                                                                                       address, quantity);

                                ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_read);
                                this.commandExecutor.EnqueueCommand(fn);
                            }
                            //ANALOGNI IZLAZI
                            else if (point.Value.RegistarType == PointType.ANALOG_OUTPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_HOLDING_REGISTERS,
                                                                                                       address, quantity);

                                ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_read);
                                this.commandExecutor.EnqueueCommand(fn);
                            }
                            //ANALOGNI ULAZI
                            else if (point.Value.RegistarType == PointType.ANALOG_INPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_INPUT_REGISTERS,
                                                                                                       address, quantity);

                                ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_read);
                                this.commandExecutor.EnqueueCommand(fn);
                            }
                            else
                            {
                                throw new Exception("PointType value is invalid");
                            }

                            //PODESAVANJE ALARMA
                            point.Value.SetAlarms();
                            logger.LogInfo("Alarm for item " + point.Value.Gid + " is set to " + point.Value.Alarm.ToString());



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