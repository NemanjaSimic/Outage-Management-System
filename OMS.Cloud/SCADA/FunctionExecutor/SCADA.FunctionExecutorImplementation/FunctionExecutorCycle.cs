using EasyModbus;
using EasyModbus.Exceptions;
using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.Cloud.Exceptions.SCADA;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.WcfClient.SCADA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA.FunctionExecutorImplementation
{
    public class FunctionExecutorCycle
    {
        private readonly string baseLogString;
        private readonly CloudQueue readCommandQueue;
        private readonly CloudQueue writeCommandQueue;
        private readonly CloudQueue modelUpdateCommandQueue;

        private IScadaConfigData configData;
        //TODO: prebaci sve u kontrakte
        //private IScadaModelUpdateAccessContract modelUpdateAccessClient;
        private ScadaModelReadAccessClient modelReadAccessClient;
        private ScadaModelUpdateAccessClient modelUpdateAccessClient;
        private ModbusClient modbusClient;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public FunctionExecutorCycle()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>";

            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.ReadCommandQueue, out this.readCommandQueue);
            this.readCommandQueue.ClearAsync();

            CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.WriteCommandQueue, out this.writeCommandQueue);
            this.writeCommandQueue.ClearAsync();

            CloudQueueHelper.TryGetQueue(CloudStorageQueueNames.ModelUpdateCommandQueue, out this.modelUpdateCommandQueue);
            this.modelUpdateCommandQueue.ClearAsync();

            string debugMessage = $"{baseLogString} Ctor => CloudQueues initialized.";
            Logger.LogDebug(debugMessage);

            this.modelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            this.modelUpdateAccessClient = ScadaModelUpdateAccessClient.CreateClient();

            debugMessage = $"{baseLogString} Ctor => Clients initialized.";
            Logger.LogDebug(debugMessage);
        }

        public async Task Start(bool isRetry = false)
        {
            string isRetryString = isRetry ? "yes" : "no";
            string verboseMessage = $"{baseLogString} entering Start method, isRetry: {isRetryString}.";
            Logger.LogVerbose(verboseMessage);

            try
            {
                if (modbusClient == null)
                {
                    await InitializeModbusClient();
                }

                if (!modbusClient.Connected)
                {
                    ConnectToModbusClient();
                }

                while (modelUpdateCommandQueue.PeekMessage() != null)
                {
                    verboseMessage = $"{baseLogString} Start => Getting Command from model update command queue.";
                    Logger.LogVerbose(verboseMessage);

                    CloudQueueMessage message = modelUpdateCommandQueue.GetMessage();
                    if(message!=null)
                    {
                        IModbusFunction currentCommand = (IModbusFunction)(Serialization.ByteArrayToObject(message.AsBytes));
                        modelUpdateCommandQueue.DeleteMessage(message);

                        string informationMessage = $"{baseLogString} Start => Command received from model update command queue about to be executed.";
                        Logger.LogInformation(informationMessage);
                        
                        await ExecuteCommand(currentCommand);
                    }
                }

                while (writeCommandQueue.PeekMessage() != null)
                {
                    verboseMessage = $"{baseLogString} Start => Getting Command from write command queue.";
                    Logger.LogVerbose(verboseMessage);

                    CloudQueueMessage message = writeCommandQueue.GetMessage();
                    if(message!=null)
                    {
                        IModbusFunction currentCommand = (IModbusFunction)(Serialization.ByteArrayToObject(message.AsBytes));
                        writeCommandQueue.DeleteMessage(message);

                        string informationMessage = $"{baseLogString} Start => Command received from write command queue about to be executed.";
                        Logger.LogInformation(informationMessage);

                        await ExecuteCommand(currentCommand);
                    }
                }

                while (readCommandQueue.PeekMessage() != null)
                {
                    verboseMessage = $"{baseLogString} Start => Getting Command from read command queue.";
                    Logger.LogInformation(verboseMessage);

                    CloudQueueMessage message = readCommandQueue.GetMessage();
                    if (message != null)
                    {
                        IModbusFunction currentCommand = (IModbusFunction)(Serialization.ByteArrayToObject(message.AsBytes));
                        readCommandQueue.DeleteMessage(message);

                        verboseMessage = $"{baseLogString} Start => Command received from read command queue about to be executed.";
                        Logger.LogVerbose(verboseMessage);

                        await ExecuteCommand(currentCommand);
                    }
                }
            }
            catch (CommunicationObjectFaultedException e)
            {
                string message = $"{baseLogString} Start => CommunicationObjectFaultedException caught.";
                Logger.LogError(message, e);

                await Task.Delay(2000);

                this.modelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
                this.modelUpdateAccessClient = ScadaModelUpdateAccessClient.CreateClient();

                string debugMessage = $"{baseLogString} Start => Clients re-initialized.";
                Logger.LogDebug(debugMessage);

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

        #region ModbusClient
        private async Task InitializeModbusClient()
        {
            try
            {
                this.configData = await this.modelReadAccessClient.GetScadaConfigData();
                this.modbusClient = new ModbusClient(configData.IpAddress.ToString(), configData.TcpPort);
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} InitializeModbusClient => Exception: {e.Message}.";
                Logger.LogError(message, e);
            }

            if (modbusClient == null)
            {
                string message = $"{baseLogString} InitializeModbusClient => ModbusClient is null";
                Logger.LogError(message);
                throw new Exception(message);
            }
        }

        private void ConnectToModbusClient()
        {
            int maxNumberOfTries = 160;
            int numberOfTries = 0;
            int sleepInterval = 500;

            string message = $"{baseLogString} ConnectToModbusClient => Connecting to modbus client...";
            Logger.LogInformation(message);

            while (!modbusClient.Connected)
            {
                try
                {
                    modbusClient.Connect();
                }
                catch (ConnectionException ce)
                {
                    string warnMessage = $"{baseLogString} ConnectToModbusClient => ConnectionException on ModbusClient.Connect().";
                    Logger.LogWarning(warnMessage, ce);
                }

                if (!modbusClient.Connected)
                {
                    numberOfTries++;
                    Logger.LogDebug($"{baseLogString} ConnectToModbusClient => Connecting try number: {numberOfTries}.");

                    if (numberOfTries >= 100)
                    {
                        sleepInterval = 1000;
                    }

                    Thread.Sleep(sleepInterval);
                }
                else if (!modbusClient.Connected && numberOfTries == maxNumberOfTries)
                {
                    string timeoutMessage = $"{baseLogString} ConnectToModbusClient => Failed to connect to Modbus client by exceeding the maximum number of connection retries ({maxNumberOfTries}).";
                    Logger.LogError(timeoutMessage);
                    throw new Exception(timeoutMessage);
                }
                else
                {
                    message = $"{baseLogString} ConnectToModbusClient => Successfully connected to modbus client.";
                    Logger.LogInformation(message);
                }
            }
        }
        #endregion ModbusClient

        private async Task ExecuteCommand(IModbusFunction command)
        {
            string verboseMessage = $"{baseLogString} entering ExecuteCommand method, command's FunctionCode: {command.FunctionCode}.";
            Logger.LogVerbose(verboseMessage);

            switch (command.FunctionCode)
            {
                case ModbusFunctionCode.READ_COILS:
                case ModbusFunctionCode.READ_DISCRETE_INPUTS:
                case ModbusFunctionCode.READ_HOLDING_REGISTERS:
                case ModbusFunctionCode.READ_INPUT_REGISTERS:
                    await ExecuteReadCommand((IReadModbusFunction)command);
                    break;

                case ModbusFunctionCode.WRITE_SINGLE_COIL:
                case ModbusFunctionCode.WRITE_SINGLE_REGISTER:
                    await ExecuteWriteSingleCommand((IWriteSingleFunction)command);
                    break;

                case ModbusFunctionCode.WRITE_MULTIPLE_COILS:
                case ModbusFunctionCode.WRITE_MULTIPLE_REGISTERS:
                    await ExecuteWriteMultipleCommand((IWriteMultipleFunction)command);
                    break;
            }
        }

        #region Execute Read
        private async Task ExecuteReadCommand(IReadModbusFunction readCommand)
        {
            string verboseMessage = $"{baseLogString} entering ExecuteReadCommand method, FunctionCode: {readCommand.FunctionCode}, StartAddress: {readCommand.StartAddress}, Quantity: {readCommand.Quantity}.";
            Logger.LogVerbose(verboseMessage);

            ModbusFunctionCode functionCode = readCommand.FunctionCode;
            ushort startAddress = readCommand.StartAddress;
            ushort quantity = readCommand.Quantity;

            if (quantity <= 0)
            {
                string message = $"{baseLogString} ExecuteReadCommand => Reading Quantity: {quantity} does not make sense.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if (startAddress + quantity >= ushort.MaxValue || startAddress + quantity == ushort.MinValue || startAddress == ushort.MinValue)
            {
                string message = $"{baseLogString} ExecuteReadCommand => Address is out of bound. Start address: {startAddress}, Quantity: {quantity}";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if(functionCode == ModbusFunctionCode.READ_COILS || functionCode == ModbusFunctionCode.READ_DISCRETE_INPUTS)
            {
                verboseMessage = $"{baseLogString} ExecuteReadCommand => ExecuteDiscreteReadCommand about to be called.";
                Logger.LogVerbose(verboseMessage);

                //LOGIC
                await ExecuteDiscreteReadCommand(functionCode, startAddress, quantity);
            }
            else if (functionCode == ModbusFunctionCode.READ_HOLDING_REGISTERS || functionCode == ModbusFunctionCode.READ_INPUT_REGISTERS)
            {
                verboseMessage = $"{baseLogString} ExecuteReadCommand => ExecuteAnalogReadCommand about to be called.";
                Logger.LogVerbose(verboseMessage);

                //LOGIC
                await ExecuteAnalogReadCommand(functionCode, startAddress, quantity);
            }
        }

        private async Task ExecuteDiscreteReadCommand(ModbusFunctionCode functionCode, ushort startAddress, ushort quantity)
        {
            string verboseMessage = $"{baseLogString} entering ExecuteDiscreteReadCommand method, command's functionCode: {functionCode}, startAddress: {startAddress}, quantity:{quantity}.";
            Logger.LogVerbose(verboseMessage);

            bool[] data;
            PointType pointType;

            if (functionCode == ModbusFunctionCode.READ_COILS)
            {
                verboseMessage = $"{baseLogString} ExecuteDiscreteReadCommand => about to call ModbusClient.ReadCoils({startAddress - 1}, {quantity}) method.";
                Logger.LogVerbose(verboseMessage);

                //LOGIC
                pointType = PointType.DIGITAL_OUTPUT;
                data = modbusClient.ReadCoils(startAddress - 1, quantity);

                verboseMessage = $"{baseLogString} ExecuteDiscreteReadCommand => ModbusClient.ReadCoils({startAddress - 1}, {quantity}) method SUCCESSFULLY executed. Resulting data count: {data.Length}.";
                Logger.LogVerbose(verboseMessage);
            }
            else if(functionCode == ModbusFunctionCode.READ_DISCRETE_INPUTS)
            {
                verboseMessage = $"{baseLogString} ExecuteDiscreteReadCommand => about to call ModbusClient.ReadDiscreteInputs({startAddress - 1}, {quantity}) method.";
                Logger.LogVerbose(verboseMessage);

                //LOGIC
                pointType = PointType.DIGITAL_INPUT;
                data = modbusClient.ReadDiscreteInputs(startAddress - 1, quantity);

                verboseMessage = $"{baseLogString} ExecuteDiscreteReadCommand => ModbusClient.ReadDiscreteInputs({startAddress - 1}, {quantity}) method SUCCESSFULLY executed. Resulting data count: {data.Length}.";
                Logger.LogVerbose(verboseMessage);
            }
            else
            {
                string errorMessage = $"{baseLogString} ExecuteDiscreteReadCommand => function code is neither ModbusFunctionCode.READ_COILS nor ModbusFunctionCode.READ_DISCRETE_INPUTS";
                Logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            var measurementCache = new Dictionary<long, DiscreteModbusData>(data.Length);
            
            ScadaModelReadAccessClient modelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            var currentSCADAModel = await modelReadAccessClient.GetGidToPointItemMap();
            var currentAddressToGidMap = await modelReadAccessClient.GetAddressToGidMap();
            var commandValuesCache = await modelReadAccessClient.GetCommandDescriptionCache();

            for (ushort i = 0; i < data.Length; i++)
            {
                ushort address = (ushort)(startAddress + i);
                ushort value = (ushort)(data[i] ? 1 : 0);

                //for commands enqueued during model update, that are not valid
                if (!currentAddressToGidMap[(short)pointType].ContainsKey(address))
                {
                    Logger.LogWarning($"{baseLogString} ExecuteDiscreteReadCommand => trying to read value on address {address}, Point type: {pointType}, which is not in the current SCADA Model.");
                    continue;
                }

                long gid = currentAddressToGidMap[(short)pointType][address];

                //for commands enqueued during model update, that are not valid
                if (!currentSCADAModel.ContainsKey(gid))
                {
                    Logger.LogWarning($"{baseLogString} ExecuteDiscreteReadCommand => trying to read value for measurement with gid: 0x{gid:X16}, which is not in the current SCADA Model.");
                    continue;
                }

                if (!(currentSCADAModel[gid] is IDiscretePointItem pointItem))
                {
                    string message = $"{baseLogString} ExecuteDiscreteReadCommand => PointItem [Gid: 0x{gid:X16}] does not implement {typeof(IDiscretePointItem)}.";
                    Logger.LogError(message);
                    throw new InternalSCADAServiceException(message);
                }

                if (pointItem.CurrentValue != value)
                {
                    //pointItem.CurrentValue = value;
                    pointItem = (IDiscretePointItem)(await modelUpdateAccessClient.UpdatePointItemRawValue(pointItem.Gid, value));
                    Logger.LogInformation($"{baseLogString} ExecuteDiscreteReadCommand => Alarm for Point [Gid: 0x{pointItem.Gid:X16}, Address: {pointItem.Address}] set to {pointItem.Alarm}.");
                }

                CommandOriginType commandOrigin = CommandOriginType.OTHER_COMMAND;

                if (commandValuesCache.ContainsKey(gid) && commandValuesCache[gid].Value == value)
                {
                    commandOrigin = commandValuesCache[gid].CommandOrigin;
                    //commandValuesCache.Remove(gid);
                    await modelUpdateAccessClient.RemoveCommandDescription(gid);
                    Logger.LogDebug($"{baseLogString} ExecuteDiscreteReadCommand => Command origin of command address: {pointItem.Address} is set to {commandOrigin}.");
                }

                DiscreteModbusData digitalData = new DiscreteModbusData(value, pointItem.Alarm, gid, commandOrigin);
                measurementCache.Add(gid, digitalData);

                verboseMessage = $"{baseLogString} ExecuteDiscreteReadCommand => DiscreteModbusData added to measurementCache. MeasurementGid: {digitalData.MeasurementGid:X16}, Value: {digitalData.Value}, Alarm: {digitalData.Alarm}, CommandOrigin: {digitalData.CommandOrigin} .";
                Logger.LogVerbose(verboseMessage);
            }
            
            //LOGIC
            await this.modelUpdateAccessClient.MakeDiscreteEntryToMeasurementCache(measurementCache, true);
            
            verboseMessage = $"{baseLogString} ExecuteDiscreteReadCommand => MakeDiscreteEntryToMeasurementCache method called. measurementCache count: {measurementCache.Count}.";
            Logger.LogVerbose(verboseMessage);
        }

        private async Task ExecuteAnalogReadCommand(ModbusFunctionCode functionCode, ushort startAddress, ushort quantity)
        {
            string verboseMessage = $"{baseLogString} entering ExecuteAnalogReadCommand method, command's functionCode: {functionCode}, startAddress: {startAddress}, quantity:{quantity}.";
            Logger.LogVerbose(verboseMessage);

            int[] data;
            PointType pointType;

            if (functionCode == ModbusFunctionCode.READ_HOLDING_REGISTERS)
            {
                verboseMessage = $"{baseLogString} ExecuteAnalogReadCommand => about to call ModbusClient.ReadHoldingRegisters({startAddress - 1}, {quantity}) method.";
                Logger.LogVerbose(verboseMessage);

                //LOGIC
                pointType = PointType.ANALOG_OUTPUT;
                data = modbusClient.ReadHoldingRegisters(startAddress - 1, quantity);

                verboseMessage = $"{baseLogString} ExecuteAnalogReadCommand => ModbusClient.ReadHoldingRegisters({startAddress - 1}, {quantity}) method SUCCESSFULLY executed. Resulting data count: {data.Length}.";
                Logger.LogVerbose(verboseMessage);
            }
            else if (functionCode == ModbusFunctionCode.READ_INPUT_REGISTERS)
            {
                verboseMessage = $"{baseLogString} ExecuteAnalogReadCommand => about to call ModbusClient.ReadInputRegisters({startAddress - 1}, {quantity}) method.";
                Logger.LogVerbose(verboseMessage);

                //LOGIC
                pointType = PointType.ANALOG_INPUT;
                data = modbusClient.ReadInputRegisters(startAddress - 1, quantity);

                verboseMessage = $"{baseLogString} ExecuteAnalogReadCommand => ModbusClient.ReadInputRegisters({startAddress - 1}, {quantity}) method SUCCESSFULLY executed. Resulting data count: {data.Length}.";
                Logger.LogVerbose(verboseMessage);
            }
            else
            {
                string message = $"{baseLogString} ExecuteAnalogReadCommand => function code is neither ModbusFunctionCode.READ_HOLDING_REGISTERS nor ModbusFunctionCode.READ_INPUT_REGISTERS";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            var measurementCache = new Dictionary<long, AnalogModbusData>(data.Length);

            ScadaModelReadAccessClient modelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            var gidToPointItemMap = await modelReadAccessClient.GetGidToPointItemMap();
            var addressToGidMap = await modelReadAccessClient.GetAddressToGidMap();
            var commandDescriptionCache = await modelReadAccessClient.GetCommandDescriptionCache();

            for (ushort i = 0; i < data.Length; i++)
            {
                ushort address = (ushort)(startAddress + i);
                int rawValue = data[i];

                //for commands enqueued during model update, that are not valid
                if (!addressToGidMap[(short)pointType].ContainsKey(address))
                {
                    Logger.LogWarning($"{baseLogString} ExecuteAnalogReadCommand => trying to read value on address {address}, Point type: {pointType}, which is not in the current SCADA Model.");
                    continue;
                }

                long gid = addressToGidMap[(short)pointType][address];

                //for commands enqueued during model update, that are not valid
                if (!gidToPointItemMap.ContainsKey(gid))
                {
                    Logger.LogWarning($"{baseLogString} ExecuteAnalogReadCommand => trying to read value for measurement with gid: 0x{gid:X16}, which is not in the current SCADA Model.");
                    continue;
                }

                if (!(gidToPointItemMap[gid] is IAnalogPointItem pointItem))
                {
                    string message = $"{baseLogString} ExecuteAnalogReadCommand => PointItem [Gid: 0x{gid:X16}] does not implement {typeof(IAnalogPointItem)}.";
                    Logger.LogError(message);
                    throw new Exception(message);
                }

                //float eguValue = pointItem.RawToEguValueConversion(rawValue);
                if (pointItem.CurrentRawValue != rawValue)
                {
                    //pointItem.CurrentEguValue = eguValue;
                    pointItem = (IAnalogPointItem)(await modelUpdateAccessClient.UpdatePointItemRawValue(pointItem.Gid, rawValue));
                    Logger.LogInformation($"{baseLogString} ExecuteAnalogReadCommand => Alarm for Point [Gid: 0x{pointItem.Gid:X16}, Address: {pointItem.Address}] set to {pointItem.Alarm}.");
                }

                CommandOriginType commandOrigin = CommandOriginType.OTHER_COMMAND;

                if (commandDescriptionCache.ContainsKey(gid) && commandDescriptionCache[gid].Value == pointItem.CurrentRawValue)
                {
                    commandOrigin = commandDescriptionCache[gid].CommandOrigin;
                    //commandValuesCache.Remove(gid);
                    await modelUpdateAccessClient.RemoveCommandDescription(gid);
                    Logger.LogDebug($"{baseLogString} ExecuteAnalogReadCommand => Command origin of command address: {pointItem.Address} is set to {commandOrigin}.");
                }

                AnalogModbusData analogData = new AnalogModbusData(pointItem.CurrentEguValue, pointItem.Alarm, gid, commandOrigin);
                measurementCache.Add(gid, analogData);

                verboseMessage = $"{baseLogString} ExecuteAnalogReadCommand => AnalogModbusData added to measurementCache. MeasurementGid: {analogData.MeasurementGid:X16}, Value: {analogData.Value}, Alarm: {analogData.Alarm}, CommandOrigin: {analogData.CommandOrigin} .";
                Logger.LogVerbose(verboseMessage);
            }

            //LOGIC
            await this.modelUpdateAccessClient.MakeAnalogEntryToMeasurementCache(measurementCache, true);

            verboseMessage = $"{baseLogString} ExecuteAnalogReadCommand => MakeAnalogEntryToMeasurementCache method called. measurementCache count: {measurementCache.Count}.";
            Logger.LogVerbose(verboseMessage);
        }
        #endregion Execute Read

        #region Execute Write
        private async Task ExecuteWriteSingleCommand(IWriteSingleFunction writeCommand)
        {
            string verboseMessage = $"{baseLogString} entering ExecuteWriteSingleCommand method, command's FunctionCode: {writeCommand.FunctionCode}.";
            Logger.LogVerbose(verboseMessage);

            PointType pointType;
            ushort outputAddress = writeCommand.OutputAddress;
            int commandValue = writeCommand.CommandValue;

            if (outputAddress >= ushort.MaxValue || outputAddress == ushort.MinValue)
            {
                string message = $"{baseLogString} ExecuteWriteSingleCommand => Address is out of bound. Output address: {outputAddress}.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if (writeCommand.FunctionCode == ModbusFunctionCode.WRITE_SINGLE_COIL)
            {
                pointType = PointType.DIGITAL_OUTPUT;
                
                bool booleanCommand;
                if (commandValue == 0)
                {
                    booleanCommand = false;
                }
                else if (commandValue == 1)
                {
                    booleanCommand = true;
                }
                else
                {
                    string errorMessage = $"{baseLogString} ExecuteWriteSingleCommand => Non-boolean value in write single coil command parameter.";
                    Logger.LogError(errorMessage);
                    throw new ArgumentException(errorMessage);
                }

                string debugMessage = $"{baseLogString} ExecuteWriteSingleCommand => about to call ModbusClient.WriteSingleCoil({outputAddress - 1}, {booleanCommand}) method. OutputAddress: {outputAddress}";
                Logger.LogDebug(debugMessage);

                //LOGIC
                modbusClient.WriteSingleCoil(outputAddress - 1, booleanCommand);

                string infoMessage = $"{baseLogString} ExecuteWriteSingleCommand => ModbusClient.WriteSingleCoil({outputAddress - 1}, {booleanCommand}) method SUCCESSFULLY executed. OutputAddress: {outputAddress}";
                Logger.LogInformation(infoMessage);
            }
            else if (writeCommand.FunctionCode == ModbusFunctionCode.WRITE_SINGLE_REGISTER)
            {
                pointType = PointType.ANALOG_OUTPUT;

                string debugMessage = $"{baseLogString} ExecuteWriteSingleCommand => about to call ModbusClient.WriteSingleRegister({outputAddress - 1}, {commandValue}) method. OutputAddress: {outputAddress}";
                Logger.LogDebug(debugMessage);

                //LOGIC
                modbusClient.WriteSingleRegister(outputAddress - 1, commandValue);

                string infoMessage = $"{baseLogString} ExecuteWriteSingleCommand => ModbusClient.WriteSingleRegister({outputAddress - 1}, {commandValue}) method SUCCESSFULLY executed. OutputAddress: {outputAddress}";
                Logger.LogInformation(infoMessage);
            }
            else
            {
                string errorMessage = $"{baseLogString} ExecuteWriteSingleCommand => function code is neither ModbusFunctionCode.READ_HOLDING_REGISTERS nor ModbusFunctionCode.READ_INPUT_REGISTERS";
                Logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            var addressToGidMap = await this.modelReadAccessClient.GetAddressToGidMap();

            if (addressToGidMap[(short)pointType].ContainsKey(outputAddress))
            {
                long gid = addressToGidMap[(short)pointType][outputAddress];

                CommandDescription commandDescription = new CommandDescription()
                {
                    Gid = gid,
                    Address = outputAddress,
                    Value = commandValue,
                    CommandOrigin = writeCommand.CommandOrigin,
                };

                string debugMessage = $"{baseLogString} ExecuteWriteSingleCommand => About to send CommandDescription to CommandDescriptionCache. Gid: {commandDescription.Gid:X16}, Address: {commandDescription.Address}, Value: {commandDescription.Value}, CommandOrigin: {commandDescription.CommandOrigin}";
                Logger.LogDebug(debugMessage);

                //LOGIC
                await this.modelUpdateAccessClient.AddOrUpdateCommandDescription(gid, commandDescription);

                string infoMessage = $"{baseLogString} ExecuteWriteSingleCommand => CommandDescription sent successfuly to CommandDescriptionCache. Gid: {commandDescription.Gid:X16}, Address: {commandDescription.Address}, Value: {commandDescription.Value}, CommandOrigin: {commandDescription.CommandOrigin}";
                Logger.LogInformation(infoMessage);
            }
        }

        private async Task ExecuteWriteMultipleCommand(IWriteMultipleFunction writeCommand)
        {
            string verboseMessage = $"{baseLogString} entering ExecuteWriteMultipleCommand method, command's FunctionCode: {writeCommand.FunctionCode}.";
            Logger.LogVerbose(verboseMessage);

            ushort startAddress = writeCommand.StartAddress;
            int quantity = writeCommand.CommandValues.Length;

            if (startAddress >= ushort.MaxValue || startAddress == ushort.MinValue)
            {
                string message = $"{baseLogString} ExecuteWriteMultipleCommand => Address is out of bound. Output address: {startAddress}.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if (writeCommand.StartAddress + quantity >= ushort.MaxValue || startAddress + quantity == ushort.MinValue || writeCommand.StartAddress == ushort.MinValue)
            {
                string message = $"{baseLogString} ExecuteWriteMultipleCommand => Address is out of bound. Start address: {startAddress}, Quantity: {quantity}";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if(writeCommand.CommandValues.Length == 0)
            {
                string message = $"{baseLogString} ExecuteWriteMultipleCommand => CommandValues array is empty.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if (writeCommand.FunctionCode == ModbusFunctionCode.WRITE_MULTIPLE_COILS)
            {
                string debugMessage = $"{baseLogString} ExecuteWriteMultipleCommand => about to call ExecuteWriteMultipleDiscreteCommand({startAddress}, {writeCommand.CommandValues}, {writeCommand.CommandOrigin})";
                Logger.LogDebug(debugMessage);

                //LOGIC
                await ExecuteWriteMultipleDiscreteCommand(startAddress, writeCommand.CommandValues, writeCommand.CommandOrigin);

                debugMessage = $"{baseLogString} ExecuteWriteMultipleCommand => ExecuteWriteMultipleDiscreteCommand() method SUCCESSFULLY executed.";
                Logger.LogDebug(debugMessage);
            }
            else if (writeCommand.FunctionCode == ModbusFunctionCode.WRITE_MULTIPLE_REGISTERS)
            {
                string debugMessage = $"{baseLogString} ExecuteWriteMultipleCommand => about to call ExecuteWriteMultipleAnalogCommand({startAddress}, {writeCommand.CommandValues}, {writeCommand.CommandOrigin})";
                Logger.LogDebug(debugMessage);

                //LOGIC
                await ExecuteWriteMultipleAnalogCommand(startAddress, writeCommand.CommandValues, writeCommand.CommandOrigin);

                debugMessage = $"{baseLogString} ExecuteWriteMultipleCommand => ExecuteWriteMultipleAnalogCommand() method SUCCESSFULLY executed.";
                Logger.LogDebug(debugMessage);
            }
            else
            {
                string errorMessage = $"{baseLogString} ExecuteWriteMultipleCommand => function code is neither ModbusFunctionCode.WRITE_MULTIPLE_COILS nor ModbusFunctionCode.WRITE_MULTIPLE_REGISTERS";
                Logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }
        }

        private async Task ExecuteWriteMultipleDiscreteCommand(ushort startAddress, int[] commandValues, CommandOriginType commandOrigin)
        {
            StringBuilder commandValuesSB = new StringBuilder();
            commandValuesSB.Append("[ ");
            foreach (int value in commandValues)
            {
                commandValuesSB.Append(value);
                commandValuesSB.Append(" ");
            }
            commandValuesSB.Append("]");

            string verboseMessage = $"{baseLogString} entering ExecuteWriteMultipleDiscreteCommand method, command's startAddress: {startAddress}, commandValues: {commandValuesSB}, commandOrigin: {commandOrigin}.";
            Logger.LogVerbose(verboseMessage);

            //LOGIC
            int quantity = commandValues.Length;
            var addressToGidMap = await this.modelReadAccessClient.GetAddressToGidMap();
            var commandDescriptions = new Dictionary<long, CommandDescription>();

            string debugMessage = "";
            bool[] booleanCommands = new bool[quantity];
            StringBuilder booleanCommandsSB = new StringBuilder();
            booleanCommandsSB.Append("[ ");

            for (ushort index = 0; index < quantity; index++)
            {
                ushort address = (ushort)(startAddress + index);

                if (commandValues[index] == 0)
                {
                    booleanCommands[index] = false;
                    booleanCommandsSB.Append(false);

                    if (index < quantity - 1)
                    {
                        booleanCommandsSB.Append(" ");
                    }
                    else
                    {
                        booleanCommandsSB.Append(" ]");
                    }
                }
                else if (commandValues[index] == 1)
                {
                    booleanCommands[index] = true;
                    booleanCommandsSB.Append(true);
                    
                    if(index < quantity - 1)
                    {
                        booleanCommandsSB.Append(" ");
                    }
                    else
                    {
                        booleanCommandsSB.Append(" ]");
                    }
                }
                else
                {
                    string errorMessage = $"{baseLogString} ExecuteWriteMultipleDiscreteCommand => Non-boolean value in write single coil command parameter.";
                    Logger.LogError(errorMessage);
                    throw new ArgumentException();
                }

                if (addressToGidMap[(short)PointType.DIGITAL_OUTPUT].ContainsKey(address))
                {
                    long gid = addressToGidMap[(short)PointType.DIGITAL_OUTPUT][address];

                    CommandDescription commandDescription = new CommandDescription()
                    {
                        Gid = gid,
                        Address = address,
                        Value = commandValues[index],
                        CommandOrigin = commandOrigin,
                    };

                    //LOGIC
                    commandDescriptions.Add(gid, commandDescription);

                    debugMessage = $"{baseLogString} ExecuteWriteMultipleDiscreteCommand => CommandDescription added to the collection of commandDescriptions. Gid: {commandDescription.Gid:X16}, Address: {commandDescription.Address}, Value: {commandDescription.Value}, CommandOrigin: {commandDescription.CommandOrigin}";
                    Logger.LogDebug(debugMessage);
                }
            }

            debugMessage = $"{baseLogString} ExecuteWriteMultipleDiscreteCommand => about to call ModbusClient.WriteMultipleCoils({startAddress - 1}, {booleanCommandsSB}) method. StartAddress: {startAddress}, Quantity: {quantity}";
            Logger.LogDebug(debugMessage);

            //LOGIC
            modbusClient.WriteMultipleCoils(startAddress - 1, booleanCommands);

            string infoMessage = $"{baseLogString} ExecuteWriteMultipleDiscreteCommand => ModbusClient.WriteMultipleCoils() method SUCCESSFULLY executed. StartAddress: {startAddress}, Quantity: {quantity}";
            Logger.LogInformation(infoMessage);

            debugMessage = $"{baseLogString} ExecuteWriteMultipleDiscreteCommand => About to send collection of CommandDescriptions to CommandDescriptionCache. collection count: {commandDescriptions.Count}";
            Logger.LogDebug(debugMessage);

            //LOGIC
            foreach (CommandDescription description in commandDescriptions.Values)
            {
                //TODO: parallelization
                await this.modelUpdateAccessClient.AddOrUpdateCommandDescription(description.Gid, description);
            }

            infoMessage = $"{baseLogString} ExecuteWriteMultipleDiscreteCommand => collection of CommandDescriptions sent SUCCESSFULLY to CommandDescriptionCache. collection count: {commandDescriptions.Count}";
            Logger.LogInformation(infoMessage);
        }

        private async Task ExecuteWriteMultipleAnalogCommand(ushort startAddress, int[] commandValues, CommandOriginType commandOrigin)
        {
            StringBuilder commandValuesSB = new StringBuilder();
            commandValuesSB.Append("[ ");
            foreach (int value in commandValues)
            {
                commandValuesSB.Append(value);
                commandValuesSB.Append(" ");
            }
            commandValuesSB.Append("]");

            string verboseMessage = $"{baseLogString} entering ExecuteWriteMultipleAnalogCommand method, command's startAddress: {startAddress}, commandValues: {commandValuesSB}, commandOrigin: {commandOrigin}.";
            Logger.LogVerbose(verboseMessage);

            //LOGIC
            string debugMessage = "";
            int quantity = commandValues.Length;
            var addressToGidMap = await this.modelReadAccessClient.GetAddressToGidMap();
            var commandDescriptions = new Dictionary<long, CommandDescription>();

            for (ushort index = 0; index < quantity; index++)
            {
                ushort address = (ushort)(startAddress + index);

                if (addressToGidMap[(short)PointType.ANALOG_OUTPUT].ContainsKey(address))
                {
                    long gid = addressToGidMap[(short)PointType.ANALOG_OUTPUT][address];

                    CommandDescription commandDescription = new CommandDescription()
                    {
                        Gid = gid,
                        Address = address,
                        Value = commandValues[index],
                        CommandOrigin = commandOrigin,
                    };

                    //LOGIC
                    commandDescriptions.Add(gid, commandDescription);

                    debugMessage = $"{baseLogString} ExecuteWriteMultipleAnalogCommand => CommandDescription added to the collection of commandDescriptions. Gid: {commandDescription.Gid:X16}, Address: {commandDescription.Address}, Value: {commandDescription.Value}, CommandOrigin: {commandDescription.CommandOrigin}";
                    Logger.LogDebug(debugMessage);
                }
            }

            debugMessage = $"{baseLogString} ExecuteWriteMultipleAnalogCommand => about to call ModbusClient.WriteMultipleRegisters({startAddress - 1}, {commandValuesSB}) method. StartAddress: {startAddress}, Quantity: {quantity}";
            Logger.LogDebug(debugMessage);

            //LOGIC
            modbusClient.WriteMultipleRegisters(startAddress - 1, commandValues);

            string infoMessage = $"{baseLogString} ExecuteWriteMultipleAnalogCommand => ModbusClient.WriteMultipleRegisters() method SUCCESSFULLY executed. StartAddress: {startAddress}, Quantity: {quantity}";
            Logger.LogInformation(infoMessage);

            debugMessage = $"{baseLogString} ExecuteWriteMultipleAnalogCommand => About to send collection of CommandDescriptions to CommandDescriptionCache. collection count: {commandDescriptions.Count}";
            Logger.LogDebug(debugMessage);

            //LOGIC
            foreach (CommandDescription description in commandDescriptions.Values)
            {
                //TODO: parallelization
                await this.modelUpdateAccessClient.AddOrUpdateCommandDescription(description.Gid, description);
            }

            infoMessage = $"{baseLogString} ExecuteWriteMultipleAnalogCommand => collection of CommandDescriptions sent SUCCESSFULLY to CommandDescriptionCache. collection count: {commandDescriptions.Count}";
            Logger.LogInformation(infoMessage);
        }
        #endregion Execute Write
    }
}
