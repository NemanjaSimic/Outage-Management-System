using CECommon;
using CECommon.Interfaces;
using CECommon.Providers;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using CalculationEngine.SCADAFunctions;

namespace Topology
{
    public class LoadFlow : ILoadFlow
    {
        readonly ILogger logger = LoggerWrapper.Instance;
        private readonly ISCADACommanding scadaCommanding = new SCADACommanding();
        private HashSet<long> reclosers;

        public void UpdateLoadFlow(List<ITopology> topologies)
        {
            reclosers = Provider.Instance.ModelProvider.GetReclosers();
            foreach (var topology in topologies)
            {
                CalculateLoadFlow(topology);
            }
            UpdateLoadFlowFromRecloser(topologies);
        }
        private void CalculateLoadFlow(ITopology topology)
        {
            logger.LogDebug("Calulate load flow started.");

            if (topology.GetElementByGid(topology.FirstNode, out ITopologyElement firsElement))
            {
                Stack<ITopologyElement> stack = new Stack<ITopologyElement>();
                stack.Push(firsElement);
                ITopologyElement nextElement;

                while (stack.Count > 0)
                {
                    nextElement = stack.Pop();

                    if (!IsElementEnergized(nextElement))
                    {
                        DeEnergizeElementsAbove(nextElement);
                    }
                    else
                    {
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
            else
            {
                logger.LogFatal("[Calulate load flow] First element of topology does not exist in collection.");
            }
            logger.LogDebug("Calulate load flow successfully finished.");
        }
        private bool IsElementEnergized(ITopologyElement element)
        {
            element.IsActive = true;
            if (element is Field field)
            {
                field.IsActive = true;
                foreach (var member in field.Members)
                {
                    if (!IsElementEnergized(member))
                    {
                        field.IsActive = false;
                    }
                }
            }
            else
            {
                foreach (var measurement in element.Measurements)
                {
                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(measurement) == DMSType.DISCRETE)
                    {
                        // Value je true ako je prekidac otvoren, tada je element neaktivan
                        element.IsActive = !Provider.Instance.MeasurementProvider.GetDiscreteValue(measurement);
                        break;
                    }
                }
            }
            return element.IsActive;
        }
        private void DeEnergizeElementsAbove(ITopologyElement element)
        {
            Stack<ITopologyElement> stack = new Stack<ITopologyElement>();
            stack.Push(element);
            ITopologyElement nextElement;

            while (stack.Count > 0)
            {
                nextElement = stack.Pop();
                nextElement.IsActive = false;
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
            foreach (var recloser in reclosers)
            {
                foreach (var topology in topologies)
                {
                    if (topology.TopologyElements.ContainsKey(recloser))
                    {
                        CalculateLoadFlowFromRecloser(recloser, topology);
                        break;
                    }
                }
            }
        }
        private ITopology CalculateLoadFlowFromRecloser(long recloserGid, ITopology topology)
        {
            if (topology.GetElementByGid(recloserGid, out ITopologyElement element))
            {
                long measurementGid = 0;

                if (element.Measurements.Count == 1)
                {
                    measurementGid = element.Measurements.First();
                }
                else
                {
                    logger.LogWarn($"[CalculateLoadFlowFromRecloser] Recloser with GID 0x{recloserGid.ToString("X16")} does not have proper measurements.");
                }

                if (element.FirstEnd != null && element.SecondEnd.Count == 1)
                {
                    if (element.FirstEnd.IsActive && !element.SecondEnd.First().IsActive)
                    {
                        CalculateLoadFlowUpsideDown(element.SecondEnd.First(), recloserGid);
                        element.IsActive = true;
                        scadaCommanding.SendDiscreteCommand(measurementGid, 0, CommandOriginType.CE_COMMAND);
                    }
                    else if (!element.FirstEnd.IsActive && element.SecondEnd.First().IsActive)
                    {
                        CalculateLoadFlowUpsideDown(element.FirstEnd, recloserGid);
                        element.IsActive = true;
                        scadaCommanding.SendDiscreteCommand(measurementGid, 0, CommandOriginType.CE_COMMAND);
                    }
                    else
                    {
                        //TODO: pitati asistente, da li da se prenese na Validate
                        scadaCommanding.SendDiscreteCommand(measurementGid, 1, CommandOriginType.CE_COMMAND);
                        element.IsActive = false;
                    }
                }
                else
                {
                    logger.LogDebug($"[CalculateLoadFlowFromRecloser] Recloser with GID 0x{recloserGid.ToString("X16")} does not have both ends, or have more than one on one end.");
                }
            }
            else
            {
                logger.LogDebug($"[CalculateLoadFlowFromRecloser] Recloser with GID 0x{recloserGid.ToString("X16")} does not exist in topology.");
            }
            return topology;
        }
        private void CalculateLoadFlowUpsideDown(ITopologyElement element, long sourceElementGid)
        {
            element.IsActive = true;

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

                foreach (var child in nextElement.SecondEnd)
                {
                    if (child.Id != sourceGid)
                    {
                        stack.Push(new Tuple<ITopologyElement, long>(child, nextElement.Id));
                    }
                }

                if (IsElementEnergized(nextElement)
                    && !nextElement.DmsType.Equals(DMSType.ENERGYCONSUMER.ToString())
                    && nextElement.FirstEnd.Id != sourceGid)
                {
                    stack.Push(new Tuple<ITopologyElement, long>(nextElement.FirstEnd, nextElement.Id));
                }
            }
        }
        #endregion
    }
}
