using Outage.Common;
using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Configuration;
using Outage.SCADA.SCADAData.Repository;
using System;
using System.Threading;

namespace Outage.SCADA.ModBus.Acquisitor
{
    public class Acquisition
    {
        private ILogger logger = LoggerWrapper.Instance;
        private ISCADAConfigData scadaConfig;

        private bool threadActiveSignal = true;
        private Thread acquisitionWorker;

        private FunctionExecutor commandExecutor;

        public SCADAModel SCADAModel { get; protected set; }

        public Acquisition()
        {
            this.scadaConfig = SCADAConfigData.Instance;
            this.commandExecutor = FunctionExecutor.Instance;
            SCADAModel = SCADAModel.Instance;

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

                while (threadActiveSignal)
                {
                    Thread.Sleep(scadaConfig.Interval);

                    if (commandExecutor.ModbusClient.Connected)
                    {
                        foreach (ISCADAModelPointItem pointItem in SCADAModel.CurrentScadaModel.Values)
                        {
                            ushort address = pointItem.Address;
                            ModbusFunction modbusFunction = null;

                            //DIGITAL_OUTPUT
                            if (pointItem.RegistarType == PointType.DIGITAL_OUTPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_COILS,
                                                                                                       address, quantity);

                                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                            }
                            //DIGITAL_INPUT
                            else if (pointItem.RegistarType == PointType.DIGITAL_INPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_DISCRETE_INPUTS,
                                                                                                       address, quantity);

                                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                            }
                            //ANALOG_OUTPUT
                            else if (pointItem.RegistarType == PointType.ANALOG_OUTPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_HOLDING_REGISTERS,
                                                                                                       address, quantity);

                                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                            }
                            //ANALOG_INPUT
                            else if (pointItem.RegistarType == PointType.ANALOG_INPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_INPUT_REGISTERS,
                                                                                                       address, quantity);

                                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                            }
                            else
                            {
                                throw new Exception("PointType value is invalid");
                            }

                            if (modbusFunction != null)
                            {
                                this.commandExecutor.EnqueueCommand(modbusFunction);
                                logger.LogDebug($"Modbus function enquided. Point type is {pointItem.RegistarType}");
                            }

                            //TOOD: PODESAVANJE ALARMA
                            //pointItem.SetAlarms();
                            //logger.LogInfo("Alarm for item " + pointItem.Gid + " is set to " + pointItem.Alarm.ToString());
                        }
                    }

                    logger.LogInfo("Acquisition thread is stopped.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                logger.LogError($"{e.Message}", e);
            }
        }
    }
}