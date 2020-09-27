using Common.CE;
using Common.CeContracts;
using Common.CeContracts.LoadFlow;
using Common.CeContracts.ModelProvider;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.WcfClient.CE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CE.LoadFlowImplementation
{
	public class LoadFlow : ILoadFlowContract
    {
        private const int recloserInterval = 20_000;

        #region Private fields
        private readonly string baseLogString;

        private HashSet<long> reclosers;
        private Dictionary<long, ITopologyElement> feeders;
        private Dictionary<long, ITopologyElement> syncMachines;
        private Dictionary<DailyCurveType, DailyCurve> dailyCurves;
        #endregion

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public LoadFlow()
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
            Logger.LogDebug(debugMessage);
        }

		#region ILoadFlowService
		public async Task<TopologyModel> UpdateLoadFlow(TopologyModel inputTopology)
        {
            string verboseMessage = $"{baseLogString} UpdateLoadFlow method called.";
            Logger.LogVerbose(verboseMessage);

            TopologyModel topology = inputTopology;

            try
            {
                Dictionary<long, float> loadOfFeeders = new Dictionary<long, float>();
                feeders = new Dictionary<long, ITopologyElement>();
                syncMachines = new Dictionary<long, ITopologyElement>();
                dailyCurves = DailyCurveReader.ReadDailyCurves();

                Logger.LogDebug($"{baseLogString} UpdateLoadFlow => Getting reclosers from model provider.");
                var modelProviderClient = CeModelProviderClient.CreateClient();
                reclosers = await modelProviderClient.GetReclosers();
                Logger.LogDebug($"{baseLogString} UpdateLoadFlow => Reclosers were delivered successfully.");


                if (topology == null)
                {
                    string message = $"{baseLogString} UpdateLoadFlow => Topology is null.";
                    Logger.LogWarning(message);
                    //throw new Exception(message);
                    return topology;
                }

                await CalculateLoadFlow(topology, loadOfFeeders);

                await UpdateLoadFlowFromRecloser(topology, loadOfFeeders);

                foreach (var syncMachine in syncMachines.Values)
                {
                    await SyncMachine(syncMachine, loadOfFeeders);
                }

                var commands = new Dictionary<long, float>();
                var modbusData = new Dictionary<long, AnalogModbusData>();

                foreach (var loadFeeder in loadOfFeeders)
                {
                    if (feeders.TryGetValue(loadFeeder.Key, out ITopologyElement feeder))
                    {
                        long signalGid = 0;
                        foreach (var measurement in feeder.Measurements)
                        {
                            if (measurement.Value.Equals(AnalogMeasurementType.FEEDER_CURRENT.ToString()))
                            {
                                signalGid = measurement.Key;
                            }
                        }

                        if (signalGid != 0)
                        {
                            Logger.LogDebug($"{baseLogString} UpdateLoadFlow => Calling SendAnalogCommand method from measurement provider. Measurement GID {signalGid:X16}, Value {loadFeeder.Value}.");
                            commands.Add(signalGid, loadFeeder.Value);

                            AlarmType alarmType = (loadFeeder.Value >= 36) ? AlarmType.HIGH_ALARM : AlarmType.NO_ALARM;
                            modbusData.Add(signalGid, new AnalogModbusData(loadFeeder.Value, alarmType, signalGid, CommandOriginType.CE_COMMAND));
                        }
                        else
                        {
                            Logger.LogWarning($"{baseLogString} UpdateLoadFlow => Feeder with GID 0x{feeder.Id:X16} does not have FEEDER_CURRENT measurement.");
                        }
                    }
                }

                var measurementProviderClient = MeasurementProviderClient.CreateClient();

                await measurementProviderClient.SendMultipleAnalogCommand(commands, CommandOriginType.CE_COMMAND);
                Logger.LogDebug($"{baseLogString} UpdateLoadFlow => SendAnalogCommand method from measurement provider successfully finished.");

                Logger.LogDebug($"{baseLogString} UpdateLoadFlow => Calling update analog measurement method from measurement provider.");
                await measurementProviderClient.UpdateAnalogMeasurement(modbusData);
                Logger.LogDebug($"{baseLogString} UpdateLoadFlow => Update analog measurement method from measurement provider successfully finished.");
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} UpdateLoadFlow => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
            }

            return topology;
        }
		#endregion

		#region Private Methods
		private async Task SyncMachine(ITopologyElement element, Dictionary<long, float> loadOfFeeders)
        {
            string verboseMessage = $"{baseLogString} SyncMachine method called. Element with GID {element?.Id:X16}";
            Logger.LogVerbose(verboseMessage);

            if (element == null)
            {
                string message = $"{baseLogString} UpdateLoadFlow => Element is null.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            if (!(element is SynchronousMachine))
            {
                string message = $"{baseLogString} UpdateLoadFlow => Element is not SynchronousMachine.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            AnalogMeasurement powerMeasurement = null;
            AnalogMeasurement voltageMeasurement = null;

            if (element.Feeder != null)
            {
                if(loadOfFeeders.TryGetValue(element.Feeder.Id, out float feederLoad))
                {
                    float machineCurrentChange;
                    if (feederLoad > 36)
                    {
                        float improvementFactor = feederLoad - 36;

                        machineCurrentChange = (((SynchronousMachine)element).Capacity >= improvementFactor)
                                                    ? improvementFactor
                                                    : ((SynchronousMachine)element).Capacity;
                    }
                    else
                    {
                        machineCurrentChange = 0;
                    }

                    foreach (var meas in element.Measurements)
                    {
                        if (meas.Value.Equals(AnalogMeasurementType.POWER.ToString()))
                        {
                            Logger.LogDebug($"{baseLogString} UpdateLoadFlow => Calling GetAnalogMeasurement method from measurement provider. Measurement GID {meas.Key:X16}.");
                            var measurementProviderClient = MeasurementProviderClient.CreateClient();
                            powerMeasurement = await measurementProviderClient.GetAnalogMeasurement(meas.Key);
                            Logger.LogDebug($"{baseLogString} UpdateLoadFlow => GetAnalogMeasurement method called successfully.");

                            if (powerMeasurement == null)
                            {
                                Logger.LogError($"{baseLogString} UpdateLoadFlow => Synchronous machine with GID {element.Id:X16} does not have POWER measurement.");
                            }
                        }
                        
                        if (meas.Value.Equals(AnalogMeasurementType.VOLTAGE.ToString()))
                        {
                            Logger.LogDebug($"{baseLogString} UpdateLoadFlow => Calling GetAnalogMeasurement method from measurement provider. Measurement GID {meas.Key:X16}.");
                            var measurementProviderClient = MeasurementProviderClient.CreateClient();
                            voltageMeasurement = await measurementProviderClient.GetAnalogMeasurement(meas.Key);
                            Logger.LogDebug($"{baseLogString} UpdateLoadFlow => GetAnalogMeasurement method called successfully.");

                            if (voltageMeasurement == null)
                            {
                                Logger.LogError($"{baseLogString} UpdateLoadFlow => Synchronous machine with GID {element.Id:X16} does not have VOLTAGE measurement.");
                            }
                        }
                    }

                    if (powerMeasurement != null && voltageMeasurement != null)
                    {
                        float newNeededPower = machineCurrentChange * voltageMeasurement.GetCurrentValue();
                        float newSMPower = (((SynchronousMachine)element).Capacity >= newNeededPower)
                                                    ? newNeededPower
                                                    : ((SynchronousMachine)element).Capacity;

                        Logger.LogDebug($"{baseLogString} UpdateLoadFlow => Calling SendAnalogCommand method from measurement provider. Measurement GID {powerMeasurement.Id:X16}, Value {newSMPower}.");
                        var measurementProviderClient = MeasurementProviderClient.CreateClient();
                        await measurementProviderClient.SendSingleAnalogCommand(powerMeasurement.Id, newSMPower, CommandOriginType.CE_COMMAND);
                        Logger.LogDebug($"{baseLogString} UpdateLoadFlow => SendAnalogCommand method called successfully.");

                        Dictionary<long, AnalogModbusData> data = new Dictionary<long, AnalogModbusData>(1)
                        {
                            { powerMeasurement.Id, new AnalogModbusData(newSMPower, AlarmType.NO_ALARM, powerMeasurement.Id, CommandOriginType.CE_COMMAND)}
                        };

                        Logger.LogDebug($"{baseLogString} UpdateLoadFlow => Calling UpdateAnalogMeasurement method from measurement provider.");
                        measurementProviderClient = MeasurementProviderClient.CreateClient();
                        await measurementProviderClient.UpdateAnalogMeasurement(data);
                        Logger.LogDebug($"{baseLogString} UpdateLoadFlow => UpdateAnalogMeasurement method called successfully.");

                        loadOfFeeders[element.Feeder.Id] -= newSMPower / voltageMeasurement.GetCurrentValue();
                    }
                    else
                    {
                        Logger.LogError($"{baseLogString} UpdateLoadFlow => Synchronous machine with GID {element.Id:X16} does not have measurements for calculating CURRENT.");
                    }
                }
            }
            else
            {
                Logger.LogError($"{baseLogString} UpdateLoadFlow => Synchronous machine with GID {element.Id:X16} does not belond to any feeder.");
            }
        }
        private async Task CalculateLoadFlow(TopologyModel topology, Dictionary<long, float> loadOfFeeders)
        {
            string verboseMessage = $"{baseLogString} CalculateLoadFlow method called.";
            Logger.LogVerbose(verboseMessage);

            if (topology == null)
            {
                string message = $"{baseLogString} CalculateLoadFlow => Topologies are null.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            if (topology.GetElementByGid(topology.FirstNode, out ITopologyElement firsElement))
            {
                Stack<ITopologyElement> stack = new Stack<ITopologyElement>();
                stack.Push(firsElement);
                ITopologyElement nextElement;
                long currentFider = 0;

                while (stack.Count > 0)
                {
                    nextElement = stack.Pop();

                    if (nextElement is Field field)
                    {
                        stack.Push(field.Members.First());
                    }
                    else
                    {

                        if (nextElement is Feeder)
                        {
                            currentFider = nextElement.Id;
                            if (!feeders.ContainsKey(currentFider))
                            {
                                feeders.Add(currentFider, nextElement);
                            }
                        }

                        Tuple<bool, float> tuple = await IsElementEnergized(nextElement);
                        bool isEnergized = tuple.Item1;
                        float load = tuple.Item2;

                        if (!isEnergized)
                        {
                            await DeEnergizeElementsUnder(nextElement);
                        }
                        else
                        {

                            if (loadOfFeeders.ContainsKey(currentFider))
                            {
                                loadOfFeeders[currentFider] += load;
                            }
                            else
                            {
                                loadOfFeeders.Add(currentFider, load);
                            }

                            foreach (var child in nextElement.SecondEnd)
                            {
                                if (!reclosers.Contains(child.Id))
                                {
                                    stack.Push(child);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.LogDebug($"{baseLogString} CalculateLoadFlowFirst => Element with GID {topology?.FirstNode:X16} does not exist in collection.");
            }

            Logger.LogVerbose($"{baseLogString} CalculateLoadFlowFirst => Calulate load flow finished.");
        }
        private async Task<Tuple<bool, float>> IsElementEnergized(ITopologyElement element)
        {
            string verboseMessage = $"{baseLogString} IsElementEnergized method called => Element: {element?.Id:X16}.";
            Logger.LogVerbose(verboseMessage);

            float load = 0;
            bool pastState = element.IsActive;
            element.IsActive = true;

            foreach (var measurement in element.Measurements)
            {
                if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(measurement.Key) == DMSType.DISCRETE
                    || measurement.Key < 10000)
                {
                    Logger.LogDebug($"{baseLogString} IsElementEnergized => Getting discrete value of {measurement.Key:X16} from measurement provider.");
                    var measurementProviderClient = MeasurementProviderClient.CreateClient();
                    bool isOpen = await measurementProviderClient.GetDiscreteValue(measurement.Key);
                    Logger.LogDebug($"{baseLogString} IsElementEnergized => Discrete value of {measurement.Key:X16} has been delivered successfully. Result is [{isOpen}].");
                    
                    // Value je true ako je prekidac otvoren, tada je element neaktivan
                    element.IsActive = !isOpen;
                    break;
                }
            }

            if (element.IsActive)
            {
                if (!pastState)
                {
                    await TurnOnAllMeasurement(element.Measurements);
                }

                List<AnalogMeasurement> analogMeasurements = await GetMeasurements(element.Measurements);

                if (element.DmsType.Equals(DMSType.SYNCHRONOUSMACHINE.ToString()))
                {
                    if (!syncMachines.ContainsKey(element.Id))
                    {
                        syncMachines.Add(element.Id, element);
                    }
                }
                else
                {
                    if (element.IsRemote)
                    {
                        float power = 0;
                        float voltage = 0;
                        foreach (var analogMeasurement in analogMeasurements)
                        {
                            if (analogMeasurement.GetMeasurementType().Equals(AnalogMeasurementType.POWER.ToString()))
                            {
                                power = analogMeasurement.GetCurrentValue();
                            }
                            else if (analogMeasurement.GetMeasurementType().Equals(AnalogMeasurementType.VOLTAGE.ToString()))
                            {
                                voltage = analogMeasurement.GetCurrentValue();
                            }
                        }

                        if (power != 0 && voltage != 0)
                        {
                            load = (float)Math.Round(power / voltage);
                        }
                    }
                    else if (element is EnergyConsumer consumer)
                    {
                        if(dailyCurves.TryGetValue(EnergyConsumerTypeToDailyCurveConverter.GetDailyCurveType(consumer.Type), out DailyCurve curve))
                        {
                            load = (float)Math.Round(curve.GetValue((short)DateTime.Now.Hour) * 1000 / 220);
                        }
                    }

                }
                
            }

            return new Tuple<bool, float>(element.IsActive, load);
        }
        private async Task DeEnergizeElementsUnder(ITopologyElement element)
        {
            string verboseMessage = $"{baseLogString} DeEnergizeElementsUnder method called => Element: {element?.Id:X16}.";
            Logger.LogVerbose(verboseMessage);

            Stack<ITopologyElement> stack = new Stack<ITopologyElement>();
            stack.Push(element);
            ITopologyElement nextElement;

            while (stack.Count > 0)
            {
                nextElement = stack.Pop();
                nextElement.IsActive = false;
                if (nextElement is Field field)
                {
                    foreach (var member in field.Members)
                    {
                        await TurnOffAllMeasurement(member.Measurements);
                        member.IsActive = false;
                    }
                }

                await TurnOffAllMeasurement(nextElement.Measurements);

                foreach (var child in nextElement.SecondEnd)
                {
                    if (!reclosers.Contains(child.Id))
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        #region RecloserLogic
        private async Task UpdateLoadFlowFromRecloser(TopologyModel topology, Dictionary<long, float> loadOfFeeders)
        {
            string verboseMessage = $"{baseLogString} UpdateLoadFlowFromRecloser method called.";
            Logger.LogVerbose(verboseMessage);

            if (topology == null)
            {
                string message = $"{baseLogString} UpdateLoadFlowFromRecloser => Topologies are null.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            foreach (var recloserGid in reclosers)
            {
                if (topology.GetElementByGid(recloserGid, out ITopologyElement recloser))
                {
                    await CalculateLoadFlowFromRecloser(recloser, topology, loadOfFeeders);
                }
            }
        }
        private async Task<TopologyModel> CalculateLoadFlowFromRecloser(ITopologyElement recloser, TopologyModel topology, Dictionary<long, float> loadOfFeeders)
        {
            string verboseMessage = $"{baseLogString} CalculateLoadFlowFromRecloser method called. Element with GID {recloser?.Id:X16}.";
            Logger.LogVerbose(verboseMessage);

            if (recloser == null)
            {
                string message = $"{baseLogString} CalculateLoadFlowUpsideDown => NULL value has been passed instead of recloser.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            if (topology == null)
            {
                string message = $"{baseLogString} CalculateLoadFlowUpsideDown => NULL value has been passed instead of topology.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            long measurementGid = 0;

            if (recloser.Measurements.Count == 1)
            {
                //dogovor je da prekidaci imaju samo discrete measurement
                measurementGid = recloser.Measurements.First().Key;
            }
            else
            {
                Logger.LogWarning($"{baseLogString} CalculateLoadFlowUpsideDown => Recloser with GID {recloser.Id:X16} does not have proper measurements.");
            }


            bool isEnergized = false;
            if (recloser.FirstEnd != null && recloser.SecondEnd.Count == 1)
            {
                if (recloser.FirstEnd.IsActive && !recloser.SecondEnd.First().IsActive)
                {
                    Tuple<bool, float> tuple = await IsElementEnergized(recloser);
                    isEnergized = tuple.Item1;
                    float load = tuple.Item2;
                    if (isEnergized)
                    {
                        await CalculateLoadFlowUpsideDown(recloser.SecondEnd.First(), recloser.Id, recloser.FirstEnd.Feeder, loadOfFeeders);
                    }
                    else if(!recloser.IsActive)
                    {
                        Thread thread = new Thread(async () => await CommandToRecloser(measurementGid, (int)DiscreteCommandingType.CLOSE, CommandOriginType.CE_COMMAND, recloser));
                        thread.Start();
                    }
                }
                else if (!recloser.FirstEnd.IsActive && recloser.SecondEnd.First().IsActive)
                {
                    Tuple<bool, float> tuple = await IsElementEnergized(recloser);
                    isEnergized = tuple.Item1;
                    float load = tuple.Item2;

                    if (isEnergized)
                    {
                        await CalculateLoadFlowUpsideDown(recloser.FirstEnd, recloser.Id, recloser.SecondEnd.First().Feeder, loadOfFeeders);
                    }
                    else if(!recloser.IsActive)
                    {
                        Thread thread = new Thread(async () => await CommandToRecloser(measurementGid, (int)DiscreteCommandingType.CLOSE, CommandOriginType.CE_COMMAND, recloser));
                        thread.Start();
                    }
                }
                else if(recloser.IsActive)
                {
                    Logger.LogDebug($"{baseLogString} TurnOnAllMeasurement => Calling SendDiscreteCommand method from measurement provider. Measurement GID {measurementGid:X16}, Value 1.");
                    var measurementProviderClient = MeasurementProviderClient.CreateClient();
                    await measurementProviderClient.SendSingleDiscreteCommand(measurementGid, (int)DiscreteCommandingType.OPEN, CommandOriginType.CE_COMMAND);
                    Logger.LogDebug($"{baseLogString} TurnOnAllMeasurement => SendDiscreteCommand method from measurement provider successfully finished.");
                    recloser.IsActive = false;
                }
            }
            else
            {
                Logger.LogDebug($"{baseLogString} TurnOnAllMeasurement =>  Recloser with GID {recloser.Id:X16} does not have both ends, or have more than one on one end.");
            }
            return topology;
        }
        private async Task CalculateLoadFlowUpsideDown(ITopologyElement element, long sourceElementGid, ITopologyElement feeder, Dictionary<long, float> loadOfFeeders)
        {
            string verboseMessage = $"{baseLogString} CalculateLoadFlowUpsideDown method called. Element with GID {element?.Id:X16}, Source GID {sourceElementGid}.";
            Logger.LogVerbose(verboseMessage);

            if (element == null)
            {
                string message = $"{baseLogString} CalculateLoadFlowUpsideDown => NULL value has been passed instead of element.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            if (feeder == null)
            {
                string message = $"{baseLogString} CalculateLoadFlowUpsideDown => NULL value has been passed instead of feeder.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            Stack<Tuple<ITopologyElement, long>> stack = new Stack<Tuple<ITopologyElement, long>>();
            stack.Push(new Tuple<ITopologyElement, long>(element, sourceElementGid));
            
            Tuple<ITopologyElement, long> tuple;
            ITopologyElement nextElement;
            long sourceGid;

            while (stack.Count > 0)
            {
                tuple = stack.Pop();
                nextElement = tuple.Item1;
                sourceGid = tuple.Item2;

                Tuple<bool, float> resTuple = await IsElementEnergized(nextElement);
                bool isEnergized = resTuple.Item1;
                float load = resTuple.Item2;

                if (isEnergized)
                {
                    if (!nextElement.DmsType.Equals(DMSType.ENERGYCONSUMER.ToString())
                    && nextElement.FirstEnd.Id != sourceGid)
                    {
                        stack.Push(new Tuple<ITopologyElement, long>(nextElement.FirstEnd, nextElement.Id));
                    }

                    foreach (var child in nextElement.SecondEnd)
                    {
                        if (child.Id != sourceGid)
                        {
                            stack.Push(new Tuple<ITopologyElement, long>(child, nextElement.Id));
                        }
                    }

                    if (feeder != null)
                    {
                        if (loadOfFeeders.ContainsKey(feeder.Id))
                        {
                            loadOfFeeders[feeder.Id] += load;
                        }
                        else
                        {
                            loadOfFeeders.Add(feeder.Id, load);
                        }
                    }
                }
                else
                {
                    if (nextElement.DmsType.Equals(DMSType.FUSE.ToString()))
                    {
                        await DeEnergizeElementsUnder(nextElement);
                    }
                    else
                    {
                        foreach (var child in nextElement.SecondEnd)
                        {
                            if (child.Id != sourceGid)
                            {
                                stack.Push(new Tuple<ITopologyElement, long>(child, nextElement.Id));
                            }
                        }
                    }
                }
            }
        }
        private async Task CommandToRecloser(long measurementGid, int value, CommandOriginType originType, ITopologyElement recloser)
        {
            string verboseMessage = $"{baseLogString} CommandToRecloser method called.";
            Logger.LogVerbose(verboseMessage);

            if (recloser == null)
            {
                string message = $"{baseLogString} CommandToRecloser => NULL value has been passed instead of element.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            if (!(recloser is Recloser))
            {
                string message = $"{baseLogString} CommandToRecloser => Element with GID {recloser.Id:X16} is not a recloser.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            Logger.LogDebug($"{baseLogString} CommandToRecloser => Enetring in sleep for 20 seconds.");
            await Task.Delay(recloserInterval);
            Logger.LogDebug($"{baseLogString} CommandToRecloser => Waking up after 20 seconds.");

            var topologyProviderClient = TopologyProviderClient.CreateClient();
            int counter = await topologyProviderClient.GetRecloserCount(recloser.Id);

            if (((Recloser)recloser).MaxNumberOfTries > counter)
            {
                topologyProviderClient = TopologyProviderClient.CreateClient();
                await topologyProviderClient.RecloserOpened(recloser.Id);

                Logger.LogDebug($"{baseLogString} CommandToRecloser => Calling SendDiscreteCommand method from measurement provider. Measurement GID: {measurementGid:X16}, Value: {value}, OriginType {originType}.");
                var measurementProviderClient = MeasurementProviderClient.CreateClient();
                await measurementProviderClient.SendSingleDiscreteCommand(measurementGid, value, originType);
                Logger.LogDebug($"{baseLogString} CommandToRecloser => SendDiscreteCommand method has been successfully called.");
            }
        }
        #endregion

        private async Task<List<AnalogMeasurement>> GetMeasurements(Dictionary<long, string> measurements)
        {
            string verboseMessage = $"{baseLogString} GetMeasurements method called.";
            Logger.LogVerbose(verboseMessage);

            List<AnalogMeasurement> analogMeasurements = new List<AnalogMeasurement>();

            foreach (var measurement in measurements)
            {
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(measurement.Key);

                if (type == DMSType.ANALOG)
                {
                    Logger.LogDebug($"{baseLogString} GetMeasurements => Calling GetAnalogMeasurement for GID {measurement.Key:X16} from measurement provider.");
                    var measurementProviderClient = MeasurementProviderClient.CreateClient();
                    AnalogMeasurement analogMeasurement = await measurementProviderClient.GetAnalogMeasurement(measurement.Key);
                    Logger.LogDebug($"{baseLogString} GetMeasurements => GetAnalogMeasurement method from measurement provider has been called successfully.");

                    if (analogMeasurement == null)
                    {
                        string message = $"{baseLogString} GetMeasurements => GetAnalogMeasurement from measurement provider returned null for measurement GID {measurement.Key:X16}";
                        Logger.LogWarning(message);
                        continue;
                    }

                    if (measurement.Value.Equals(AnalogMeasurementType.POWER.ToString()))
                    {
                        analogMeasurements.Add(analogMeasurement);
                    }
                    else if (measurement.Value.Equals(AnalogMeasurementType.VOLTAGE.ToString()))
                    {
                        analogMeasurements.Add(analogMeasurement);
                    }
                    else if (measurement.Value.Equals(AnalogMeasurementType.CURRENT.ToString()))
                    {
                        analogMeasurements.Add(analogMeasurement);
                    }
                    else if (measurement.Value.Equals(AnalogMeasurementType.FEEDER_CURRENT.ToString()))
                    {
                        analogMeasurements.Add(analogMeasurement);
                    }
                    else
                    {
                        Logger.LogWarning($"{baseLogString} GetMeasurements => Unknown type [{measurement.Value}] of measurement with GID {measurement.Key:X16}.");
                    }
                }
            }

            return analogMeasurements;
        }
        private async Task TurnOffAllMeasurement(Dictionary<long, string> measurements)
        {
            string verboseMessage = $"{baseLogString} TurnOffAllMeasurement method called => Measurements count: {measurements.Count}.";
            Logger.LogVerbose(verboseMessage);

            List<AnalogMeasurement> analogMeasurements = await GetMeasurements(measurements);

            var commands = new Dictionary<long, float>();
            var modbusData = new Dictionary<long, AnalogModbusData>();

            foreach (var meas in analogMeasurements)
            {
                Logger.LogDebug($"{baseLogString} TurnOffAllMeasurement => Adding Command method from measurement provider. Measurement GID {meas.Id:X16}.");
                commands.Add(meas.Id, 0);
                modbusData.Add(meas.Id, new AnalogModbusData(0, AlarmType.NO_ALARM, meas.Id, CommandOriginType.CE_COMMAND));
            }

            var measurementProviderClient = MeasurementProviderClient.CreateClient();

            await measurementProviderClient.SendMultipleAnalogCommand(commands, CommandOriginType.CE_COMMAND);
            Logger.LogDebug($"{baseLogString} TurnOffAllMeasurement => SendAnalogCommand method from measurement provider has been called successfully.");

            Logger.LogDebug($"{baseLogString} TurnOffAllMeasurement => Calling UpdateAnalogMeasurement method from measurement provider.");
            await measurementProviderClient.UpdateAnalogMeasurement(modbusData);
            Logger.LogDebug($"{baseLogString} TurnOffAllMeasurement => UpdateAnalogMeasurement method from measurement provider has been called successfully.");
        }
        private async Task TurnOnAllMeasurement(Dictionary<long, string> measurements)
        {
            string verboseMessage = $"{baseLogString} TurnOnAllMeasurement method called. Measurements count: {measurements.Count}";
            Logger.LogVerbose(verboseMessage);

            List<AnalogMeasurement> analogMeasurements = await GetMeasurements(measurements);

            var commands = new Dictionary<long,float>();
            var modbusData = new Dictionary<long, AnalogModbusData>();

            foreach (var meas in analogMeasurements)
            {
                commands.Add(meas.Id, meas.NormalValue);
                modbusData.Add(meas.Id, new AnalogModbusData(meas.NormalValue, AlarmType.NO_ALARM, meas.Id, CommandOriginType.CE_COMMAND));
            }

            var measurementProviderClient = MeasurementProviderClient.CreateClient();
            await measurementProviderClient.SendMultipleAnalogCommand(commands, CommandOriginType.CE_COMMAND);
                
            Logger.LogDebug($"{baseLogString} TurnOnAllMeasurement => Calling update analog measurement method from measurement provider.");
            await measurementProviderClient.UpdateAnalogMeasurement(modbusData);
            Logger.LogDebug($"{baseLogString} TurnOnAllMeasurement => Update analog measurement method from measurement provider successfully finished.");
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion
    }
}
