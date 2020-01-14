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
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private bool threadActiveSignal = true;
        private Thread acquisitionThread;

        private ISCADAConfigData scadaConfig;
        private FunctionExecutor functionExecutor;

        public SCADAModel SCADAModel { get; protected set; }

        public Acquisition()
        {
            this.scadaConfig = SCADAConfigData.Instance;
            this.functionExecutor = FunctionExecutor.Instance;
            SCADAModel = SCADAModel.Instance;

            this.InitializeAcquisitionThread();
        }

        private void InitializeAcquisitionThread()
        {
            this.acquisitionThread = new Thread(AcquisitionThread)
            {
                Name = "AcquisitionThread"
            };

            Logger.LogDebug("InitializeAcquisitionThread is initialized.");
        }

        public void StartAcquisitionThread()
        {
            threadActiveSignal = true;
            Logger.LogDebug("threadActiveSignal is set on true.");
            acquisitionThread.Start();
        }

        public void StopAcquisitionThread()
        {
            threadActiveSignal = false;
            Logger.LogDebug("threadActiveSignal is set on false.");
        }

        private void AcquisitionThread()
        {
            ushort quantity = 1;
            ushort length = 6;

            try
            {
                Logger.LogInfo("AcquisitionThread is started.");

                while (threadActiveSignal)
                {
                    Thread.Sleep(scadaConfig.Interval);

                    if (functionExecutor.ModbusClient.Connected)
                    {
                        foreach (ISCADAModelPointItem pointItem in SCADAModel.CurrentScadaModel.Values)
                        {
                            ushort address = pointItem.Address;
                            ModbusFunction modbusFunction;

                            //DIGITAL_OUTPUT
                            if (pointItem.RegisterType == PointType.DIGITAL_OUTPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_COILS,
                                                                                                       address, quantity);

                                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                            }
                            //DIGITAL_INPUT
                            else if (pointItem.RegisterType == PointType.DIGITAL_INPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_DISCRETE_INPUTS,
                                                                                                       address, quantity);

                                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                            }
                            //ANALOG_OUTPUT
                            else if (pointItem.RegisterType == PointType.ANALOG_OUTPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_HOLDING_REGISTERS,
                                                                                                       address, quantity);

                                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                            }
                            //ANALOG_INPUT
                            else if (pointItem.RegisterType == PointType.ANALOG_INPUT)
                            {
                                ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                       (byte)ModbusFunctionCode.READ_INPUT_REGISTERS,
                                                                                                       address, quantity);

                                modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                            }
                            else
                            {
                                modbusFunction = null;
                                string message = $"PointType value is invalid";
                                Logger.LogError(message);
                            }

                            if (modbusFunction != null)
                            {
                                if (this.functionExecutor.EnqueueCommand(modbusFunction))
                                {
                                    Logger.LogDebug($"Modbus function enquided. Point type is {pointItem.RegisterType}");
                                }
                            }
                        }
                    }
                }

                Logger.LogInfo("AcquisitionThread is stopped.");
            }
            catch (Exception e)
            {
                string message = "Exception caught in AcquisitionThread.";
                Logger.LogError(message, e);
            }
        }
    }
}