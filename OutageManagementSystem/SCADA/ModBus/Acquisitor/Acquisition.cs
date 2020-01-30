using Outage.Common;
using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Configuration;
using Outage.SCADA.SCADAData.Repository;
using System;
using System.Collections.Generic;
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

        private readonly ISCADAConfigData scadaConfig;
        private readonly FunctionExecutor functionExecutor;
        private readonly SCADAModel scadaModel;
        
        private bool threadActiveSignal = true;
        private Thread acquisitionThread;

        public Acquisition(FunctionExecutor functionExecutor, SCADAModel scadaModel)
        {
            this.functionExecutor = functionExecutor;
            this.scadaModel = scadaModel;
            this.scadaConfig = SCADAConfigData.Instance;

            InitializeAcquisitionThread();
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
            //ushort length = 6;
            //ushort quantity;

            try
            {
                Logger.LogInfo("AcquisitionThread is started.");

                while (threadActiveSignal)
                {
                    if (this.functionExecutor == null)
                    {
                        string message = $"Function Executor is null.";
                        Logger.LogError(message);
                        
                        Thread.Sleep(scadaConfig.Interval);
                        continue;
                    }

                    //MODEL UPDATE -> will swap incoming and current SCADAModel in commit step, so we have to save the reference locally
                    Dictionary<PointType, Dictionary<ushort, long>> currentAddressToGidMap = scadaModel.CurrentAddressToGidMap;

                    foreach (PointType pointType in currentAddressToGidMap.Keys)
                    {
                        ushort length = 6;  //expected by protocol
                        ushort address;
                        ushort quantity;
                        ModbusFunction modbusFunction;

                        if (pointType == PointType.DIGITAL_OUTPUT)
                        {
                            address = 1;
                            quantity = (ushort)currentAddressToGidMap.Count;

                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_COILS,
                                                                                                   address,
                                                                                                   quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }
                        //DIGITAL_INPUT
                        else if (pointType == PointType.DIGITAL_INPUT)
                        {
                            address = 1;
                            quantity = (ushort)currentAddressToGidMap.Count;

                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_DISCRETE_INPUTS,
                                                                                                   address,
                                                                                                   quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }
                        //ANALOG_OUTPUT
                        else if (pointType == PointType.ANALOG_OUTPUT)
                        {
                            address = 1;
                            quantity = (ushort)currentAddressToGidMap.Count;

                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_HOLDING_REGISTERS,
                                                                                                   address,
                                                                                                   quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }
                        //ANALOG_INPUT
                        else if (pointType == PointType.ANALOG_INPUT)
                        {
                            address = 1;
                            quantity = (ushort)currentAddressToGidMap.Count;

                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_INPUT_REGISTERS,
                                                                                                   address,
                                                                                                   quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }
                        else
                        {
                            modbusFunction = null;
                            string message = $"PointType value is invalid";
                            Logger.LogError(message);
                            continue;
                        }

                        if (this.functionExecutor.EnqueueCommand(modbusFunction))
                        {
                            Logger.LogDebug($"Modbus function enquided. Point type is {pointType}, quantity {quantity}.");
                        }
                    }

                    Thread.Sleep(scadaConfig.Interval);
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