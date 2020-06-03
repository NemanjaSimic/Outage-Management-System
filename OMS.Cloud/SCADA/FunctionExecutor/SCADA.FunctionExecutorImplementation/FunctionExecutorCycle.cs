using EasyModbus;
using EasyModbus.Exceptions;
using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.Cloud.WcfServiceFabricClients.SCADA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using Outage.Common;
using Outage.Common.Exceptions.SCADA;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA.FunctionExecutorImplementation
{
    public class FunctionExecutorCycle
    {
        private ILogger logger;
        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private readonly CloudQueue readCommandQueue;
        private readonly CloudQueue writeCommandQueue;
        private readonly CloudQueue modelUpdateCommandQueue;

        private ScadaModelReadAccessClient modelReadAccessClient;
        private ScadaModelUpdateAccessClient modelUpdateAccessClient;

        private IScadaConfigData configData;
        private ModbusClient modbusClient;

        public FunctionExecutorCycle()
        {
            CloudQueueHelper.TryGetQueue("readcommandqueue", out this.readCommandQueue);
            this.readCommandQueue.ClearAsync();

            CloudQueueHelper.TryGetQueue("writecommandqueue", out this.writeCommandQueue);
            this.writeCommandQueue.ClearAsync();

            CloudQueueHelper.TryGetQueue("mucommandqueue", out this.modelUpdateCommandQueue);
            this.modelUpdateCommandQueue.ClearAsync();

            this.modelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            this.modelUpdateAccessClient = ScadaModelUpdateAccessClient.CreateClient();
        }

        public async Task Start(bool isRetry = false)
        {
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
                    CloudQueueMessage message = modelUpdateCommandQueue.GetMessage();
                    if(message!=null)
                    {
                        IModbusFunction currentCommand = (IModbusFunction)(Serialization.ByteArrayToObject(message.AsBytes));
                        modelUpdateCommandQueue.DeleteMessage(message);
                        await ExecuteCommand(currentCommand);
                    }
                }

                while (writeCommandQueue.PeekMessage() != null)
                {
                    CloudQueueMessage message = writeCommandQueue.GetMessage();
                    if(message!=null)
                    {
                        IModbusFunction currentCommand = (IModbusFunction)(Serialization.ByteArrayToObject(message.AsBytes));
                        writeCommandQueue.DeleteMessage(message);
                        await ExecuteCommand(currentCommand);
                    }
                }

                while (readCommandQueue.PeekMessage() != null)
                {
                    CloudQueueMessage message = readCommandQueue.GetMessage();
                    if (message != null)
                    {
                        IModbusFunction currentCommand = (IModbusFunction)(Serialization.ByteArrayToObject(message.AsBytes));
                        readCommandQueue.DeleteMessage(message);
                        await ExecuteCommand(currentCommand);
                    }
                }
            }
            catch (Exception ex)
            {
                //if (!isRetry)
                //{
                //    await Task.Delay(2000);

                //    this.modelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
                //    this.modelUpdateAccessClient = ScadaModelUpdateAccessClient.CreateClient();
                //    await Start(true);
                //}
                //else
                {
                    string message = "Exception caught in FunctionExecutorCycle.Start method.";
                    Logger.LogError(message, ex);
                    throw ex;
                }
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
                string message = "Exception caught in InitializeModbusClient().";
                Logger.LogError(message, e);
            }

            if (modbusClient == null)
            {
                throw new Exception("InitializeModbusClient failed: ModbusClient is null");
            }
        }

        private void ConnectToModbusClient()
        {
            int maxNumberOfTries = 160;
            int numberOfTries = 0;
            int sleepInterval = 500;

            string message = $"Connecting to modbus client...";
            Trace.WriteLine(message);
            Logger.LogInfo(message);

            while (!modbusClient.Connected)
            {
                try
                {
                    modbusClient.Connect();
                }
                catch (ConnectionException ce)
                {
                    Logger.LogWarn("ConnectionException on ModbusClient.Connect().", ce);
                }

                if (!modbusClient.Connected)
                {
                    numberOfTries++;
                    Logger.LogDebug($"Connecting try number: {numberOfTries}.");

                    if (numberOfTries >= 100)
                    {
                        sleepInterval = 1000;
                    }

                    Thread.Sleep(sleepInterval);
                }
                else if (!modbusClient.Connected && numberOfTries == maxNumberOfTries)
                {
                    string timeoutMessage = $"Failed to connect to Modbus client by exceeding the maximum number of connection retries ({maxNumberOfTries}).";
                    Logger.LogError(timeoutMessage);
                    throw new Exception(timeoutMessage);
                }
                else
                {
                    message = $"Successfully connected to modbus client.";
                    Trace.WriteLine(message);
                    Logger.LogInfo(message);
                }
            }
        }
        #endregion ModbusClient

        private async Task ExecuteCommand(IModbusFunction command)
        {
            switch(command.FunctionCode)
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
            ModbusFunctionCode functionCode = readCommand.FunctionCode;
            ushort startAddress = readCommand.StartAddress;
            ushort quantity = readCommand.Quantity;

            if (quantity <= 0)
            {
                string message = $"Reading Quantity: {quantity} does not make sense.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            if (startAddress + quantity >= ushort.MaxValue || startAddress + quantity == ushort.MinValue || startAddress == ushort.MinValue)
            {
                string message = $"Address is out of bound. Start address: {startAddress}, Quantity: {quantity}";
                Logger.LogError(message);
                throw new Exception(message);
            }

            if(functionCode == ModbusFunctionCode.READ_COILS || functionCode == ModbusFunctionCode.READ_DISCRETE_INPUTS)
            {
                await ExecuteDiscreteReadCommand(functionCode, startAddress, quantity);
            }
            else if (functionCode == ModbusFunctionCode.READ_HOLDING_REGISTERS || functionCode == ModbusFunctionCode.READ_INPUT_REGISTERS)
            {
                await ExecuteAnalogReadCommand(functionCode, startAddress, quantity);
            }
        }

        private async Task ExecuteDiscreteReadCommand(ModbusFunctionCode functionCode, ushort startAddress, ushort quantity)
        {
            bool[] data;
            PointType pointType;

            if (functionCode == ModbusFunctionCode.READ_COILS)
            {
                pointType = PointType.DIGITAL_OUTPUT;
                data = modbusClient.ReadCoils(startAddress - 1, quantity);
            }
            else if(functionCode == ModbusFunctionCode.READ_DISCRETE_INPUTS)
            {
                pointType = PointType.DIGITAL_INPUT;
                data = modbusClient.ReadDiscreteInputs(startAddress - 1, quantity);
            }
            else
            {
                string message = "ExecuteDiscreteReadCommand: function code is neither ModbusFunctionCode.READ_COILS nor ModbusFunctionCode.READ_DISCRETE_INPUTS";
                throw new ArgumentException(message);
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

                //for commands enqueued during model update
                if (!currentAddressToGidMap[(short)pointType].ContainsKey(address))
                {
                    Logger.LogWarn($"ExecuteDiscreteReadCommand => trying to read value on address {address}, Point type: {pointType}, which is not in the current SCADA Model.");
                    continue;
                }

                long gid = currentAddressToGidMap[(short)pointType][address];

                //for commands enqueued during model update
                if (!currentSCADAModel.ContainsKey(gid))
                {
                    Logger.LogWarn($"ExecuteDiscreteReadCommand => trying to read value for measurement with gid: 0x{gid:X16}, which is not in the current SCADA Model.");
                    continue;
                }

                if (!(currentSCADAModel[gid] is IDiscretePointItem pointItem))
                {
                    string message = $"PointItem [Gid: 0x{gid:X16}] does not implement {typeof(IDiscretePointItem)}.";
                    Logger.LogError(message);
                    throw new InternalSCADAServiceException(message);
                }

                if (pointItem.CurrentValue != value)
                {
                    pointItem.CurrentValue = value;//TODO: Update na provideru
                    Logger.LogInfo($"Alarm for Point [Gid: 0x{pointItem.Gid:X16}, Address: {pointItem.Address}] set to {pointItem.Alarm}.");
                }

                CommandOriginType commandOrigin = CommandOriginType.OTHER_COMMAND;

                if (commandValuesCache.ContainsKey(gid) && commandValuesCache[gid].Value == value)
                {
                    commandOrigin = commandValuesCache[gid].CommandOrigin;
                    commandValuesCache.Remove(gid); //TODO: Update na provider-u
                    Logger.LogDebug($"[ExecuteDiscreteReadCommand] Command origin of command address: {pointItem.Address} is set to {commandOrigin}.");
                }

                DiscreteModbusData digitalData = new DiscreteModbusData(value, pointItem.Alarm, gid, commandOrigin);
                measurementCache.Add(gid, digitalData);
            }
            
            await this.modelUpdateAccessClient.MakeDiscreteEntryToMeasurementCache(measurementCache, true);
            //await this.modelUpdateAccessClient.MakeDiscreteEntryToMeasurementCache();
            //await this.newScadaModelUpdateAccessClient.MakeDiscreteEntryToMeasurementCache(measurementCache, true);
        }

        private async Task ExecuteAnalogReadCommand(ModbusFunctionCode functionCode, ushort startAddress, ushort quantity)
        {
            int[] data;
            PointType pointType;

            if (functionCode == ModbusFunctionCode.READ_HOLDING_REGISTERS)
            {
                pointType = PointType.ANALOG_OUTPUT;
                data = modbusClient.ReadHoldingRegisters(startAddress - 1, quantity);
            }
            else if (functionCode == ModbusFunctionCode.READ_INPUT_REGISTERS)
            {
                pointType = PointType.ANALOG_INPUT;
                data = modbusClient.ReadInputRegisters(startAddress - 1, quantity);
            }
            else
            {
                string message = "ExecuteAnalogReadCommand: function code is neither ModbusFunctionCode.READ_HOLDING_REGISTERS nor ModbusFunctionCode.READ_INPUT_REGISTERS";
                throw new ArgumentException(message);
            }

            var measurementCache = new Dictionary<long, AnalogModbusData>(data.Length);

            ScadaModelReadAccessClient modelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            var currentSCADAModel = await modelReadAccessClient.GetGidToPointItemMap();
            var currentAddressToGidMap = await modelReadAccessClient.GetAddressToGidMap();
            var commandValuesCache = await modelReadAccessClient.GetCommandDescriptionCache();

            for (ushort i = 0; i < data.Length; i++)
            {
                ushort address = (ushort)(startAddress + i);
                int rawValue = data[i];

                //for commands enqueued during model update
                if (!currentAddressToGidMap[(short)pointType].ContainsKey(address))
                {
                    Logger.LogWarn($"ExecuteAnalogReadCommand => trying to read value on address {address}, Point type: {pointType}, which is not in the current SCADA Model.");
                    continue;
                }

                long gid = currentAddressToGidMap[(short)pointType][address];

                //for commands enqueued during model update
                if (!currentSCADAModel.ContainsKey(gid))
                {
                    Logger.LogWarn($"ExecuteAnalogReadCommand => trying to read value for measurement with gid: 0x{gid:X16}, which is not in the current SCADA Model.");
                    continue;
                }

                if (!(currentSCADAModel[gid] is IAnalogPointItem pointItem))
                {
                    string message = $"PointItem [Gid: 0x{gid:X16}] does not implement {typeof(IAnalogPointItem)}.";
                    Logger.LogError(message);
                    throw new Exception(message);
                }


                float eguValue = pointItem.RawToEguValueConversion(rawValue);
                if (pointItem.CurrentEguValue != eguValue)
                {
                    pointItem.CurrentEguValue = eguValue; //TODO: Update na provideru
                    Logger.LogInfo($"Alarm for Point [Gid: 0x{pointItem.Gid:X16}, Address: {pointItem.Address}] set to {pointItem.Alarm}.");
                }

                CommandOriginType commandOrigin = CommandOriginType.OTHER_COMMAND;

                if (commandValuesCache.ContainsKey(gid) && commandValuesCache[gid].Value == pointItem.CurrentRawValue)
                {
                    commandOrigin = commandValuesCache[gid].CommandOrigin;
                    commandValuesCache.Remove(gid); //TODO: Update na provider-u
                    Logger.LogDebug($"[ExecuteAnalogReadCommand] Command origin of command address: {pointItem.Address} is set to {commandOrigin}.");
                }

                AnalogModbusData digitalData = new AnalogModbusData(pointItem.CurrentEguValue, pointItem.Alarm, gid, commandOrigin);
                measurementCache.Add(gid, digitalData);
            }
            
            await this.modelUpdateAccessClient.MakeAnalogEntryToMeasurementCache(measurementCache, true);
            //await this.modelUpdateAccessClient.MakeAnalogEntryToMeasurementCache();
            //await this.newScadaModelUpdateAccessClient.MakeAnalogEntryToMeasurementCache(measurementCache, true);
        }
        #endregion Execute Read

        #region Execute Write
        private async Task ExecuteWriteSingleCommand(IWriteSingleFunction writeCommand)
        {
            PointType pointType;
            ushort outputAddress = writeCommand.OutputAddress;
            int commandValue = writeCommand.CommandValue;

            if (outputAddress >= ushort.MaxValue || outputAddress == ushort.MinValue)
            {
                string message = $"Address is out of bound. Output address: {outputAddress}.";
                Logger.LogError(message);
                throw new Exception(message);
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
                    throw new ArgumentException("Non-boolean value in write single coil command parameter.");
                }

                modbusClient.WriteSingleCoil(outputAddress - 1, booleanCommand);
                Logger.LogInfo($"ExecuteWriteSingleCommand [Discrete] executed SUCCESSFULLY. OutputAddress: {outputAddress}, Value: {booleanCommand}");
            }
            else if (writeCommand.FunctionCode == ModbusFunctionCode.WRITE_SINGLE_REGISTER)
            {
                pointType = PointType.ANALOG_OUTPUT;
                modbusClient.WriteSingleRegister(outputAddress - 1, commandValue);
                Logger.LogInfo($"ExecuteWriteSingleCommand [Analog] executed SUCCESSFULLY. OutputAddress: {outputAddress}, Value: {commandValue}");
            }
            else
            {
                string message = "ExecuteAnalogReadCommand: function code is neither ModbusFunctionCode.READ_HOLDING_REGISTERS nor ModbusFunctionCode.READ_INPUT_REGISTERS";
                throw new ArgumentException(message);
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

                await this.modelUpdateAccessClient.UpdateCommandDescription(gid, commandDescription);
                //await this.modelUpdateAccessClient.UpdateCommandDescription();
                //await this.newScadaModelUpdateAccessClient.UpdateCommandDescription(gid, commandDescription);
            }
        }

        private async Task ExecuteWriteMultipleCommand(IWriteMultipleFunction writeCommand)
        {
            ushort startAddress = writeCommand.StartAddress;
            int quantity = writeCommand.CommandValues.Length;

            if (startAddress >= ushort.MaxValue || startAddress == ushort.MinValue)
            {
                string message = $"ExecuteWriteMultipleCommand => Address is out of bound. Output address: {startAddress}.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            if (writeCommand.StartAddress + quantity >= ushort.MaxValue || startAddress + quantity == ushort.MinValue || writeCommand.StartAddress == ushort.MinValue)
            {
                string message = $"ExecuteWriteMultipleCommand => Address is out of bound. Start address: {startAddress}, Quantity: {quantity}";
                Logger.LogError(message);
                throw new Exception(message);
            }

            if (writeCommand.FunctionCode == ModbusFunctionCode.WRITE_MULTIPLE_COILS)
            {
                await ExecuteWriteMultipleDiscreteCommand(startAddress, writeCommand.CommandValues, writeCommand.CommandOrigin);
            }
            else if (writeCommand.FunctionCode == ModbusFunctionCode.WRITE_MULTIPLE_REGISTERS)
            {
                await ExecuteWriteMultipleAnalogCommand(startAddress, writeCommand.CommandValues, writeCommand.CommandOrigin);
            }
            else
            {
                string message = "ExecuteWriteMultipleCommand: function code is neither ModbusFunctionCode.WRITE_MULTIPLE_COILS nor ModbusFunctionCode.WRITE_MULTIPLE_REGISTERS";
                throw new ArgumentException(message);
            }
        }

        private async Task ExecuteWriteMultipleDiscreteCommand(ushort startAddress, int[] commandValues, CommandOriginType commandOrigin)
        {
            int quantity = commandValues.Length;
            var addressToGidMap = await this.modelReadAccessClient.GetAddressToGidMap();
            var commandDescriptions = new Dictionary<long, CommandDescription>();

            bool[] booleanCommands = new bool[quantity];
            for (ushort index = 0; index < quantity; index++)
            {
                ushort address = (ushort)(startAddress + index);

                if (commandValues[index] == 0)
                {
                    booleanCommands[index] = false;
                }
                else if (commandValues[index] == 1)
                {
                    booleanCommands[index] = true;
                }
                else
                {
                    throw new ArgumentException("ExecuteWriteMultipleDiscreteCommand => Non-boolean value in write single coil command parameter.");
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

                    commandDescriptions.Add(gid, commandDescription);
                }
            }

            modbusClient.WriteMultipleCoils(startAddress - 1, booleanCommands);
            Logger.LogInfo($"ExecuteWriteMultipleDiscreteCommand executed SUCCESSFULLY. StartAddress: {startAddress}, Quantity: {quantity}");

            foreach (CommandDescription description in commandDescriptions.Values)
            {
                //TODO: parallelization
                await this.modelUpdateAccessClient.UpdateCommandDescription(description.Gid, description);
                //await this.modelUpdateAccessClient.UpdateCommandDescription();
                //await this.newScadaModelUpdateAccessClient.UpdateCommandDescription(description.Gid, description);
            }
        }

        private async Task ExecuteWriteMultipleAnalogCommand(ushort startAddress, int[] commandValues, CommandOriginType commandOrigin)
        {
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

                    commandDescriptions.Add(gid, commandDescription);
                }
            }

            modbusClient.WriteMultipleRegisters(startAddress - 1, commandValues);
            Logger.LogInfo($"ExecuteWriteMultipleAnalogCommand executed SUCCESSFULLY. StartAddress: {startAddress}, Quantity: {quantity}");

            foreach (CommandDescription description in commandDescriptions.Values)
            {
                //TODO: parallelization
                await this.modelUpdateAccessClient.UpdateCommandDescription(description.Gid, description);
                //await this.modelUpdateAccessClient.UpdateCommandDescription();
                //await this.newScadaModelUpdateAccessClient.UpdateCommandDescription(description.Gid, description);
            }
        }
        #endregion Execute Write
    }
}
