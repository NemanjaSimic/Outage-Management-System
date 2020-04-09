using EasyModbus;
using EasyModbus.Exceptions;
using Microsoft.WindowsAzure.Storage.Queue;
using OMS.Cloud.SCADA.Data.Repository;
using OMS.Common.Cloud;
using OMS.Common.Cloud.AzureStorageHelpers;
using OMS.Common.Cloud.WcfServiceFabricClients.SCADA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Cloud.SCADA.FunctionExecutorService
{
    internal class FunctionExecutorCycle
    {
        private ILogger logger;
        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private readonly CloudQueue readCommandQueue;
        private readonly CloudQueue writeCommandQueue;
        private readonly CloudQueue modelUpdateCommandQueue;

        private ISCADAConfigData configData;
        private ModbusClient modbusClient;
        
        public FunctionExecutorCycle()
        {
            InitializeModbusClient();

            CloudQueueHelper.TryGetQueue("readcommandqueue", out this.readCommandQueue);
            CloudQueueHelper.TryGetQueue("writecommandqueue", out this.writeCommandQueue);
            CloudQueueHelper.TryGetQueue("mucommandqueue", out this.modelUpdateCommandQueue);
        }

        public async void Start()
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
                    ExecuteCommand(currentCommand);
                }
                
                //HIGH PRIORITY COMMANDS - model update commands

                //this.modelUpdateQueueEmptyEvent.Set();

                //WRITE COMMANDS

                while (writeCommandQueue.PeekMessage() != null)
                {
                    CloudQueueMessage message = writeCommandQueue.GetMessage();
                    IWriteModbusFunction currentCommand = (IWriteModbusFunction)(Serialization.ByteArrayToObject(message.AsBytes));
                    modelUpdateCommandQueue.DeleteMessage(message);
                    ExecuteCommand(currentCommand);
                }


                //this.writeCommandQueueEmptyEvent.Set();

                //READ COMMANDS - acquisition
                while (readCommandQueue.PeekMessage() != null)
                {
                    CloudQueueMessage message = readCommandQueue.GetMessage();
                    IWriteModbusFunction currentCommand = (IWriteModbusFunction)(Serialization.ByteArrayToObject(message.AsBytes));
                    modelUpdateCommandQueue.DeleteMessage(message);
                    ExecuteCommand(currentCommand);
                }
            }
            catch (Exception ex)
            {
                string message = "Exception caught in FunctionExecutorThread.";
                Logger.LogError(message, ex);
            }
        }

        private async Task InitializeModbusClient()
        {
            ScadaModelReadAccessClient modelReadAccess = ScadaModelReadAccessClient.CreateClient();
            this.configData = await modelReadAccess.GetScadaConfigData();
            this.modbusClient = new ModbusClient(configData.IpAddress.ToString(), configData.TcpPort);
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

        private void ExecuteCommand(IModbusFunction command)
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
                //todo: MakeAnalogEntryToMeasurementCache(readAnalogCommand.Data, true); POZIV KA PROVIDER
            }
            else if (command is IReadDiscreteModbusFunction readDiscreteCommand)
            {
                //todo: MakeDiscreteEntryToMeasurementCache(readDiscreteCommand.Data, true); POZIV KA PROVIDER
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

                SCADAModel SCADAModel = null; //TODO: dobaviti od providera
                if (SCADAModel.CurrentAddressToGidMap[pointType].ContainsKey(commandValue.Address))
                {
                    long gid = SCADAModel.CurrentAddressToGidMap[pointType][commandValue.Address];

                    SCADAModel.CommandedValuesCache[gid] = commandValue;
                    //TODO: update na provideru
                }
            }
        }
    }
}
