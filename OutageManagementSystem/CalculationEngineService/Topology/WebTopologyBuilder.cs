using CECommon;
using CECommon.Interfaces;
using Outage.Common;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopologyElementsFuntions;

namespace Topology
{
    public class WebTopologyBuilder : IWebTopologyBuilder
    {
        ILogger logger = LoggerWrapper.Instance;
        public UIModel CreateTopologyForWeb(ITopology topology)
        {
            logger.LogDebug("Web topology builder started.");
            UIModel uIModel = new UIModel();
            Stack<long> stack = new Stack<long>();
            uIModel.FirstNode = topology.FirstNode;
            stack.Push(topology.FirstNode);

            while (stack.Count > 0)
            {
                if (topology.GetElementByGid(stack.Pop(), out ITopologyElement element))
                {
                    foreach (long child in element.SecondEnd)
                    {
                        long nextElement = child;
                        if (TopologyHelper.Instance.GetDMSTypeOfTopologyElement(child).Equals("FIELD"))
                        {
                            try
                            {
                                topology.GetElementByGid(child, out ITopologyElement fieldElement);
                                Field field = fieldElement as Field;
                                nextElement = field.Members.First();
                            }
                            catch (Exception)
                            {
                                logger.LogError($"Error while getting field in WebTopologyBuilder. Element is not field or field is empty.");
                            }
                        }
                        uIModel.AddRelation(element.Id, nextElement);
                        stack.Push(nextElement);
                    }
                    uIModel.AddNode(new UINode(element.Id, element.DmsType,element.NominalVoltage, element.GetMeasurementType(), element.GetMeasurementValue(), element.IsActive, element.IsRemote));
                }
                else
                {
                    logger.LogWarn($"Error while getting topology element in WebTopologyBuilder.");
                }
            }
            logger.LogDebug("Web topology builder finished.");
            return uIModel;
        }
    }
}
