using OMS.Common.Cloud.Logger;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts.ModbusFunctions;
using OMS.Common.WcfClient.SCADA;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SCADA.AcquisitionImplementation
{
    public class AcquisitionCycle
    {
        private readonly string baseLogString;
        private readonly ServiceContext context;

        private ReadCommandEnqueuerClient commandEnqueuerClient;
        private ScadaModelReadAccessClient readAccessClient;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public AcquisitionCycle(ServiceContext context)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            this.context = context;

            this.commandEnqueuerClient = ReadCommandEnqueuerClient.CreateClient();
            this.readAccessClient = ScadaModelReadAccessClient.CreateClient();
        }

        public async Task Start(bool isRetry = false)
        {
            string isRetryString = isRetry ? "yes" : "no";
            string verboseMessage = $"{baseLogString} entering Start method, isRetry: {isRetryString}.";
            Logger.LogVerbose(verboseMessage);

            try
            {
                verboseMessage = $"{baseLogString} Start => Trying to get AddressToGidMap.";
                Logger.LogVerbose(verboseMessage);

                Dictionary<short, Dictionary<ushort, long>> addressToGidMap = await this.readAccessClient.GetAddressToGidMap();

                verboseMessage = $"{baseLogString} Start => AddressToGidMap received, Count: {addressToGidMap.Count}.";
                Logger.LogVerbose(verboseMessage);

                foreach (var kvp in addressToGidMap)
                {
                    if((PointType)kvp.Key == PointType.HR_LONG)
                    {
                        continue;
                    }

                    verboseMessage = $"{baseLogString} Start => AddressToGidMap value for key {kvp.Key} is dictionary with Count: {kvp.Value.Count}.";
                    Logger.LogVerbose(verboseMessage);

                    if (TryCreateModbusFunction(kvp, out IReadModbusFunction modbusFunction))
                    {
                        await commandEnqueuerClient.EnqueueReadCommand(modbusFunction);
                        verboseMessage = $"{baseLogString} Start => Modbus function enquided. Point type is {kvp.Key}, FunctionCode: {modbusFunction.FunctionCode}, StartAddress: {modbusFunction.StartAddress}, Quantity: {modbusFunction.Quantity}.";
                        Logger.LogVerbose(verboseMessage);
                    }
                }
            }
            catch(CommunicationObjectFaultedException e)
            {
                string message = $"{baseLogString} Start => CommunicationObjectFaultedException caught.";
                Logger.LogError(message, e);

                this.commandEnqueuerClient = ReadCommandEnqueuerClient.CreateClient();
                this.readAccessClient = ScadaModelReadAccessClient.CreateClient();
                await Start(true);
                //todo: different logic on multiple rety?
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} Start => Exception caught.";
                Logger.LogError(message, e);
                throw e;
            }
        }

        private bool TryCreateModbusFunction(KeyValuePair<short, Dictionary<ushort, long>> addressToGidMapKvp, out IReadModbusFunction modbusFunction)
        {
            string verboseMessage = $"{baseLogString} entering TryCreateModbusFunction method => addressToGidMapKvp(key: {addressToGidMapKvp.Key}, value count: {addressToGidMapKvp.Value.Count}).";
            Logger.LogVerbose(verboseMessage);

            modbusFunction = null;
            PointType pointType = (PointType)addressToGidMapKvp.Key;
            Dictionary<ushort, long> addressToGidMap = addressToGidMapKvp.Value;
            ModbusFunctionCode functionCode;

            try
            {
                functionCode = MapPointTypeToModbusFunctionCode(pointType);
                verboseMessage = $"{baseLogString} TryCreateModbusFunction => function code mapped: {functionCode}.";
                Logger.LogVerbose(verboseMessage);
            }
            catch (ArgumentException ae)
            {
                Logger.LogVerbose(ae.Message); //recomended to be verbose, becouse Acquisition happens very often
                return false;
            }

            ushort startAddress = 1;
            ushort quantity = (ushort)addressToGidMap.Count;

            if (quantity == 0)
            {
                return false;
            }

            modbusFunction = new ReadFunction(functionCode, startAddress, quantity);
            verboseMessage = $"{baseLogString} TryCreateModbusFunction => ReadFunction with code: {modbusFunction.FunctionCode}, stratring address: {modbusFunction.StartAddress} and quantity: {modbusFunction.Quantity} SUCCESSFULLY created.";
            Logger.LogVerbose(verboseMessage);
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

            string message = $"{baseLogString} MapPointTypeToModbusFunctionCode => PointType {pointType} is not asociated with any member of {typeof(ModbusFunctionCode)}";
            Logger.LogError(message);
            throw new ArgumentException(message);
        }
    }
}
