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
                       // Provider.Instance.MeasurementProvider.UpdateAnalogMeasurement(signalGid, loadFider.Value, CommandOriginType.CE_COMMAND);
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
            AnalogMeasurement feederCurrentmeasurement = null;
            AnalogMeasurement measurement = null;

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
                        if (!(meas.Value.Equals(AnalogMeasurementType.CURRENT.ToString())
                            && Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(meas.Key, out measurement)))
                        {

                        }
                    }

                    if (measurement != null)
                    {
                        scadaCommanding.SendAnalogCommand(measurement.Id, machineCurrentChange, CommandOriginType.CE_COMMAND);
                        loadOfFeeders[element.Feeder.Id] -= machineCurrentChange;
                    }
                    else
                    {
                        logger.LogError($"[Load flow] Synchronous machine with GID 0x{element.Id:X16} does not have CURRENT measurement.");
                    }
                }

                foreach (var meas in element.Feeder.Measurements)
                {
                    if (meas.Value.Equals(AnalogMeasurementType.FEEDER_CURRENT.ToString()))
                    {
                        if (!Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(meas.Key, out feederCurrentmeasurement))
                        {
                            logger.LogError($"[Load flow] FEEDER_CURRENT with GID 0x{meas.Key:X16} does not exist in measurement provider.");
                        }
                        break;
                    }
                }

                if (feederCurrentmeasurement != null)
                {
                    float feederCurrent = feederCurrentmeasurement.GetCurrentValue();
                    

                   

                }
                else
                {
                    logger.LogError($"[Load flow] Feeder, which synchronous machine with GID 0x{element.Id:X16} belongs to, does not have FEEDER_CURRENT measurement.");
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

                        if (nextElement.Mrid.Equals("ACL_5"))
                        {
                            currentFider = nextElement.Id;
                            if (!feeders.ContainsKey(currentFider))
                            {
                                feeders.Add(currentFider, nextElement);
                            }
                        }
                        else if (nextElement.Mrid.Equals("ACL_6"))
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
            element.IsActive = true;
            float power = 0;
            float voltage = 0;
            float current = 0;
            AnalogMeasurement currentMeasurement = null;
            AnalogMeasurement voltageMeasurement = null;
            AnalogMeasurement powerMeasurement = null;

            foreach (var measurement in element.Measurements)
            {
                if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(measurement.Key) == DMSType.DISCRETE
                    || measurement.Key < 10000)
                {
                    // Value je true ako je prekidac otvoren, tada je element neaktivan
                    element.IsActive = !Provider.Instance.MeasurementProvider.GetDiscreteValue(measurement.Key);
                    break;
                }

                if (measurement.Value.Equals(AnalogMeasurementType.POWER.ToString())
                    && Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(measurement.Key, out powerMeasurement))
                {
                    power = powerMeasurement.GetCurrentValue();
                }
                else if (measurement.Value.Equals(AnalogMeasurementType.VOLTAGE.ToString())
                    && Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(measurement.Key, out voltageMeasurement))
                {
                    voltage = voltageMeasurement.GetCurrentValue();

                }
                else if (measurement.Value.Equals(AnalogMeasurementType.CURRENT.ToString())
                    && Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(measurement.Key, out currentMeasurement))
                {
                    current = currentMeasurement.GetCurrentValue();
                }
            }

            if (element.IsActive)
            {
                if (element.DmsType.Equals(DMSType.SYNCHRONOUSMACHINE.ToString()))
                {
                    //AnalogMeasurement feederCurrentmeasurement = null;

                    //if (element.Feeder != null)
                    //{
                    //    foreach (var meas in element.Feeder.Measurements)
                    //    {
                    //        if (meas.Value.Equals(AnalogMeasurementType.FEEDER_CURRENT.ToString()))
                    //        {
                    //            if (!Provider.Instance.MeasurementProvider.TryGetAnalogMeasurement(meas.Key, out feederCurrentmeasurement))
                    //            {
                    //                logger.LogError($"[Load flow] FEEDER_CURRENT with GID 0x{meas.Key:X16} does not exist in measurement provider.");
                    //            }
                    //            break;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    logger.LogWarn($"[Load flow] Synchronous machine with GID 0x{element.Id:X16} does not belond to any feeder.");
                    //}

                    //if (feederCurrentmeasurement != null)
                    //{
                    //    float machineCurrentChange;
                    //    float feederCurrent = feederCurrentmeasurement.GetCurrentValue();
                    //    if (feederCurrent > 36)
                    //    {
                    //        float improvementFactor = feederCurrent - 36;

                    //        machineCurrentChange = (((SynchronousMachine)element).Capacity >= improvementFactor) 
                    //                                    ? improvementFactor 
                    //                                    : ((SynchronousMachine)element).Capacity;
                    //    }
                    //    else
                    //    {
                    //        machineCurrentChange = 0;
                    //    }

                    //    if (currentMeasurement != null)
                    //    {
                    //        scadaCommanding.SendAnalogCommand(currentMeasurement.Id, machineCurrentChange, CommandOriginType.CE_COMMAND);
                    //        load -= machineCurrentChange;
                    //    }
                    //    else
                    //    {
                    //        logger.LogError($"[Load flow] Synchronous machine with GID 0x{element.Id:X16} does not have CURRENT measurement.");
                    //    }

                    //}
                    //else
                    //{
                    //    logger.LogError($"[Load flow] Feeder, which synchronous machine with GID 0x{element.Id:X16} belongs to, does not have FEEDER_CURRENT measurement.");
                    //}

                    syncMachines.Add(element.Id, element);

                }
                else if (power != 0 && voltage != 0)
                {
                    load = (float)Math.Round(power / voltage);
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
                        member.IsActive = false;
                    }
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
            Thread.Sleep(5000);
            if (!((Recloser)recloser).IsReachedMaximumOfTries())
            {
                scadaCommanding.SendDiscreteCommand(measurementGid, value, originType);
                ((Recloser)recloser).NumberOfTry++;
            }
        }
        #endregion
    }
}
