using OMS.Common.SCADA;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OMS.Common.ScadaContracts.DataContracts.ModbusFunctions;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.ScadaContracts.Commanding;
using OMS.Common.WcfClient.SCADA;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Exceptions.SCADA;
using System.ServiceModel;
using OMS.Common.ScadaContracts.FunctionExecutior;
using OMS.Common.ScadaContracts.ModelProvider;

namespace SCADA.CommandingImplementation
{
    public class CommandingProvider : IScadaCommandingContract
    {
        private readonly string baseLogString;

        #region Private Properties
        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        public CommandingProvider()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
            Logger.LogDebug(debugMessage);
        }

        #region IScadaCommandingContract
        public async Task SendSingleAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType)
        {
            string verboseMessage = $"{baseLogString} SendSingleAnalogCommand method called. gid: {gid:X16}, commandingValue: {commandingValue}, commandOriginType: {commandOriginType}";
            Logger.LogVerbose(verboseMessage);

            IScadaModelReadAccessContract scadaModelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            Dictionary<long, IScadaModelPointItem> gidToPointItemMap = await scadaModelReadAccessClient.GetGidToPointItemMap();

            if (gidToPointItemMap == null)
            {
                string message = $"{baseLogString} SendSingleAnalogCommand => SendSingleAnalogCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            if (!gidToPointItemMap.ContainsKey(gid))
            {
                string message = $"{baseLogString} SendSingleAnalogCommand => Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            IScadaModelPointItem pointItem = gidToPointItemMap[gid];

            if (!(pointItem is IAnalogPointItem analogPointItem && pointItem.RegisterType == PointType.ANALOG_OUTPUT))
            {
                string message = $"{baseLogString} SendSingleAnalogCommand => Either RegistarType of entity with gid: 0x{gid:X16} is not ANALOG_OUTPUT or entity does not implement IAnalogPointItem interface.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            try
            {
                if (!analogPointItem.Initialized)
                {
                    string errorMessage = $"{baseLogString} SendSingleAnalogCommand => PointItem was initialized. Gid: 0x{analogPointItem.Gid:X16}, Addres: {analogPointItem.Address}, Name: {analogPointItem.Name}, RegisterType: {analogPointItem.RegisterType}, Initialized: {analogPointItem.Initialized}";
                    Logger.LogError(errorMessage);
                }

                //LOGIC
                int modbusValue = analogPointItem.EguToRawValueConversion(commandingValue);

                string debugMessage = $"{baseLogString} SendSingleAnalogCommand => Calling SendSingleCommand({pointItem}, {modbusValue}, {commandOriginType})";
                Logger.LogDebug(verboseMessage);

                //KEY LOGIC
                await SendSingleCommand(pointItem, modbusValue, commandOriginType);

                debugMessage = $"{baseLogString} SendSingleAnalogCommand => SendSingleCommand() executed SUCCESSFULLY";
                Logger.LogDebug(debugMessage);
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} SendSingleAnalogCommand => Exception in SendAnalogCommand() method.";
                Logger.LogError(message, e);
                throw new InternalSCADAServiceException(message, e);
            }
        }

        public async Task SendMultipleAnalogCommand(Dictionary<long, float> commandingValues, CommandOriginType commandOriginType)
        {
            if(commandingValues.Count == 0)
            {
                string warnMessage = $"{baseLogString} SendMultipleAnalogCommand => commandingValues is empty and thus aborting the call.";
                Logger.LogWarning(warnMessage);
                return;
            }

            ushort startAddress = 1; //EasyModbus spec
            IScadaModelReadAccessContract scadaModelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            Dictionary<long, IScadaModelPointItem> gidToPointItemMap = await scadaModelReadAccessClient.GetGidToPointItemMap();
            Dictionary<short, Dictionary<ushort, long>> addressToGidMap = await scadaModelReadAccessClient.GetAddressToGidMap();

            if (gidToPointItemMap == null)
            {
                string message = $"{baseLogString} SendMultipleAnalogCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            int analogOutputCount = addressToGidMap[(short)PointType.ANALOG_OUTPUT].Count;
            int[] multipleCommandingValues = new int[addressToGidMap[(short)PointType.ANALOG_OUTPUT].Count];

            //for (ushort address = 1; address <= analogOutputCount; address++)
            //{
            foreach(ushort address in addressToGidMap[(short)PointType.ANALOG_OUTPUT].Keys)
            { 
                long gid = addressToGidMap[(short)PointType.ANALOG_OUTPUT][address];

                if (!gidToPointItemMap.ContainsKey(gid))
                {
                    string message = $"{baseLogString} SendMultipleAnalogCommand => Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                    Logger.LogError(message);
                    throw new ArgumentException(message);
                }
                else if(!(gidToPointItemMap[gid] is IAnalogPointItem analogPointItem))
                {
                    string message = $"{baseLogString} SendMultipleAnalogCommand => Entity with gid: 0x{gid:X16} does not implement IAnalogPointItem interface.";
                    Logger.LogError(message);
                    throw new InternalSCADAServiceException(message);
                }
                else
                {
                    if (!analogPointItem.Initialized)
                    {
                        string errorMessage = $"{baseLogString} SendSingleAnalogCommand => PointItem was initialized. Gid: 0x{analogPointItem.Gid:X16}, Addres: {analogPointItem.Address}, Name: {analogPointItem.Name}, RegisterType: {analogPointItem.RegisterType}, Initialized: {analogPointItem.Initialized}";
                        Logger.LogError(errorMessage);
                    }

                    int commandingValue;

                    if (commandingValues.ContainsKey(gid))
                    {
                        commandingValue = analogPointItem.EguToRawValueConversion(commandingValues[gid]);
                    }
                    else
                    {
                        commandingValue = analogPointItem.CurrentRawValue;
                    }

                    if(address <= analogOutputCount)
                    {
                        multipleCommandingValues[address - 1] = commandingValue;
                    }
                    else
                    {
                        string errorMessage = $"{baseLogString} SendMultipleAnalogCommand => PointItem addresses of ANALOG entities are not successive. This can happen due to cim/xml being invalid.";
                        Logger.LogError(errorMessage);
                        throw new Exception(errorMessage);
                    }
                }
            }

            try
            {
                string debugMessage = $"{baseLogString} SendMultipleAnalogCommand => Calling SendMultipleCommand({ModbusFunctionCode.WRITE_MULTIPLE_REGISTERS}, {startAddress}, {multipleCommandingValues}, {commandOriginType})";
                Logger.LogDebug(debugMessage);

                //KEY LOGIC
                await SendMultipleCommand(ModbusFunctionCode.WRITE_MULTIPLE_REGISTERS, startAddress, multipleCommandingValues, commandOriginType);

                debugMessage = $"{baseLogString} SendMultipleAnalogCommand => SendMultipleCommand() executed SUCCESSFULLY";
                Logger.LogDebug(debugMessage);
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} SendMultipleAnalogCommand => Exception: {e.Message}.";
                Logger.LogError(message, e);
                throw new InternalSCADAServiceException(message, e);
            }
        }

        public async Task SendSingleDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType)
        {
            IScadaModelReadAccessContract scadaModelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            Dictionary<long, IScadaModelPointItem> gidToPointItemMap = await scadaModelReadAccessClient.GetGidToPointItemMap();

            if (gidToPointItemMap == null)
            {
                string message = $"{baseLogString} SendSingleDiscreteCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            if (!gidToPointItemMap.ContainsKey(gid))
            {
                string message = $"{baseLogString} SendSingleDiscreteCommand => Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            IScadaModelPointItem pointItem = gidToPointItemMap[gid];

            if (!(pointItem is IDiscretePointItem && pointItem.RegisterType == PointType.DIGITAL_OUTPUT))
            {
                string message = $"{baseLogString} SendSingleDiscreteCommand => RegistarType of entity with gid: 0x{gid:X16} is not DIGITAL_OUTPUT or entity does not implement IDiscretePointItem interface.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            try
            {
                string debugMessage = $"{baseLogString} SendSingleDiscreteCommand => Calling SendSingleCommand({pointItem}, {commandingValue}, {commandOriginType})";
                Logger.LogDebug(debugMessage);

                //KEY LOGIC
                await SendSingleCommand(pointItem, commandingValue, commandOriginType);

                debugMessage = $"{baseLogString} SendSingleDiscreteCommand => SendSingleCommand() executed SUCCESSFULLY";
                Logger.LogDebug(debugMessage);
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} SendSingleDiscreteCommand => Exception: {e.Message}.";
                Logger.LogError(message, e);
                throw new InternalSCADAServiceException(message, e);
            }
        }

        public async Task SendMultipleDiscreteCommand(Dictionary<long, ushort> commandingValues, CommandOriginType commandOriginType)
        {
            if (commandingValues.Count == 0)
            {
                string warnMessage = $"{baseLogString} SendMultipleDiscreteCommand => commandingValues is empty and thus aborting the call.";
                Logger.LogWarning(warnMessage);
                return;
            }

            ushort startAddress = 1; //EasyModbus spec
            IScadaModelReadAccessContract scadaModelReadAccessClient = ScadaModelReadAccessClient.CreateClient();
            Dictionary<long, IScadaModelPointItem> gidToPointItemMap = await scadaModelReadAccessClient.GetGidToPointItemMap();
            Dictionary<short, Dictionary<ushort, long>> addressToGidMap = await scadaModelReadAccessClient.GetAddressToGidMap();

            if (gidToPointItemMap == null)
            {
                string message = $"{baseLogString} SendMultipleDiscreteCommand => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            int digitalOutputCount = addressToGidMap[(short)PointType.DIGITAL_OUTPUT].Count;
            int[] multipleCommandingValues = new int[addressToGidMap[(short)PointType.DIGITAL_OUTPUT].Count];

            foreach (ushort address in addressToGidMap[(short)PointType.DIGITAL_OUTPUT].Keys)
            {
                long gid = addressToGidMap[(short)PointType.DIGITAL_OUTPUT][address];

                if (!gidToPointItemMap.ContainsKey(gid))
                {
                    string message = $"{baseLogString} SendMultipleDiscreteCommand => Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                    Logger.LogError(message);
                    throw new ArgumentException(message);
                }
                else if (!(gidToPointItemMap[gid] is IDiscretePointItem discretePointItem))
                {
                    string message = $"{baseLogString} SendMultipleDiscreteCommand => Entity with gid: 0x{gid:X16} does not implement IDiscretePointItem interface.";
                    Logger.LogError(message);
                    throw new InternalSCADAServiceException(message);
                }
                else
                {
                    int commandingValue;

                    if (commandingValues.ContainsKey(gid))
                    {
                        commandingValue = commandingValues[gid];
                    }
                    else
                    {
                        commandingValue = discretePointItem.CurrentValue;
                    }

                    if (address <= digitalOutputCount)
                    {
                        multipleCommandingValues[address - 1] = commandingValue;
                    }
                    else
                    {
                        string errorMessage = $"{baseLogString} SendMultipleDiscreteCommand => PointItem addresses of DISCRETE entities are not successive. This can happen due to cim/xml being invalid.";
                        Logger.LogError(errorMessage);
                        throw new Exception(errorMessage);
                    }
                }
            }

            try
            {
                string debugMessage = $"{baseLogString} SendMultipleDiscreteCommand => Calling SendMultipleCommand({ModbusFunctionCode.WRITE_MULTIPLE_COILS}, {multipleCommandingValues}, {commandOriginType})";
                Logger.LogDebug(debugMessage);

                //KEY LOGIC
                await SendMultipleCommand(ModbusFunctionCode.WRITE_MULTIPLE_COILS, startAddress, multipleCommandingValues, commandOriginType);

                debugMessage = $"{baseLogString} SendMultipleDiscreteCommand => SendMultipleCommand() executed SUCCESSFULLY";
                Logger.LogDebug(debugMessage);
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} SendMultipleDiscreteCommand => Exception: {e.Message}.";
                Logger.LogError(message, e);
                throw new InternalSCADAServiceException(message, e);
            }
        }
        #endregion IScadaCommandingContract
        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        private async Task SendSingleCommand(IScadaModelPointItem pointItem, int commandingValue, CommandOriginType commandOriginType, bool isRetry = false)
        { 
            try
            {
                ModbusFunctionCode functionCode;

                if (pointItem.RegisterType == PointType.ANALOG_OUTPUT)
                {
                    functionCode = ModbusFunctionCode.WRITE_SINGLE_REGISTER;
                }
                else if (pointItem.RegisterType == PointType.DIGITAL_OUTPUT)
                {
                    functionCode = ModbusFunctionCode.WRITE_SINGLE_COIL;
                }
                else
                {
                    string errorMessage = $"{baseLogString} SendSingleCommand => Commanding arguments are not valid. Registry type: {pointItem.RegisterType}, expected: {PointType.ANALOG_OUTPUT}, {PointType.DIGITAL_OUTPUT}";
                    Logger.LogError(errorMessage);
                    throw new ArgumentException(errorMessage);
                }

                IWriteModbusFunction modbusFunction = new WriteSingleFunction(functionCode, pointItem.Address, commandingValue, commandOriginType);
                
                IWriteCommandEnqueuerContract commandEnqueuerClient = WriteCommandEnqueuerClient.CreateClient();
                await commandEnqueuerClient.EnqueueWriteCommand(modbusFunction);

                string message = $"{baseLogString} SendSingleCommand => Command SUCCESSFULLY enqueued. Function code: {modbusFunction.FunctionCode}, Origin: {modbusFunction.CommandOrigin}";
                Logger.LogInformation(message);
            }
            catch (Exception e)
            {
                if (!isRetry)
                {
                    await Task.Delay(2000);
                    await SendSingleCommand(pointItem, commandingValue, commandOriginType, true);
                }
                else
                {
                    string message = $"{baseLogString} SendSingleCommand => Exception: {e.Message}.";
                    Logger.LogError(message, e);
                    throw new InternalSCADAServiceException(message, e);
                }
            }
        }

        private async Task SendMultipleCommand(ModbusFunctionCode functionCode, ushort startAddress, int[] commandingValues, CommandOriginType commandOriginType, bool isRetry = false)
        {
            try
            {
                //KEY LOGIC
                IWriteModbusFunction modbusFunction = new WriteMultipleFunction(functionCode, startAddress, commandingValues, commandOriginType);
                
                IWriteCommandEnqueuerContract commandEnqueuerClient = WriteCommandEnqueuerClient.CreateClient();
                await commandEnqueuerClient.EnqueueWriteCommand(modbusFunction);
                
                string message = $"{baseLogString} SendMultipleCommand => Command SUCCESSFULLY enqueued. Function code: {modbusFunction.FunctionCode}, Origin: {modbusFunction.CommandOrigin}";
                Logger.LogInformation(message);
            }
            catch (Exception e)
            {
                if (!isRetry)
                {
                    await Task.Delay(2000);
                    await SendMultipleCommand(functionCode, startAddress, commandingValues, commandOriginType, true);
                }
                else
                {
                    string message = $"{baseLogString} SendMultipleCommand => Exception: {e.Message}";
                    Logger.LogError(message, e);
                    throw new InternalSCADAServiceException(message, e);
                }
            }
        }
    }
}
