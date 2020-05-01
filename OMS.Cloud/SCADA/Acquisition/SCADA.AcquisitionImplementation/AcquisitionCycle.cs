using OMS.Common.Cloud.WcfServiceFabricClients.SCADA;
using OMS.Common.SCADA;
using Outage.Common;
using SCADA.ModbusFunctions;
using SCADA.ModbusFunctions.Parameters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCADA.AcquisitionImplementation
{
    public class AcquisitionCycle
    {
        private readonly FunctionFactory functionFactory;
        private ReadCommandEnqueuerClient commandEnqueuerClient;
        private ScadaModelReadAccessClient readAccessClient;

        private ILogger logger;
        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public AcquisitionCycle()
        {
            this.commandEnqueuerClient = ReadCommandEnqueuerClient.CreateClient();
            this.readAccessClient = ScadaModelReadAccessClient.CreateClient();
            this.functionFactory = new FunctionFactory();
        }

        public async Task Start(bool isRetry = false)
        {
            try
            {
                Dictionary<ushort, Dictionary<ushort, long>> addressToGidMap = await this.readAccessClient.GetAddressToGidMap();

                foreach (var kvp in addressToGidMap)
                {
                    if (TryCreateModbusFunction(kvp, out IReadModbusFunction modbusFunction))
                    {
                        await commandEnqueuerClient.EnqueueReadCommand(modbusFunction);
                        Logger.LogDebug($"Modbus function enquided. Point type is {kvp.Key}, quantity {modbusFunction.ModbusReadCommandParameters.Quantity}.");
                    }
                }
            }
            catch (Exception e)
            {
                if(!isRetry)
                {
                    this.commandEnqueuerClient = ReadCommandEnqueuerClient.CreateClient();
                    this.readAccessClient = ScadaModelReadAccessClient.CreateClient();
                    await Start(true);
                }
                else
                {
                    string message = "Exception caught in AcquisitionCycle.Start method.";
                    Logger.LogError(message, e);
                    throw e;
                }
            }
        }

        private bool TryCreateModbusFunction(KeyValuePair<ushort, Dictionary<ushort, long>> addressToGidMapKvp, out IReadModbusFunction modbusFunction)
        {
            modbusFunction = null;
            PointType pointType = (PointType)addressToGidMapKvp.Key;
            Dictionary<ushort, long> addressToGidMap = addressToGidMapKvp.Value;
            ModbusFunctionCode functionCode;

            try
            {
                functionCode = MapPointTypeToModbusFunctionCode(pointType);
            }
            catch (ArgumentException)
            {
                string message = $"PointType:{pointType} value is invalid";
                Logger.LogError(message);
                return false;
            }

            ushort length = 6;  //expected by protocol
            ushort startAddress = 1;
            ushort quantity = (ushort)addressToGidMap.Count;

            if (quantity == 0)
            {
                return false;
            }

            ModbusReadCommandParameters mdb_read = new ModbusReadCommandParameters(length, (byte)functionCode, startAddress, quantity);
            modbusFunction = functionFactory.CreateReadModbusFunction(mdb_read);
            return true;
        }

        private ModbusFunctionCode MapPointTypeToModbusFunctionCode(PointType pointType)
        {
            switch (pointType)
            {
                case PointType.DIGITAL_OUTPUT:
                    return ModbusFunctionCode.READ_COILS;

                case PointType.DIGITAL_INPUT:
                    return ModbusFunctionCode.READ_DISCRETE_INPUTS;

                case PointType.ANALOG_OUTPUT:
                    return ModbusFunctionCode.READ_HOLDING_REGISTERS;

                case PointType.ANALOG_INPUT:
                    return ModbusFunctionCode.READ_INPUT_REGISTERS;
            }

            throw new ArgumentException("MapPointTypeToModbusFunctionCode");
        }
    }
}
