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

namespace Topology
{
    public class LoadFlow : ILoadFlow
    {
        readonly ILogger logger = LoggerWrapper.Instance;
        private readonly ISCADACommanding scadaCommanding = new SCADACommanding();
        private HashSet<long> reclosers;
        private Dictionary<long, ITopologyElement> fiders;
        private Dictionary<long, float> loadOfFiders;


        public void UpdateLoadFlow(List<ITopology> topologies)
        {
            loadOfFiders = new Dictionary<long, float>();
            fiders = new Dictionary<long, ITopologyElement>();
            reclosers = Provider.Instance.ModelProvider.GetReclosers();
            foreach (var topology in topologies)
            {
                CalculateLoadFlow(topology);
            }
            UpdateLoadFlowFromRecloser(topologies);

            foreach (var loadFider in loadOfFiders)
            {
                if (fiders.TryGetValue(loadFider.Key, out ITopologyElement fider))
                {
                    long signalGid = 0;
                    foreach (var measurement in fider.Measurements)
                    {
                        if (measurement.Value.Equals(AnalogMeasurementType.CURRENT.ToString()))
                        {
                            signalGid = measurement.Key;
                        }
                    }

                    if (signalGid != 0)
                    {
                        scadaCommanding.SendAnalogCommand(signalGid, loadFider.Value, CommandOriginType.CE_COMMAND);
                        Provider.Instance.MeasurementProvider.UpdateAnalogMeasurement(signalGid, loadFider.Value, CommandOriginType.CE_COMMAND);
                    }
                }
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
                            if (!fiders.ContainsKey(currentFider))
                            {
                                fiders.Add(currentFider, nextElement);
                            }
                        }
                        else if (nextElement.Mrid.Equals("ACL_6"))
                        {
                            currentFider = nextElement.Id;
                            if (!fiders.ContainsKey(currentFider))
                            {
                                fiders.Add(currentFider, nextElement);
                            }
                        }

                        if (!IsElementEnergized(nextElement, out float load))
                        {
                            DeEnergizeElementsUnder(nextElement);
                        }
                        else
                        {

                            if (loadOfFiders.ContainsKey(currentFider))
                            {
                                loadOfFiders[currentFider] += load;
                            }
                            else
                            {
                                loadOfFiders.Add(currentFider, load);
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
            element.IsActive = true;
            foreach (var measurement in element.Measurements.Keys)
            {
                if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(measurement) == DMSType.DISCRETE
                    || measurement < 10000)
                {
                    // Value je true ako je prekidac otvoren, tada je element neaktivan
                    element.IsActive = !Provider.Instance.MeasurementProvider.GetDiscreteValue(measurement);
                    break;
                }
            }

            if (element.DmsType.Equals(DMSType.ENERGYCONSUMER.ToString()))
            {
                load = 2;
            }
            else
            {
                load = 0;
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
                        CalculateLoadFlowUpsideDown(recloser.SecondEnd.First(), recloser.Id, recloser.FirstEnd.Fider);
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
                        CalculateLoadFlowUpsideDown(recloser.FirstEnd, recloser.Id, recloser.SecondEnd.First().Fider);
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
        private void CalculateLoadFlowUpsideDown(ITopologyElement element, long sourceElementGid, long fider)
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

                    if (loadOfFiders.ContainsKey(fider))
                    {
                        loadOfFiders[fider] += load;
                    }
                    else
                    {
                        loadOfFiders.Add(fider, load);
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
