using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using CECommon.Providers;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Topology
{
    public class VoltageFlow : IVoltageFlow
    {
        ILogger logger = LoggerWrapper.Instance;
        private HashSet<long> reclosers;
        private List<ITopology> tempTopology;


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
                bool hasDiscrete = false;
                foreach (var measurement in element.Measurements)
                {
                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(measurement) == DMSType.DISCRETE)
                    {
                        // Value je true ako je prekidac otvoren, tada je element neaktivan
                        element.IsActive = !Provider.Instance.MeasurementProvider.GetDiscreteValue(measurement);
                        hasDiscrete = true;
                        break;
                    }
                }

                //if (!hasDiscrete)
                //{
                //    if (element.FirstEnd != null)
                //    {
                //        element.IsActive = element.FirstEnd.IsActive;
                //    }
                //    else
                //    {
                //        logger.LogDebug($"[CaluclateVoltageFlow] Element with GID 0x{element.Id.ToString("X16")} has no parent.");
                //    }
                //}
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

        public void UpdateLoadFlowFromRecloser(List<ITopology> topologies)
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


        public ITopology CalculateLoadFlowFromRecloser(long recloserGid, ITopology topology)
        {
            if (topology.GetElementByGid(recloserGid, out ITopologyElement element))
            {
                if (element.FirstEnd != null && element.SecondEnd.Count == 1)
                {
                    if (element.FirstEnd.IsActive && !element.SecondEnd.First().IsActive)
                    {
                        CalculateLoadFlowUpsideDown(element.SecondEnd.First(), recloserGid);
                    }
                    else if (!element.FirstEnd.IsActive && element.SecondEnd.First().IsActive)
                    {
                        CalculateLoadFlowUpsideDown(element.FirstEnd, recloserGid);
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

















        public List<ITopology> UpdateVoltageFlow(List<long> signalGids, List<ITopology> topologies)
        {
            List<ITopology> retVal = new List<ITopology>(topologies);
            foreach (long signalGid in signalGids)
            {
                foreach (var topology in topologies)
                {
                    if (topology.TopologyElements.ContainsKey(signalGid))
                    {
                        retVal.Remove(topology);
                        retVal.Add(CalulateVoltageFlow(signalGid, topology));
                        break;
                    }
                }
            }
            return retVal;
        }
        public ITopology CalulateVoltageFlow(long startingElementGid, ITopology topology)
        {
            logger.LogDebug("CalulateVoltageFlow started.");
            
            if (topology.TopologyElements.ContainsKey(startingElementGid))
            {
                var reclosers = Provider.Instance.ModelProvider.GetReclosers();
                Stack<long> stack = new Stack<long>();
                stack.Push(startingElementGid);
                ITopologyElement element;
                long nextElementId;

                while (stack.Count > 0)
                {
                    nextElementId = stack.Pop();
                    if (topology.GetElementByGid(nextElementId, out element))
                    {
                        if (!IsElementActive(element, topology))
                        {
                            TurnOffAllElementsAbove(element.Id, topology);
                        }
                        else
                        {
                            foreach (var child in element.SecondEnd)
                            {
                                if (!reclosers.Contains(child.Id))
                                {
                                    stack.Push(child.Id);
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.LogError($"[CalulateVoltageFlow] Element with GID 0x{nextElementId.ToString("X16")} does not exist in topology.");
                    }
                }
            }

            logger.LogDebug("CalulateVoltageFlow successfully finished.");
            return topology;
        }
        private void TurnOffAllElementsAbove(long topologyElement, ITopology topology)
        {
            var reclosers = Provider.Instance.ModelProvider.GetReclosers();
            Stack<long> stack = new Stack<long>();
            stack.Push(topologyElement);
            while (stack.Count > 0)
            {
                var element = stack.Pop();
                if (topology.TopologyElements.ContainsKey(element))
                {
                    topology.TopologyElements[element].IsActive = false;
                    foreach (var child in topology.TopologyElements[element].SecondEnd)
                    {
                        if (!reclosers.Contains(child.Id))
                        {
                            stack.Push(child.Id);
                        }
                    }
                }
                else
                {
                    logger.LogError($"[TurnOffAllElements] Element with GID 0x{element.ToString("X16")} does not exist in topology.");
                }
            }
        }
        private bool IsElementActive(ITopologyElement element, ITopology topology)
        {
            if (element is Field field)
            {
                bool isFieldActive = true;
                foreach (var member in field.Members)
                {
                    if (topology.GetElementByGid(member.Id, out ITopologyElement memberElement))
                    {
                        if (!IsElementActive(memberElement, topology))
                        {
                            isFieldActive = false;
                        }
                    }
                    else
                    {
                        logger.LogError($"[CaluclateVoltageFlow] Element with GID 0x{member.Id.ToString("X16")} does not exist in topology.");
                    }
                }
                field.IsActive = isFieldActive;
            }
            else
            {
                bool hasDiscrete = false;
                foreach (var measurement in element.Measurements)
                {
                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(measurement) == DMSType.DISCRETE)
                    {
                        // Value je true ako je prekidac otvoren, tada je element neaktivan
                        element.IsActive = !Provider.Instance.MeasurementProvider.GetDiscreteValue(measurement);
                        hasDiscrete = true;
                        break;
                    }
                }

                if (!hasDiscrete)
                {
                    if (element.FirstEnd != null)
                    {
                        element.IsActive = element.FirstEnd.IsActive;
                    }
                    else
                    {
                        logger.LogDebug($"[CaluclateVoltageFlow] Element with GID 0x{element.Id.ToString("X16")} has no parent.");
                    }
                }
            }

            return element.IsActive;
        }


        public List<ITopology> UpdateVoltageFlowFromRecloser(List<long> recloserGids, List<ITopology> topologies)
        {
            List<ITopology> retTopologies = topologies;
            foreach (var recloser in recloserGids)
            {
                foreach (var topology in topologies)
                {
                    if (topology.TopologyElements.ContainsKey(recloser))
                    {
                        retTopologies.Remove(topology);
                        retTopologies.Add(CalculateVoltageFlowFromRecloser(recloser, topology));
                        break;
                    }
                }
            }

            return retTopologies;
        }

        public ITopology CalculateVoltageFlowFromRecloser(long startingGid, ITopology topology)
        {
            if (topology.GetElementByGid(startingGid, out ITopologyElement element) && IsElementActive(element, topology))
            {
                if (element.FirstEnd != null && element.SecondEnd.Count == 1)
                {
                    if (element.FirstEnd.IsActive && !element.SecondEnd.First().IsActive)
                    {
                        CalculateVoltageFlowUpsideDown(element.SecondEnd.First(), startingGid, topology);
                    }
                    else if (!element.FirstEnd.IsActive && element.SecondEnd.First().IsActive)
                    {
                        CalculateVoltageFlowUpsideDown(element.FirstEnd, startingGid, topology);
                    }
                }
                else
                {
                    logger.LogDebug($"[CalculateVoltageFlowUpsideDown] Recloser with GID 0x{startingGid.ToString("X16")} does not have both ends, or have more than one on one end.");
                }
            }
            else
            {
                logger.LogDebug($"[CalculateVoltageFlowUpsideDown] Element with GID 0x{startingGid.ToString("X16")} does not exist in topology.");
            }
            return topology;
        }

        private void CalculateVoltageFlowUpsideDown(ITopologyElement element, long sourceElementGid, ITopology topology)
        {
            Stack<Tuple<ITopologyElement, long>> stack = new Stack<Tuple<ITopologyElement, long>>();
            stack.Push(new Tuple<ITopologyElement, long>(element, sourceElementGid));
            element.IsActive = true;
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

                if (IsActiveFromUpsideDown(nextElement, topology) 
                    && !nextElement.DmsType.Equals(DMSType.ENERGYCONSUMER.ToString())
                    && nextElement.FirstEnd.Id != sourceGid)
                {
                    stack.Push(new Tuple<ITopologyElement, long>(nextElement.FirstEnd, nextElement.Id));           
                }
            }
        }

        private bool IsActiveFromUpsideDown(ITopologyElement element, ITopology topology)
        {
            element.IsActive = true;
            if (element is Field field)
            {
                foreach (var member in field.Members)
                {
                    if(topology.GetElementByGid(member.Id, out ITopologyElement memberElement) 
                        && !IsActiveFromUpsideDown(memberElement, topology))
                    {
                        element.IsActive = false;
                        break;
                    }
                   
                }
                field.IsActive = element.IsActive;
            }
            else
            {
                foreach (var measurement in element.Measurements)
                {
                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(measurement) == DMSType.DISCRETE)
                    {
                        element.IsActive = !Provider.Instance.MeasurementProvider.GetDiscreteValue(measurement);
                        break;
                    }
                }
            }

            return element.IsActive;
        }
    }
}
