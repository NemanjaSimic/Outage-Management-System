using CECommon;
using CECommon.Interfaces;
using NetworkModelServiceFunctions;
using Outage.Common;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TopologyBuilder;

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
                        if (ModelCodeHelper.ExtractTypeFromGlobalId(child) == 0)
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
                    List<UIMeasurement> measurements = new List<UIMeasurement>();
                    foreach (var meas in element.Measurements)
                    {
                        measurements.Add(new UIMeasurement()
                        {
                            Gid = meas.Id,
                            Type = meas.GetMeasurementType(),
                            Value = meas.GetCurrentVaule()
                        });
                    }

                    UINode newUINode = new UINode()
                    {
                        Id = element.Id,
                        Name = element.Name,
                        Mrid = element.Mrid,
                        Description = element.Description,
                        DMSType = element.DmsType,
                        NominalVoltage = element.NominalVoltage,
                        Measurements = measurements,
                        IsActive = element.IsActive,
                        IsRemote = element.IsRemote
                    };
                    uIModel.AddNode(newUINode);
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
