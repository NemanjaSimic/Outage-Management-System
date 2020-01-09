using EasyModbus;
using Outage.Common;
using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADACommon;
using System;
using System.Threading;

namespace Outage.SCADA.ModBus.Acquisitor
{
    public class Acquisition
    {
        ILogger logger = LoggerWrapper.Instance;

        private bool threadActiveSignal = true;
        private Thread acquisitionWorker;

        private FunctionExecutor commandExecutor; 
        //private SCADAModel

        public Acquisition(FunctionExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;

            this.InitializeAcquisitionThread();
        }

        private void InitializeAcquisitionThread()
        {
            this.acquisitionWorker = new Thread(Acquire)
            {
                Name = "Acquisition thread"
            };

            logger.LogDebug("InitializeAcquisitionThread is initialized.");
        }

        public void StartAcquisitionThread()
        {
            threadActiveSignal = true;
            logger.LogDebug("threadActiveSignal is set on true.");
            acquisitionWorker.Start();
        }

        public void StopAcquisitionThread()
        {
            threadActiveSignal = false;
            logger.LogDebug("threadActiveSignal is set on false.");
        }

        private void Acquire()
        {
            ushort quantity = 1;
            ushort length = 6;

            try
            {
                logger.LogInfo("Acquisition thread is started.");

                //while (threadActiveSignal)
                //{
                //    Thread.Sleep(DataModelRepository.Instance.Interval);

                //    if (commandExecutor.connectionState == ConnectionState.CONNECTED)
                //    {
                //        foreach (var point in DataModelRepository.Instance.Points)
                //        {
                //            ushort address = point.Value.Address;
                //            ModbusFunction modbusFunction = null;

                //            //DIGITAL_OUTPUT
                //            if (point.Value.RegistarType == PointType.DIGITAL_OUTPUT)
                //            {
                //                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                //                                                                                       (byte)ModbusFunctionCode.READ_COILS,
                //                                                                                       address, quantity);

                //                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                //            }
                //            //DIGITAL_INPUT
                //            else if (point.Value.RegistarType == PointType.DIGITAL_INPUT)
                //            {
                //                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                //                                                                                       (byte)ModbusFunctionCode.READ_DISCRETE_INPUTS,
                //                                                                                       address, quantity);

                //                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                //            }
                //            //ANALOG_OUTPUT
                //            else if (point.Value.RegistarType == PointType.ANALOG_OUTPUT)
                //            {
                //                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                //                                                                                       (byte)ModbusFunctionCode.READ_HOLDING_REGISTERS,
                //                                                                                       address, quantity);

                //                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                //            }
                //            //ANALOG_INPUT
                //            else if (point.Value.RegistarType == PointType.ANALOG_INPUT)
                //            {
                //                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                //                                                                                       (byte)ModbusFunctionCode.READ_INPUT_REGISTERS,
                //                                                                                       address, quantity);

                //                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                //            }
                //            else
                //            {
                //                throw new Exception("PointType value is invalid");
                //            }

                //            if (modbusFunction != null)
                //            {
                //                this.commandExecutor.EnqueueCommand(modbusFunction);
                //                logger.LogDebug($"Modbus function enquided. Point type is {point.Value.RegistarType}");
                //            }

                //            //PODESAVANJE ALARMA
                //            point.Value.SetAlarms();
                //            logger.LogInfo("Alarm for item " + point.Value.Gid + " is set to " + point.Value.Alarm.ToString());
                //        }
                //    }

                //    logger.LogInfo("Acquisition thread is stopped.");
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                logger.LogError($"{e.Message}", e);
            }
        }
    }
}