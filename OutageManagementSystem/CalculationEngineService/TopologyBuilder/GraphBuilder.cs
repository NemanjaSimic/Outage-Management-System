using CECommon;
using CECommon.Interfaces;
using CECommon.Model;
using NetworkModelServiceFunctions;
using Outage.Common;
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
        private Stack<TopologyElement> stack;
        #endregion

        public TopologyModel CreateGraphTopology(long firstElementGid, TransactionFlag flag)
        {
            string message = $"Creating graph topology from first element with GID {firstElementGid}.";
            logger.LogInfo(message);

            visited = new HashSet<long>();
            stack = new Stack<TopologyElement>();
            fields = new List<Field>();

            TopologyModel topology = new TopologyModel();
            TopologyElement firstNode = topologyElementFactory.CreateTopologyElement(firstElementGid);

            stack.Push(firstNode);

            while (stack.Count > 0)
            {
                var currentNode = stack.Pop();
                if (!visited.Contains(currentNode.Id))
                {
                    visited.Add(currentNode.Id);
                }

                var connectedElements = CheckIgnorable(currentNode.Id, flag);
                foreach (var element in connectedElements)
                {
                    var newNode = ConnectTwoNodes(element, currentNode);
                    topology.AddRelation(currentNode.Id, newNode.Id);
                    stack.Push(newNode);
                }
                currentNode.DmsType = TopologyHelper.Instance.GetDMSTypeOfTopologyElement(currentNode.Id);
                topology.AddElement(currentNode);
            }
            topology.FirstNode = firstNode;

            message = $"Topology graph created.";
            logger.LogInfo(message);
            return topology;
        }

        #region HelperFunctions
        private List<long> CheckIgnorable(long gid, TransactionFlag flag)
        {
            var list = NMSManager.Instance.GetAllReferencedElements(gid, flag).Where(e => !visited.Contains(e)).ToList();
            List<long> elements = new List<long>();
            foreach (var element in list)
            {
                if (TopologyHelper.Instance.GetElementTopologyStatus(element) == TopologyStatus.Ignorable)
                {
                    visited.Add(element);
                    elements.AddRange(CheckIgnorable(element, flag));
                }
                else
                {
                    elements.Add(element);
                }
            }
            return elements;
        }
        private TopologyElement ConnectTwoNodes(long newElementGid, TopologyElement parent)
        {
            bool newElementIsField = TopologyHelper.Instance.GetElementTopologyStatus(newElementGid) == TopologyStatus.Field;
            bool parentElementIsField = TopologyHelper.Instance.GetElementTopologyStatus(parent.Id) == TopologyStatus.Field;

            TopologyElement newNode = topologyElementFactory.CreateTopologyElement(newElementGid);

            if (newElementIsField && !parentElementIsField)
            {
                var field = new Field(newNode);
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
                    field.SecondEnd.Add(newNode);
                    newNode.FirstEnd = field;
                }
            }
            else
            {
                newNode.FirstEnd = parent;
                parent.SecondEnd.Add(newNode);
            }
            return newNode;
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
