using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using NetworkModelServiceFunctions;
using Outage.Common;
using SCADACommanding;
using System;
using System.Collections.Generic;
using System.Linq;
using TopologyElementsFuntions;

namespace TopologyBuilder
{
    public class GraphBuilder : ITopologyBuilder
    {
        #region Fields
        private ILogger logger = LoggerWrapper.Instance;
        private readonly TopologyElementFactory topologyElementFactory = new TopologyElementFactory();
        private List<Field> fields;
        private HashSet<long> visited;
        private Stack<ITopologyElement> stack;
        #endregion

        public ITopology CreateGraphTopology(long firstElementGid)
        {
            logger.LogInfo($"Creating graph topology from first element with GID {firstElementGid}.");

            visited = new HashSet<long>();
            stack = new Stack<ITopologyElement>();
            fields = new List<Field>();

            ITopology topology = new TopologyModel();
            ITopologyElement firstNode = topologyElementFactory.CreateTopologyElement(firstElementGid);
            firstNode.IsActive = true;
            stack.Push(firstNode);

            while (stack.Count > 0)
            {
                var currentNode = stack.Pop();
                if (!visited.Contains(currentNode.Id))
                {
                    visited.Add(currentNode.Id);
                }

                var connectedElements = GetReferencedElementsWithoutIgnorables(currentNode.Id);
                foreach (var element in connectedElements)
                {
                    if (TopologyHelper.Instance.GetElementTopologyType(element) == TopologyType.Measurement)
                    {
                        string elementType = TopologyHelper.Instance.GetDMSTypeOfTopologyElement(element);
                        if (elementType.Equals("BASEVOLTAGE"))
                        {
                            currentNode.NominalVoltage = NMSManager.Instance.GetBaseVoltageForElement(element);
                        }
                        else if (elementType.Equals("ANALOG") || elementType.Equals("DISCRETE"))
                        {
                            currentNode.Measurements.Add(topologyElementFactory.CreateMeasurement(element));
                            //SCADACommandingCache.Instance.AddMeasurementToElement(currentNode.Id, currentNode.Measurement.Id);
                        }
                        else
                        {
                            logger.LogError($"Failed to procces element of type Measurement. Element is neither Analog,Discrete nor BaseVoltage.");
                        }
                    }
                    else
                    {
                        var newNode = ConnectTwoNodes(element, currentNode);       
                        stack.Push(newNode);
                    }
                }
                currentNode.DmsType = TopologyHelper.Instance.GetDMSTypeOfTopologyElement(currentNode.Id);
                topology.AddElement(currentNode);
            }
            topology.FirstNode = firstNode.Id;

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
            var list = NMSManager.Instance.GetAllReferencedElements(gid).Where(e => !visited.Contains(e)).ToList();
            List<long> elements = new List<long>();
            foreach (var element in list)
            {
                if (TopologyHelper.Instance.GetElementTopologyStatus(element) == TopologyStatus.Ignorable)
                {
                    visited.Add(element);
                    elements.AddRange(GetReferencedElementsWithoutIgnorables(element));
                }
                else
                {
                    elements.Add(element);
                }
            }
            return elements;
        }
        private ITopologyElement ConnectTwoNodes(long newElementGid, ITopologyElement parent)
        {
            bool newElementIsField = TopologyHelper.Instance.GetElementTopologyStatus(newElementGid) == TopologyStatus.Field;
            bool parentElementIsField = TopologyHelper.Instance.GetElementTopologyStatus(parent.Id) == TopologyStatus.Field;

            ITopologyElement newNode = topologyElementFactory.CreateTopologyElement(newElementGid);

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
            return newNode;
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
