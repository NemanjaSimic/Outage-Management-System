using CECommon;
using CECommon.Interfaces;
using CECommon.Providers;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using CalculationEngine.SCADAFunctions;
using System.Threading;
using CECommon.Models;
using CECommon.Model;
using Outage.Common.PubSub.SCADADataContract;

namespace Topology
{
    public class LoadFlow : ILoadFlow
    {
        readonly ILogger logger = LoggerWrapper.Instance;
        private readonly ISCADACommanding scadaCommanding = new SCADACommanding();
        private HashSet<long> reclosers;
        private Dictionary<long, ITopologyElement> feeders;
        private Dictionary<long, float> loadOfFeeders;
        private Dictionary<long, ITopologyElement> syncMachines;

        public void UpdateLoadFlow(List<ITopology> topologies)
        {
            loadOfFeeders = new Dictionary<long, float>();
            feeders = new Dictionary<long, ITopologyElement>();
            syncMachines = new Dictionary<long, ITopologyElement>();
            reclosers = Provider.Instance.ModelProvider.GetReclosers();
            foreach (var topology in topologies)
            {
                CalculateLoadFlow(topology);
            }
            UpdateLoadFlowFromRecloser(topologies);

            foreach (var syncMachine in syncMachines.Values)
            {
                SyncMachine(syncMachine);
            }

            foreach (var loadFider in loadOfFeeders)
            {
                if (feeders.TryGetValue(loadFider.Key, out ITopologyElement fider))
                {
                    long signalGid = 0;
                    foreach (var measurement in fider.Measurements)
                    {
                        if (measurement.Value.Equals(AnalogMeasurementType.FEEDER_CURRENT.ToString()))
                        {
                            signalGid = measurement.Key;
                        }
                    }

                    if (signalGid != 0)
                    {
                        scadaCommanding.SendAnalogCommand(signalGid, loadFider.Value, CommandOriginType.CE_COMMAND);
                        AlarmType alarmType = (loadFider.Value >= 36) ? AlarmType.HIGH_ALARM : AlarmType.NO_ALARM;

                        Dictionary<long, AnalogModbusData> data = new Dictionary<long, AnalogModbusData>(1)
                        {
                            { signalGid, new AnalogModbusData(loadFider.Value, alarmType, signalGid, CommandOriginType.CE_COMMAND)}
                        };

                        Provider.Instance.MeasurementProvider.UpdateAnalogMeasurement(data);
                    }
                    else
                    {
                        logger.LogWarn($"[Load flow] Feeder with GID 0x{fider.Id:X16} does not have FEEDER_CURRENT measurement.");
                    }
                }
            }
        }
        private void SyncMachine(ITopologyElement element)
        {
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
                            if (!Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(meas.Key, out powerMeasurement))
                            {
                                logger.LogError($"[Load flow] Synchronous machine with GID 0x{element.Id:X16} does not have POWER measurement.");
                            }
                        }
                        
                        if (meas.Value.Equals(AnalogMeasurementType.VOLTAGE.ToString()))
                        {
                            if (!Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(meas.Key, out voltageMeasurement))
                            {
                                logger.LogError($"[Load flow] Synchronous machine with GID 0x{element.Id:X16} does not have VOLTAGE measurement.");
                            }
                        }
                    }

                    if (powerMeasurement != null && voltageMeasurement != null)
                    {
                        float newNeededPower = machineCurrentChange * voltageMeasurement.GetCurrentValue();
                        float newSMPower = (((SynchronousMachine)element).Capacity >= newNeededPower)
                                                    ? newNeededPower
                                                    : ((SynchronousMachine)element).Capacity;

                        scadaCommanding.SendAnalogCommand(powerMeasurement.Id, newSMPower, CommandOriginType.CE_COMMAND);

                        Dictionary<long, AnalogModbusData> data = new Dictionary<long, AnalogModbusData>(1)
                        {
                            { powerMeasurement.Id, new AnalogModbusData(newSMPower, AlarmType.NO_ALARM, powerMeasurement.Id, CommandOriginType.CE_COMMAND)}
                        };

                        Provider.Instance.MeasurementProvider.UpdateAnalogMeasurement(data);

                        loadOfFeeders[element.Feeder.Id] -= newSMPower / voltageMeasurement.GetCurrentValue();
                    }
                    else
                    {
                        logger.LogError($"[Load flow] Synchronous machine with GID 0x{element.Id:X16} does not have CURRENT measurement.");
                    }
                }

            }
            else
            {
                logger.LogWarn($"[Load flow] Synchronous machine with GID 0x{element.Id:X16} does not belond to any feeder.");
            }

            
        }
        private void CalculateLoadFlow(ITopology topology)
        {
            logger.LogDebug("Calulate load flow started.");

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
                      

                        if (!IsElementEnergized(nextElement, out float load))
                        {
                            DeEnergizeElementsUnder(nextElement);
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
                logger.LogFatal("[Calulate load flow] First element of topology does not exist in collection.");
            }
            logger.LogDebug("Calulate load flow successfully finished.");
        }
        private bool IsElementEnergized(ITopologyElement element, out float load)
        {
            load = 0;
            bool pastState = element.IsActive;
            element.IsActive = true;

            foreach (var measurement in element.Measurements)
            {
                if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(measurement.Key) == DMSType.DISCRETE
                    || measurement.Key < 10000)
                {
                    // Value je true ako je prekidac otvoren, tada je element neaktivan
                    element.IsActive = !Provider.Instance.MeasurementProvider.GetDiscreteValue(measurement.Key);
                    break;
                }
            }

            if (element.IsActive)
            {
                if (!pastState)
                {
                    TurnOnAllMeasurement(element.Measurements);
                }
                var analogMeasurements = GetMeasurements(element.Measurements);

                if (element.DmsType.Equals(DMSType.SYNCHRONOUSMACHINE.ToString()))
                {
                    syncMachines.Add(element.Id, element);
                }
                else
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
                
            }

            return element.IsActive;
        }
        private void DeEnergizeElementsUnder(ITopologyElement element)
        {
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
                        TurnOffAllMeasurement(member.Measurements);
                        member.IsActive = false;
                    }
                }

                TurnOffAllMeasurement(nextElement.Measurements);

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
        private void UpdateLoadFlowFromRecloser(List<ITopology> topologies)
        {
            List<ITopology> retTopologies = topologies;
            foreach (var recloserGid in reclosers)
            {
                foreach (var topology in topologies)
                {
                    if (topology.GetElementByGid(recloserGid, out ITopologyElement recloser))
                    {
                        CalculateLoadFlowFromRecloser(recloser, topology);
                        break;
                    }
                }
            }
        }
        private ITopology CalculateLoadFlowFromRecloser(ITopologyElement recloser, ITopology topology)
        {
            long measurementGid = 0;

            if (recloser.Measurements.Count == 1)
            {
                //dogovor je da prekidaci imaju samo discrete measurement
                measurementGid = recloser.Measurements.First().Key;
            }
            else
            {
                logger.LogWarn($"[CalculateLoadFlowFromRecloser] Recloser with GID 0x{recloser.Id.ToString("X16")} does not have proper measurements.");
            }

            if (recloser.FirstEnd != null && recloser.SecondEnd.Count == 1)
            {
                if (recloser.FirstEnd.IsActive && !recloser.SecondEnd.First().IsActive)
                {
                    if (IsElementEnergized(recloser, out float load))
                    {
                        CalculateLoadFlowUpsideDown(recloser.SecondEnd.First(), recloser.Id, recloser.FirstEnd.Feeder);
                    }
                    else
                    {
                        Thread thread = new Thread(() => CommandToRecloser(measurementGid, 0, CommandOriginType.CE_COMMAND, recloser));
                        thread.Start();
                    }
                }
                else if (!recloser.FirstEnd.IsActive && recloser.SecondEnd.First().IsActive)
                {
                    if (IsElementEnergized(recloser, out float load))
                    {
                        CalculateLoadFlowUpsideDown(recloser.FirstEnd, recloser.Id, recloser.SecondEnd.First().Feeder);
                    }
                    else
                    {
                        Thread thread = new Thread(() => CommandToRecloser(measurementGid, 0, CommandOriginType.CE_COMMAND, recloser));
                        thread.Start();
                    }
                }
                else
                {
                    //TODO: pitati asistente, da li da se prenese na Validate
                    scadaCommanding.SendDiscreteCommand(measurementGid, 1, CommandOriginType.CE_COMMAND);
                    recloser.IsActive = false;
                }
            }
            else
            {
                logger.LogDebug($"[CalculateLoadFlowFromRecloser] Recloser with GID 0x{recloser.Id.ToString("X16")} does not have both ends, or have more than one on one end.");
            }
            return topology;
        }
        private void CalculateLoadFlowUpsideDown(ITopologyElement element, long sourceElementGid, ITopologyElement feeder)
        {
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

                if (IsElementEnergized(nextElement, out float load))
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

                    if (loadOfFeeders.ContainsKey(feeder.Id))
                    {
                        loadOfFeeders[feeder.Id] += load;
                    }
                    else
                    {
                        loadOfFeeders.Add(feeder.Id, load);
                    }
                }
                else
                {
                    if (nextElement.DmsType.Equals(DMSType.FUSE.ToString()))
                    {
                        DeEnergizeElementsUnder(nextElement);
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
        private void CommandToRecloser(long measurementGid, int value, CommandOriginType originType, ITopologyElement recloser)
        {
            Thread.Sleep(10000);
            if (!((Recloser)recloser).IsReachedMaximumOfTries())
            {
                scadaCommanding.SendDiscreteCommand(measurementGid, value, originType);
                ((Recloser)recloser).NumberOfTry++;
            }
        }
        #endregion

        public List<AnalogMeasurement> GetMeasurements(Dictionary<long, string> measurements)
        {
            List<AnalogMeasurement> analogMeasurements = new List<AnalogMeasurement>();

            foreach (var measurement in measurements)
            {
                if (measurement.Value.Equals(AnalogMeasurementType.POWER.ToString())
                    && Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(measurement.Key, out AnalogMeasurement power))
                {
                    analogMeasurements.Add(power);
                }
                else if (measurement.Value.Equals(AnalogMeasurementType.VOLTAGE.ToString())
                    && Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(measurement.Key, out AnalogMeasurement voltage))
                {
                    analogMeasurements.Add(voltage);
                }
                else if (measurement.Value.Equals(AnalogMeasurementType.CURRENT.ToString())
                    && Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(measurement.Key, out AnalogMeasurement current))
                {
                    analogMeasurements.Add(current);
                }
                else if (measurement.Value.Equals(AnalogMeasurementType.FEEDER_CURRENT.ToString())
                  && Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(measurement.Key, out AnalogMeasurement feederCurrent))
                {
                    analogMeasurements.Add(feederCurrent);
                }
            }

            return analogMeasurements;
        }

        private void TurnOffAllMeasurement(Dictionary<long, string> measurements)
        {
            List<AnalogMeasurement> analogMeasurements = GetMeasurements(measurements);

            foreach (var meas in analogMeasurements)
            {
                scadaCommanding.SendAnalogCommand(meas.Id, 0, CommandOriginType.CE_COMMAND);

                Dictionary<long, AnalogModbusData> data = new Dictionary<long, AnalogModbusData>(1)
                {
                            { meas.Id, new AnalogModbusData(0, AlarmType.NO_ALARM, meas.Id, CommandOriginType.CE_COMMAND)}
                };

                Provider.Instance.MeasurementProvider.UpdateAnalogMeasurement(data);
            }
        }

        private void TurnOnAllMeasurement(Dictionary<long, string> measurements)
        {
            List<AnalogMeasurement> analogMeasurements = GetMeasurements(measurements);

            foreach (var meas in analogMeasurements)
            {
                scadaCommanding.SendAnalogCommand(meas.Id, 0, CommandOriginType.CE_COMMAND);

                Dictionary<long, AnalogModbusData> data = new Dictionary<long, AnalogModbusData>(1)
                {
                            { meas.Id, new AnalogModbusData(meas.NormalValue, AlarmType.NO_ALARM, meas.Id, CommandOriginType.CE_COMMAND)}
                };

                Provider.Instance.MeasurementProvider.UpdateAnalogMeasurement(data);
            }
        }
    }
}
