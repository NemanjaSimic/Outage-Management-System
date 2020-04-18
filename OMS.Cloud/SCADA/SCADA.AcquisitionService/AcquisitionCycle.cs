using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Cloud.SCADA.ModbusFunctions;
using OMS.Cloud.SCADA.ModbusFunctions.Parameters;
using OMS.Common.Cloud;
using OMS.Common.Cloud.WcfServiceFabricClients.SCADA;
using OMS.Common.SCADA;
using Outage.Common;
using System;
using OMS.Common.Cloud.AzureStorageHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Cloud.SCADA.AcquisitionService
{
    internal class AcquisitionCycle
    {
        private readonly FunctionFactory functionFactory;
        
        private ILogger logger;
        private  ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private CloudQueue readCommandQueue;

        public AcquisitionCycle()
        {
            this.functionFactory = new FunctionFactory();
            CloudQueueHelper.TryGetQueue("readcommandqueue", out this.readCommandQueue);
        }

        public async Task Start()
        {
            if (this.readCommandQueue == null)
            {
                string message = $"Read command queue is null.";
                Logger.LogWarn(message);

                if(!CloudQueueHelper.TryGetQueue("readcommandqueue", out this.readCommandQueue))
                {
                    return;
                }
            }

            ScadaModelReadAccessClient scadaModelClient = ScadaModelReadAccessClient.CreateClient();
            Dictionary<ushort, Dictionary<ushort, long>> addressToGidMap = await scadaModelClient.GetAddressToGidMap();

            foreach (var kvp in addressToGidMap)
            {
                if(TryCreateModbusFunction(kvp, out IReadModbusFunction modbusFunction))
                {
                    await this.readCommandQueue.AddMessageAsync(new CloudQueueMessage(Serialization.ObjectToByteArray(modbusFunction)));
                    Logger.LogDebug($"Modbus function enquided. Point type is {kvp.Key}, quantity {modbusFunction.ModbusReadCommandParameters.Quantity}.");
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
            catch(ArgumentException)
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
            switch(pointType)
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
