using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using CECommon.Providers;
using NetworkModelServiceFunctions;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TopologyBuilder
{
    public class GraphBuilder : ITopologyBuilder
    {
        #region Fields
        private ILogger logger = LoggerWrapper.Instance;
        private List<Field> fields;
        private HashSet<long> visited;
        private Stack<long> stack;
        private Stopwatch stopwatch = new Stopwatch();
        private Dictionary<long, ITopologyElement> elements;
        private Dictionary<long, IMeasurement> measurements;
        private Dictionary<long, List<long>> connections;
        #endregion

        public ITopology CreateGraphTopology(long firstElementGid)
        { 
            elements = Provider.Instance.ModelProvider.GetElementModels();
            measurements = Provider.Instance.ModelProvider.GetMeasurementModels();
            connections = Provider.Instance.ModelProvider.GetConnections();
            
            logger.LogInfo($"Creating graph topology from first element with GID {firstElementGid}.");

            visited = new HashSet<long>();
            stack = new Stack<long>();
            fields = new List<Field>();

            ITopology topology = new TopologyModel();
            topology.FirstNode = firstElementGid;
 
            stack.Push(firstElementGid);

            while (stack.Count > 0)
            {
                var currentElementId = stack.Pop();
                if (!visited.Contains(currentElementId))
                {
                    visited.Add(currentElementId);
                }
                ITopologyElement currentElement = elements[currentElementId];

                foreach (var element in GetReferencedElementsWithoutIgnorables(currentElementId))
                {
                    if (elements.ContainsKey(element))
                    {
                        ConnectTwoNodes(element, currentElement);
                        stack.Push(element);
                    }
                    else if (measurements.ContainsKey(element))
                    {
                        IMeasurement newMeasurement = measurements[element];
                        newMeasurement.ElementId = currentElement.Id;
                        currentElement.Measurements.Add(newMeasurement);
                        //if (Enum.TryParse<AnalogMeasurementType>(newMeasurement.GetMeasurementType(), out AnalogMeasurementType type))
                        //{
                            //SCADACommandingCache.Instance.AddAnalogMeasurement(newMeasurement.Id, currentElementId, newMeasurement.GetCurrentVaule());
                       // }
                        //else
                        //{
                            //SCADACommandingCache.Instance.AddDiscreteMeasurement(newMeasurement.Id, currentElementId, (ushort)newMeasurement.GetCurrentVaule());
                       // }

                    }
                    

                }  
               
                topology.AddElement(currentElement);
            }

            foreach (var field in fields)
            {
                topology.AddElement(field);
            }

            logger.LogInfo("Topology graph created.");
            return topology;
        }

        #region HelperFunctions
        private List<long> GetReferencedElementsWithoutIgnorables(long gid)
        {
            var list = connections[gid].Where(e => !visited.Contains(e)).ToList();
            List<long> refElements = new List<long>();
            foreach (var element in list)
            {
                if (TopologyHelper.Instance.GetElementTopologyStatus(element) == TopologyStatus.Ignorable)
                {
                    visited.Add(element);
                    refElements.AddRange(GetReferencedElementsWithoutIgnorables(element));
                }
                else
                {
                    refElements.Add(element);
                }
            }
            return refElements;
        }
        private void ConnectTwoNodes(long newElementGid, ITopologyElement parent)
        {
            bool newElementIsField = TopologyHelper.Instance.GetElementTopologyStatus(newElementGid) == TopologyStatus.Field;
            bool parentElementIsField = TopologyHelper.Instance.GetElementTopologyStatus(parent.Id) == TopologyStatus.Field;
            
            ITopologyElement newNode = elements[newElementGid];

            if (newElementIsField && !parentElementIsField)
            {
                var field = new Field(newNode.Id);
                field.FirstEnd = parent.Id;
                newNode.FirstEnd = parent.Id;
                fields.Add(field);
                parent.SecondEnd.Add(field.Id);
            }
            else if (newElementIsField && parentElementIsField)
            {
                try
                {
                    GetField(parent.Id).Members.Add(newNode.Id);
                    newNode.FirstEnd = parent.Id;
                    parent.SecondEnd.Add(newNode.Id);
                }
                catch (Exception)
                {
                    string message = $"Element with GID {parent.Id.ToString("X")} has no field.";
                    logger.LogDebug(message);
                    throw new Exception(message);
                }

            }
            else if (!newElementIsField && parentElementIsField)
            {
                var field = GetField(parent.Id);
                if (field == null)
                {
                    string message = $"Element with GID {parent.Id.ToString("X")} has no field.";
                    logger.LogDebug(message);
                    throw new Exception(message);
                }
                else
                {
                    field.SecondEnd.Add(newNode.Id);
                    parent.SecondEnd.Add(newNode.Id);
                    newNode.FirstEnd = field.Id;
                }
            }
            else
            {
                newNode.FirstEnd = parent.Id;
                parent.SecondEnd.Add(newNode.Id);
            }

            newNode.IsActive = parent.IsActive;
            foreach (var measurement in newNode.Measurements)
            {
                if (measurement is DiscreteMeasurement && parent.IsActive)
                {
                    if (measurement.GetCurrentVaule() == 1)
                    {
                        newNode.IsActive = false;
                    }
                    else
                    {
                        newNode.IsActive = true;
                    }
                }
            }

        }
        private Field GetField(long memberGid)
        {
            Field field = null;
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Members.Where(e => e == memberGid).ToList().Count > 0)
                {
                    return fields[i];
                }
            }
            return field;
        }
        #endregion
    }
}
