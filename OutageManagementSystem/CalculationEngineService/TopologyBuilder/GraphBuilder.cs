using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using CECommon.Providers;
using NetworkModelServiceFunctions;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TopologyBuilder
{
    public class GraphBuilder : ITopologyBuilder
    {
        #region Fields
        private ILogger logger = LoggerWrapper.Instance;
        private List<Field> fields;
        private HashSet<long> visited;
        private HashSet<long> reclosers;
        private Stack<long> stack;
        private Dictionary<long, ITopologyElement> elements;
        private Dictionary<long, List<long>> connections;
        #endregion

        public ITopology CreateGraphTopology(long firstElementGid)
        { 
            elements = Provider.Instance.ModelProvider.GetElementModels();
            connections = Provider.Instance.ModelProvider.GetConnections();
            reclosers = Provider.Instance.ModelProvider.GetReclosers();
            
            logger.LogInfo($"Creating topology from first element with GID 0x{firstElementGid.ToString("X16")}.");

            visited = new HashSet<long>();
            stack = new Stack<long>();
            fields = new List<Field>();

            ITopology topology = new TopologyModel
            {
                FirstNode = firstElementGid
            };

            if (elements.ContainsKey(firstElementGid))
            {
                elements[firstElementGid].IsActive = true;
            }
            else
            {
                logger.LogFatal($"Failed to build topology.Topology elements do not contain element with GID 0x{firstElementGid.ToString("X16")}which appear to be first element of topology.");
            }

            stack.Push(firstElementGid);
            ITopologyElement currentElement;
            while (stack.Count > 0)
            {
                var currentElementId = stack.Pop();
                if (!visited.Contains(currentElementId))
                {
                    visited.Add(currentElementId);
                }

                if (!elements.TryGetValue(currentElementId, out currentElement))
                {
                    logger.LogFatal($"Failed to build topology.Topology elements do not contain element with GID 0x{currentElementId.ToString("X16")}.");
                }

              
                foreach (var element in GetReferencedElementsWithoutIgnorables(currentElementId))
                {
                    if (elements.ContainsKey(element))
                    {
                        if (!reclosers.Contains(element))
                        {
                            ConnectTwoNodes(element, currentElement);
                            stack.Push(element);
                        }
                        else if (elements.TryGetValue(element, out ITopologyElement newNode))
                        {
                            currentElement.SecondEnd.Add(newNode);
                            if (newNode.FirstEnd == null)
                            {
                                newNode.FirstEnd = currentElement;
                            }
                            else
                            {
                                newNode.SecondEnd.Add(currentElement);
                            }

                            if (!topology.TopologyElements.ContainsKey(newNode.Id))
                            {
                                topology.AddElement(newNode);
                            }
                        }
                        else
                        {
                            logger.LogWarn($"[GraphBuilder] Recloser with GID 0x{element.ToString("X16")} does not exist in collection of elements.");
                        }
                    }
                    else
                    {
                        logger.LogError($"[GraphBuilder] Element with GID 0x{element.ToString("X16")} does not exist in collection of elements.");
                    }
                }
                
                topology.AddElement(currentElement);
            }

            foreach (var field in fields)
            {
                topology.AddElement(field);
            }

            logger.LogInfo("Topology successfully created.");
            return topology;
        }

        #region HelperFunctions
        private List<long> GetReferencedElementsWithoutIgnorables(long gid)
        {
            List<long> refElements = new List<long>();
            if (connections.TryGetValue(gid, out List<long> list))
            {
                list = list.Where(e => !visited.Contains(e)).ToList();
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
            }
            else
            {
                logger.LogWarn($"[GraphBuilder] Failed to get connected elements for element with GID 0x{gid.ToString("X16")}.");
            }
            return refElements;
        }
        private void ConnectTwoNodes(long newElementGid, ITopologyElement parent)
        {
            bool newElementIsField = TopologyHelper.Instance.GetElementTopologyStatus(newElementGid) == TopologyStatus.Field;
            bool parentElementIsField = TopologyHelper.Instance.GetElementTopologyStatus(parent.Id) == TopologyStatus.Field;

            if (elements.TryGetValue(newElementGid, out ITopologyElement newNode))
            {
                if (newElementIsField && !parentElementIsField)
                {
                    var field = new Field(newNode);
                    field.FirstEnd = parent;
                    newNode.FirstEnd = parent;
                    fields.Add(field);
                    parent.SecondEnd.Add(field);
                }
                else if (newElementIsField && parentElementIsField)
                {
                    try
                    {
                        GetField(parent.Id).Members.Add(newNode);
                        newNode.FirstEnd = parent;
                        parent.SecondEnd.Add(newNode);
                    }
                    catch (Exception)
                    {
                        string message = $"Element with GID 0x{parent.Id.ToString("X16")} has no field.";
                        logger.LogDebug(message);
                        throw new Exception(message);
                    }

                }
                else if (!newElementIsField && parentElementIsField)
                {
                    var field = GetField(parent.Id);
                    if (field == null)
                    {
                        string message = $"Element with GID 0x{parent.Id.ToString("X16")} has no field.";
                        logger.LogDebug(message);
                        throw new Exception(message);
                    }
                    else
                    {
                        field.SecondEnd.Add(newNode);
                        parent.SecondEnd.Add(newNode);
                        newNode.FirstEnd = field;
                    }
                }
                else
                {
                    newNode.FirstEnd = parent;
                    parent.SecondEnd.Add(newNode);
                }
            }
            else
            {
                logger.LogError($"[GraphBuilder] Element with GID 0x{newElementGid.ToString("X16")} does not exist in collection of elements.");
            }
        }
        private Field GetField(long memberGid)
        {
            Field field = null;
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Members.Where(e => e.Id == memberGid).ToList().Count > 0)
                {
                    return fields[i];
                }
            }
            return field;
        }
        #endregion
    }
}
