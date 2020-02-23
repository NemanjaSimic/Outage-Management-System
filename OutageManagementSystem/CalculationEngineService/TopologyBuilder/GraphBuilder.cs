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

            stack.Push(firstElementGid);
            ITopologyElement currentElement;
            long currentElementId = 0;

            while (stack.Count > 0)
            {
                currentElementId = stack.Pop();
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
                    if (elements.TryGetValue(element, out ITopologyElement newNode))
                    {
                        if (!reclosers.Contains(element))
                        {
                            ConnectTwoNodes(newNode, currentElement);
                            stack.Push(element);
                        }
                        else
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
                    DMSType elementType = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(element);
                    if(TopologyHelper.Instance.GetElementTopologyStatus(element) == TopologyStatus.Ignorable)
                    {
                        visited.Add(element);
                        refElements.AddRange(GetReferencedElementsWithoutIgnorables(element));
                    }
                    else if(elementType != DMSType.DISCRETE && elementType != DMSType.ANALOG && elementType != DMSType.BASEVOLTAGE)
                    {
                        refElements.Add(element);
                    }
                }
            }
            else
            {
                logger.LogDebug($"[GraphBuilder] Failed to get connected elements for element with GID 0x{gid.ToString("X16")}.");
            }
            return refElements;
        }
        private void ConnectTwoNodes(ITopologyElement newNode, ITopologyElement parent)
        {
            bool newElementIsField = TopologyHelper.Instance.GetElementTopologyStatus(newNode.Id) == TopologyStatus.Field;
            bool parentElementIsField = TopologyHelper.Instance.GetElementTopologyStatus(parent.Id) == TopologyStatus.Field;

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
