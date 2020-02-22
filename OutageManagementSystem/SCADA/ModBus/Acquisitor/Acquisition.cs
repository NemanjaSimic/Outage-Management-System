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
        private readonly IReadCommandEnqueuer readCommandEnqueuer;
        private readonly SCADAModel scadaModel;
        
        private bool threadActiveSignal = true;
        private Thread acquisitionThread;

        public Acquisition(IReadCommandEnqueuer readCommandEnqueuer, SCADAModel scadaModel)
        {
            this.readCommandEnqueuer = readCommandEnqueuer;
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
            try
            {
                Logger.LogInfo("AcquisitionThread is started.");

                while (threadActiveSignal)
                {
                    if (this.readCommandEnqueuer == null)
                    {
                        string message = $"Read command enqueuer is null.";
                        Logger.LogError(message);
                        
                        Thread.Sleep(scadaConfig.Interval);
                        continue;
                    }

                    //MODEL UPDATE -> will swap incoming and current SCADAModel in commit step, so we have to save the reference locally
                    Dictionary<PointType, Dictionary<ushort, long>> currentAddressToGidMap = scadaModel.CurrentAddressToGidMap;

                    foreach (PointType pointType in currentAddressToGidMap.Keys)
                    {
                        ushort length = 6;  //expected by protocol
                        ushort startAddress = 1;
                        ushort quantity;

                        ModbusFunction modbusFunction;

                        if (pointType == PointType.DIGITAL_OUTPUT)
                        {
                            quantity = (ushort)currentAddressToGidMap[PointType.DIGITAL_OUTPUT].Count;

                            if (quantity == 0)
                            {
                                continue;
                            }

                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_COILS,
                                                                                                   startAddress,
                                                                                                   quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }
                        //DIGITAL_INPUT
                        else if (pointType == PointType.DIGITAL_INPUT)
                        {
                            quantity = (ushort)currentAddressToGidMap[PointType.DIGITAL_INPUT].Count;

                            if (quantity == 0)
                            {
                                continue;
                            }

                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_DISCRETE_INPUTS,
                                                                                                   startAddress,
                                                                                                   quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }
                        //ANALOG_OUTPUT
                        else if (pointType == PointType.ANALOG_OUTPUT)
                        {
                            quantity = (ushort)currentAddressToGidMap[PointType.ANALOG_OUTPUT].Count;

                            if (quantity == 0)
                            {
                                continue;
                            }

                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_HOLDING_REGISTERS,
                                                                                                   startAddress,
                                                                                                   quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }
                        //ANALOG_INPUT
                        else if (pointType == PointType.ANALOG_INPUT)
                        {
                            quantity = (ushort)currentAddressToGidMap[PointType.ANALOG_INPUT].Count;

                            if(quantity == 0)
                            {
                                continue;
                            }

                            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length,
                                                                                                   (byte)ModbusFunctionCode.READ_INPUT_REGISTERS,
                                                                                                   startAddress,
                                                                                                   quantity);
                            modbusFunction = FunctionFactory.CreateModbusFunction(mdb_read);
                        }
                        else if(pointType == PointType.HR_LONG)
                        {
                            continue;
                        }
                        else
                        {
                            modbusFunction = null;
                            string message = $"PointType:{pointType} value is invalid";
                            Logger.LogError(message);
                            continue;
                        }

                        if (this.readCommandEnqueuer.EnqueueReadCommand(modbusFunction))
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