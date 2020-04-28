using EasyModbus;
using EasyModbus.Exceptions;
using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Common.Cloud;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.Cloud.WcfServiceFabricClients.SCADA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using Outage.Common;
using System;
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

        private ScadaConfigData configData;
        private ModbusClient modbusClient;

        public FunctionExecutorCycle()
        {
            //InitializeModbusClient();
            CloudQueueHelper.TryGetQueue("readcommandqueue", out this.readCommandQueue);
            CloudQueueHelper.TryGetQueue("writecommandqueue", out this.writeCommandQueue);
            CloudQueueHelper.TryGetQueue("mucommandqueue", out this.modelUpdateCommandQueue);
        }

        public async Task Start()
        {
            try
            {
                if (modbusClient == null)
                {
                    await InitializeModbusClient();
                }

                //Logger.LogDebug("Connected and waiting for command event.");

                //this.commandEvent.WaitOne();

                //Logger.LogDebug("Command event happened.");

                if (!modbusClient.Connected)
                {
                    ConnectToModbusClient();
                }

                while (modelUpdateCommandQueue.PeekMessage() != null)
                {
                    CloudQueueMessage message = modelUpdateCommandQueue.GetMessage();
                    IWriteModbusFunction currentCommand = (IWriteModbusFunction)(Serialization.ByteArrayToObject(message.AsBytes));
                    modelUpdateCommandQueue.DeleteMessage(message);
                    await ExecuteCommand(currentCommand);
                }

                //HIGH PRIORITY COMMANDS - model update commands

                //this.modelUpdateQueueEmptyEvent.Set();

                //WRITE COMMANDS

                while (writeCommandQueue.PeekMessage() != null)
                {
                    CloudQueueMessage message = writeCommandQueue.GetMessage();
                    IWriteModbusFunction currentCommand = (IWriteModbusFunction)(Serialization.ByteArrayToObject(message.AsBytes));
                    modelUpdateCommandQueue.DeleteMessage(message);
                    await ExecuteCommand(currentCommand);
                }


                //this.writeCommandQueueEmptyEvent.Set();

                //READ COMMANDS - acquisition
                while (readCommandQueue.PeekMessage() != null)
                {
                    CloudQueueMessage message = readCommandQueue.GetMessage();
                    IWriteModbusFunction currentCommand = (IWriteModbusFunction)(Serialization.ByteArrayToObject(message.AsBytes));
                    modelUpdateCommandQueue.DeleteMessage(message);
                    await ExecuteCommand(currentCommand);
                }
            }
            catch (Exception ex)
            {
                string message = "Exception caught in Start().";
                Logger.LogError(message, ex);
            }
        }

        private async Task InitializeModbusClient()
        {
            ScadaModelReadAccessClient modelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            
            try
            {
                this.configData = await modelReadAccessClient.GetScadaConfigData();
                this.modbusClient = new ModbusClient(configData.IpAddress.ToString(), configData.TcpPort);
            }
            catch (Exception e)
            {
                string message = "Exception caught in InitializeModbusClient().";
                Logger.LogError(message, e);
            }
        }

        private void ConnectToModbusClient()
        {
            int numberOfTries = 0;
            int sleepInterval = 500;

            string message = $"Connecting to modbus client...";
            Console.WriteLine(message);
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
                else if (!modbusClient.Connected && numberOfTries == int.MaxValue)
                {
                    string timeoutMessage = "Failed to connect to Modbus client by exceeding the maximum number of connection retries.";
                    Logger.LogError(timeoutMessage);
                    throw new Exception(timeoutMessage);
                }
                else
                {
                    message = $"Successfully connected to modbus client.";
                    Console.WriteLine(message);
                    Logger.LogInfo(message);
                }
            }
        }

        private async Task ExecuteCommand(IModbusFunction command)
        {
            try
            {
                command.Execute(modbusClient);
            }
            catch (Exception e)
            {
                string message = "Exception on currentCommand.Execute().";
                Logger.LogWarn(message, e);
                modbusClient.Disconnect();
                return;
            }

            if (command is IReadAnalogModusFunction readAnalogCommand)
            {
                ScadaModelUpdateAccessClient modelUpdateAccessClient = ScadaModelUpdateAccessClient.CreateClient();
                await modelUpdateAccessClient.MakeAnalogEntryToMeasurementCache(readAnalogCommand.Data, true);
            }
            else if (command is IReadDiscreteModbusFunction readDiscreteCommand)
            {
                ScadaModelUpdateAccessClient modelUpdateAccessClient = ScadaModelUpdateAccessClient.CreateClient();
                await modelUpdateAccessClient.MakeDiscreteEntryToMeasurementCache(readDiscreteCommand.Data, true);
            }
            else if (command is IWriteModbusFunction writeModbusCommand)
            {
                CommandDescription commandValue = new CommandDescription()
                {
                    Address = writeModbusCommand.ModbusWriteCommandParameters.OutputAddress,
                    Value = writeModbusCommand.ModbusWriteCommandParameters.Value,
                    CommandOrigin = writeModbusCommand.CommandOrigin,
                };

                PointType pointType;
                switch (writeModbusCommand.ModbusWriteCommandParameters.FunctionCode)
                {
                    case (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER:
                        pointType = PointType.ANALOG_OUTPUT;
                        break;
                    case (byte)ModbusFunctionCode.WRITE_SINGLE_COIL:
                        pointType = PointType.DIGITAL_OUTPUT;
                        break;
                    default:
                        Logger.LogError($"Function code {writeModbusCommand.ModbusWriteCommandParameters.FunctionCode} is not comatible with write command.");
                        return;
                }

                ScadaModelReadAccessClient modelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
                var currentAddressToGidMap = await modelReadAccessClient.GetAddressToGidMap();
                var commandedValuesCache = await modelReadAccessClient.GetCommandDescriptionCache();

                if (currentAddressToGidMap[(ushort)pointType].ContainsKey(commandValue.Address))
                {
                    long gid = currentAddressToGidMap[(ushort)pointType][commandValue.Address];

                    //commandedValuesCache[gid] = commandValue;
                    ScadaModelUpdateAccessClient modelUpdateAccessClient = ScadaModelUpdateAccessClient.CreateClient();
                    await modelUpdateAccessClient.UpdateCommandDescription(gid, commandValue);
                }
            }
        }
    }
}
