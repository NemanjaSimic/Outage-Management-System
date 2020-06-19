using OMS.Common.Cloud.Logger;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts.ModbusFunctions;
using OMS.Common.WcfClient.SCADA;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;

namespace SCADA.AcquisitionImplementation
{
    public class AcquisitionCycle
    {
        private readonly ServiceContext context;

        private ReadCommandEnqueuerClient commandEnqueuerClient;
        private ScadaModelReadAccessClient readAccessClient;

        private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public AcquisitionCycle(ServiceContext context)
        { 
            this.context = context;

            this.commandEnqueuerClient = ReadCommandEnqueuerClient.CreateClient();
            this.readAccessClient = ScadaModelReadAccessClient.CreateClient();
        }

        public async Task Start(bool isRetry = false)
        {
            try
            {
                Dictionary<short, Dictionary<ushort, long>> addressToGidMap = await this.readAccessClient.GetAddressToGidMap();

                foreach (var kvp in addressToGidMap)
                {
                    if (TryCreateModbusFunction(kvp, out IReadModbusFunction modbusFunction))
                    {
                        await commandEnqueuerClient.EnqueueReadCommand(modbusFunction);
                        Logger.LogVerbose($"Modbus function enquided. Point type is {kvp.Key}, FunctionCode: {modbusFunction.FunctionCode}, StartAddress: {modbusFunction.StartAddress}, Quantity: {modbusFunction.Quantity}.");
                    }
                }
            }
            catch (Exception e)
            {
                string message = "Exception caught in AcquisitionCycle. Start method.";
                Logger.LogError(message, e);

                if (!isRetry)
                {
                    this.commandEnqueuerClient = ReadCommandEnqueuerClient.CreateClient();
                    this.readAccessClient = ScadaModelReadAccessClient.CreateClient();
                    await Start(true);
                }
                else
                {
                    message = "Exception caught in (Retry) AcquisitionCycle. Start method.";
                    Logger.LogError(message, e);
                    throw e;
                }
            }
        }

        private bool TryCreateModbusFunction(KeyValuePair<short, Dictionary<ushort, long>> addressToGidMapKvp, out IReadModbusFunction modbusFunction)
        {
            modbusFunction = null;
            PointType pointType = (PointType)addressToGidMapKvp.Key;
            Dictionary<ushort, long> addressToGidMap = addressToGidMapKvp.Value;
            ModbusFunctionCode functionCode;

            try
            {
                functionCode = MapPointTypeToModbusFunctionCode(pointType);
            }
            catch (ArgumentException ae)
            {
                Logger.LogVerbose(ae.Message); //recomended to be verbose
                return false;
            }

            ushort startAddress = 1;
            ushort quantity = (ushort)addressToGidMap.Count;

            if (quantity == 0)
            {
                return false;
            }

            modbusFunction = new ReadFunction(functionCode, startAddress, quantity);
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

            throw new ArgumentException($"PointType {pointType} is not asociated with any member of {typeof(ModbusFunctionCode)}");
        }
    }
}
